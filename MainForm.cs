using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace YtDlpGui
{
    /// <summary>
    /// Formulário principal: barra lateral de navegação (Home, Histórico,
    /// Pasta, Configurações) + barra de status inferior com progresso.
    /// Não exibe a saída bruta do yt-dlp (sem "terminal") — tudo é traduzido
    /// para mensagens curtas e amigáveis, e o download avisa via notificação
    /// do Windows quando termina ou falha.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly string _ytDlpPath;
        private readonly string _ffmpegPath;
        private readonly string _ffmpegDir;
        private readonly string _pastaAssets;

        private Process? _processoAtual;
        private bool _canceladoPeloUsuario;
        private string? _ultimoArquivoBaixado;
        private readonly System.Collections.Generic.List<string> _arquivosBaixadosNestaSessao = new();
        private string? _nomePlaylistAtual;
        private int _itemAtualPlaylist;
        private int _totalItensPlaylist;
        private string? _ultimoErro;
        private bool _avisoBotMostrado;

        // Campos da prévia de vídeo (Home)
        private System.Threading.CancellationTokenSource? _ctsPreviaCts;
        private readonly System.Net.Http.HttpClient _httpClient = new();
        private System.Windows.Forms.Timer? _timerSpinner;
        private int _spinnerFrame;
        private static readonly string[] SpinnerFrames = { "⠋", "⠙", "⠸", "⠴", "⠦", "⠇" };

        private static readonly Regex RegexProgresso =
            new Regex(@"\[download\]\s+(\d{1,3}(?:\.\d+)?)%", RegexOptions.Compiled);
        private static readonly Regex RegexDestino =
            new Regex(@"Destination:\s*(.+)$", RegexOptions.Compiled);
        private static readonly Regex RegexErro =
            new Regex(@"^ERROR:\s*(.+)$", RegexOptions.Compiled);
        private static readonly Regex RegexPlaylistNome =
            new Regex(@"Downloading playlist:\s*(.+)$", RegexOptions.Compiled);
        private static readonly Regex RegexPlaylistItem =
            new Regex(@"Downloading item (\d+) of (\d+)", RegexOptions.Compiled);

        public MainForm()
        {
            InitializeComponent();

            string pastaApp = AppDomain.CurrentDomain.BaseDirectory;
            _ytDlpPath = Path.Combine(pastaApp, "yt-dlp.exe");
            _ffmpegPath = Path.Combine(pastaApp, "ffmpeg.exe");
            _ffmpegDir = pastaApp;
            _pastaAssets = Path.Combine(pastaApp, "Assets");

            txtSavePath.Text = ObterPastaDownloadsPadrao();
            cmbNavegador.SelectedIndex = 0; // "chrome"

            CarregarIconeDaJanela();
            CarregarImagens();
            ConfigurarNotificacoes();
            MostrarPagina("home");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            VerificarDependenciasSilenciosamente();
        }

        // ---------------------------------------------------------------
        // Configuração inicial (ícone, imagens, notificações)
        // ---------------------------------------------------------------

        private void CarregarIconeDaJanela()
        {
            try
            {
                string caminhoIcone = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                if (File.Exists(caminhoIcone))
                {
                    Icon = new System.Drawing.Icon(caminhoIcone);
                }
            }
            catch
            {
                // Ícone é apenas estético.
            }
        }

        /// <summary>Carrega uma imagem da pasta Assets, ou null se não existir/falhar.</summary>
        private System.Drawing.Image? CarregarImagem(string nomeArquivo)
        {
            try
            {
                string caminho = Path.Combine(_pastaAssets, nomeArquivo);
                return File.Exists(caminho) ? System.Drawing.Image.FromFile(caminho) : null;
            }
            catch
            {
                return null;
            }
        }

        private void CarregarImagens()
        {
            picLogo.Image = CarregarImagem("logo_transparente.png");
            picLogoGrande.Image = CarregarImagem("logo_icone.png");
            picIlustracaoPasta.Image = CarregarImagem("ilustracao_pasta.png");
            picLogoConfig.Image = CarregarImagem("logo_icone.png");

            AtualizarIconesDeNavegacao("home");
        }

        // ---------------------------------------------------------------
        // Prévia de vídeo (Home)
        // ---------------------------------------------------------------

        /// <summary>
        /// Disparado pelo evento TextChanged do campo de URL (debounced via
        /// cancellation: só roda de fato se o usuário parar de digitar por
        /// 600ms, evitando chamadas ao yt-dlp a cada tecla).
        /// </summary>
        private async void txtUrl_TextChanged(object sender, EventArgs e)
        {
            // Cancela qualquer busca anterior ainda em andamento
            _ctsPreviaCts?.Cancel();
            _ctsPreviaCts = new CancellationTokenSource();
            var token = _ctsPreviaCts.Token;

            string url = txtUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                MostrarLogoPadrao();
                return;
            }

            // Debounce: aguarda 600ms antes de consultar o yt-dlp
            try { await Task.Delay(600, token); }
            catch (TaskCanceledException) { return; }

            if (token.IsCancellationRequested) return;

            IniciarSpinner();

            try
            {
                var previa = await BuscarPreviaAsync(url, token);
                if (token.IsCancellationRequested) return;

                if (previa is not null)
                    ExibirPrevia(previa, token);
                else
                    MostrarLogoPadrao();
            }
            catch (TaskCanceledException) { }
            catch { MostrarLogoPadrao(); }
            finally { PararSpinner(); }
        }

        /// <summary>
        /// Roda yt-dlp --dump-json (sem baixar) e extrai título, canal e
        /// URL da thumbnail do vídeo.
        /// </summary>
        private async Task<PreviaVideo?> BuscarPreviaAsync(string url, CancellationToken token)
        {
            if (!File.Exists(_ytDlpPath)) return null;

            var psi = new ProcessStartInfo
            {
                FileName = _ytDlpPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
            };
            psi.ArgumentList.Add("--dump-json");
            psi.ArgumentList.Add("--no-warnings");
            psi.ArgumentList.Add("--skip-download");
            psi.ArgumentList.Add("--no-playlist");
            psi.ArgumentList.Add(url);

            using var proc = Process.Start(psi);
            if (proc is null) return null;

            string saida = await proc.StandardOutput.ReadToEndAsync();
            await proc.WaitForExitAsync(token);

            if (proc.ExitCode != 0 || string.IsNullOrWhiteSpace(saida))
                return null;

            using var doc = JsonDocument.Parse(saida);
            var root = doc.RootElement;

            string titulo = root.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
            string canal = root.TryGetProperty("channel", out var c) ? c.GetString() ?? "" : "";
            string thumbUrl = root.TryGetProperty("thumbnail", out var th) ? th.GetString() ?? "" : "";
            string duracao = root.TryGetProperty("duration_string", out var d) ? d.GetString() ?? "" : "";

            if (string.IsNullOrEmpty(titulo)) return null;

            Image? thumbImg = null;
            if (!string.IsNullOrEmpty(thumbUrl))
            {
                try
                {
                    byte[] bytes = await _httpClient.GetByteArrayAsync(thumbUrl, token);
                    using var ms = new MemoryStream(bytes);
                    thumbImg = Image.FromStream(ms);
                }
                catch { /* thumbnail é opcional */ }
            }

            return new PreviaVideo(titulo, canal, duracao, thumbImg);
        }

        private record PreviaVideo(string Titulo, string Canal, string Duracao, Image? Thumbnail);

        /// <summary>Exibe os dados da prévia no card da Home.</summary>
        private void ExibirPrevia(PreviaVideo previa, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            picLogoGrande.Visible = false;
            pnlPrevia.Visible = true;

            if (previa.Thumbnail is not null)
                picThumb.Image = previa.Thumbnail;

            lblTituloVideo.Text = previa.Titulo.Length > 60
                ? previa.Titulo[..57] + "..."
                : previa.Titulo;

            string info = previa.Canal;
            if (!string.IsNullOrEmpty(previa.Duracao))
                info += (info.Length > 0 ? " • " : "") + previa.Duracao;

            lblInfoVideo.Text = info;
        }

        /// <summary>Volta a exibir o logo padrão e esconde o painel de prévia.</summary>
        private void MostrarLogoPadrao()
        {
            if (InvokeRequired) { BeginInvoke(MostrarLogoPadrao); return; }
            pnlPrevia.Visible = false;
            picLogoGrande.Visible = true;
            picLogoGrande.Image = CarregarImagem("logo_icone.png");
        }

        private void IniciarSpinner()
        {
            if (InvokeRequired) { BeginInvoke(IniciarSpinner); return; }
            picLogoGrande.Visible = false;
            pnlPrevia.Visible = false;
            lblSpinner.Visible = true;
            _spinnerFrame = 0;
            _timerSpinner ??= new System.Windows.Forms.Timer { Interval = 100 };
            _timerSpinner.Tick -= SpinnerTick;
            _timerSpinner.Tick += SpinnerTick;
            _timerSpinner.Start();
        }

        private void PararSpinner()
        {
            if (InvokeRequired) { BeginInvoke(PararSpinner); return; }
            _timerSpinner?.Stop();
            lblSpinner.Visible = false;
        }

        private void SpinnerTick(object? sender, EventArgs e)
        {
            _spinnerFrame = (_spinnerFrame + 1) % SpinnerFrames.Length;
            lblSpinner.Text = SpinnerFrames[_spinnerFrame] + " Buscando informações...";
        }


        private void AtualizarIconesDeNavegacao(string paginaAtiva)
        {
            picNavHistorico.Image = CarregarImagem(paginaAtiva == "historico" ? "nav_historico_azul.png" : "nav_historico_branco.png");
            picNavPasta.Image = CarregarImagem(paginaAtiva == "pasta" ? "nav_pasta_azul.png" : "nav_pasta_branco.png");
            picNavConfig.Image = CarregarImagem(paginaAtiva == "config" ? "nav_config_azul.png" : "nav_config_branco.png");
        }

        private void ConfigurarNotificacoes()
        {
            try
            {
                notifyIcon1.Icon = Icon;
                notifyIcon1.Visible = true;
            }
            catch
            {
                // Notificações são um extra; se falhar, o app continua funcionando normalmente.
            }
        }

        private void MostrarNotificacao(string titulo, string mensagem, bool erro = false)
        {
            try
            {
                notifyIcon1.BalloonTipIcon = erro ? ToolTipIcon.Error : ToolTipIcon.Info;
                notifyIcon1.BalloonTipTitle = titulo;
                notifyIcon1.BalloonTipText = mensagem;
                notifyIcon1.ShowBalloonTip(5000);
            }
            catch
            {
                // Se a notificação falhar por qualquer motivo, apenas ignora.
            }
        }

        // ---------------------------------------------------------------
        // Navegação entre páginas
        // ---------------------------------------------------------------

        private void MostrarPagina(string pagina)
        {
            pnlHome.Visible = pagina == "home";
            pnlHistorico.Visible = pagina == "historico";
            pnlPasta.Visible = pagina == "pasta";
            pnlConfiguracoes.Visible = pagina == "config";

            // As páginas ocupam a área inteira de pnlContent (inclusive o canto
            // onde o botão Cancelar fica), então é preciso reafirmar que ele
            // continua no topo do z-order toda vez que a página é trocada.
            btnCancelar.BringToFront();

            AtualizarIconesDeNavegacao(pagina);

            if (pagina == "historico")
            {
                CarregarHistorico();
            }
        }

        private void picLogo_Click(object sender, EventArgs e) => MostrarPagina("home");
        private void picNavHistorico_Click(object sender, EventArgs e) => MostrarPagina("historico");
        private void picNavPasta_Click(object sender, EventArgs e) => MostrarPagina("pasta");
        private void picNavConfig_Click(object sender, EventArgs e) => MostrarPagina("config");

        // ---------------------------------------------------------------
        // Página Pasta (local de salvamento)
        // ---------------------------------------------------------------

        private static string ObterPastaDownloadsPadrao()
        {
            try
            {
                string perfil = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string downloads = Path.Combine(perfil, "Downloads");
                return Directory.Exists(downloads)
                    ? downloads
                    : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
            catch
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
        }

        private void btnAlterarLocal_Click(object sender, EventArgs e)
        {
            using var dialogo = new FolderBrowserDialog
            {
                Description = "Selecione a pasta onde o arquivo será salvo",
                UseDescriptionForTitle = true,
                SelectedPath = Directory.Exists(txtSavePath.Text) ? txtSavePath.Text : ObterPastaDownloadsPadrao()
            };

            if (dialogo.ShowDialog(this) == DialogResult.OK)
            {
                txtSavePath.Text = dialogo.SelectedPath;
            }
        }

        // ---------------------------------------------------------------
        // Página Home (downloads)
        // ---------------------------------------------------------------

        private async void btnDownloadMp4_Click(object sender, EventArgs e)
        {
            if (!ValidarEntradas(exigeFfmpeg: false))
                return;

            // ---- MÓDULO OPCIONAL: Filtro de Conteúdo (ver FiltroConteudo.cs) ----
            // Sem "filtro-conteudo.json" na pasta do app, isto não faz nada.
            if (!await PassaPeloFiltroDeConteudoAsync())
                return;
            // -----------------------------------------------------------------

            var argumentos = new System.Collections.Generic.List<string>
            {
                txtUrl.Text.Trim(),
                "-f", "bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/bestvideo+bestaudio/best",
                "--merge-output-format", "mp4",
                "--newline",
                "-P", txtSavePath.Text,
                "-o", ObterTemplateDeSaida()
            };

            AdicionarArgumentoDePlaylist(argumentos);
            AdicionarArgumentosDeAutenticacao(argumentos);

            if (File.Exists(_ffmpegPath))
            {
                argumentos.Add("--ffmpeg-location");
                argumentos.Add(_ffmpegDir);
            }

            await ExecutarYtDlpAsync(argumentos, "MP4");
        }

        private async void btnDownloadMp3_Click(object sender, EventArgs e)
        {
            if (!ValidarEntradas(exigeFfmpeg: true))
                return;

            // ---- MÓDULO OPCIONAL: Filtro de Conteúdo (ver FiltroConteudo.cs) ----
            if (!await PassaPeloFiltroDeConteudoAsync())
                return;
            // -----------------------------------------------------------------

            var argumentos = new System.Collections.Generic.List<string>
            {
                txtUrl.Text.Trim(),
                "-x",
                "--audio-format", "mp3",
                "--newline",
                "-P", txtSavePath.Text,
                "-o", ObterTemplateDeSaida()
            };

            AdicionarArgumentoDePlaylist(argumentos);
            AdicionarArgumentosDeAutenticacao(argumentos);

            if (File.Exists(_ffmpegPath))
            {
                argumentos.Add("--ffmpeg-location");
                argumentos.Add(_ffmpegDir);
            }

            await ExecutarYtDlpAsync(argumentos, "MP3");
        }

        // =================================================================
        // MÓDULO OPCIONAL: integração com FiltroConteudo.cs
        //
        // Este bloco inteiro pode ser removido (junto das duas chamadas nos
        // handlers de download acima) sem afetar mais nada no app — é
        // exatamente por isso que a lógica de fato vive num arquivo à parte.
        // Sem "filtro-conteudo.json" na pasta do .exe, CarregarConfiguracao()
        // retorna Habilitado=false e este método sempre retorna true de
        // imediato (nenhum custo extra, nenhuma chamada ao yt-dlp).
        // =================================================================

        /// <summary>
        /// Retorna true se o download pode prosseguir (filtro desligado,
        /// URL permitida, ou código mestre digitado corretamente). Retorna
        /// false se o usuário cancelou ou não sabia o código.
        /// </summary>
        private async System.Threading.Tasks.Task<bool> PassaPeloFiltroDeConteudoAsync()
        {
            var configFiltro = FiltroConteudo.CarregarConfiguracao();
            if (!configFiltro.Habilitado)
                return true;

            AtualizarStatus("Verificando conteúdo...");
            var resultado = await FiltroConteudo.VerificarAsync(txtUrl.Text.Trim(), _ytDlpPath, configFiltro);

            if (resultado.Permitido)
            {
                AtualizarStatus("Pronto para uso.");
                return true;
            }

            RegistrarLog($"Filtro de conteúdo bloqueou: {resultado.Motivo} | URL: {txtUrl.Text.Trim()}");
            AtualizarStatus("Bloqueado pelo filtro de conteúdo.");

            bool liberadoPeloCodigo = FiltroConteudo.SolicitarCodigoMestre(this, configFiltro, resultado.Motivo ?? "Conteúdo bloqueado.");

            if (liberadoPeloCodigo)
            {
                RegistrarLog("Bloqueio liberado com o código mestre.");
                AtualizarStatus("Liberado pelo código mestre.");
                return true;
            }

            AtualizarStatus("Download não iniciado (bloqueado pelo filtro de conteúdo).");
            return false;
        }

        private void AdicionarArgumentoDePlaylist(System.Collections.Generic.List<string> argumentos)
        {
            argumentos.Add(chkPlaylist.Checked ? "--yes-playlist" : "--no-playlist");
        }

        /// <summary>
        /// Define onde/como os arquivos são nomeados e salvos:
        /// - Vídeo único: sempre solto direto na pasta escolhida.
        /// - Playlist + "organizar em subpastas" marcado: cria uma subpasta
        ///   com o nome da playlist dentro da pasta escolhida, e cada vídeo
        ///   vai lá dentro.
        /// - Playlist sem a opção marcada: todos os vídeos soltos direto na
        ///   pasta escolhida (comportamento antigo).
        /// </summary>
        private string ObterTemplateDeSaida()
        {
            if (chkPlaylist.Checked && chkOrganizarPlaylist.Checked)
            {
                return "%(playlist_title)s/%(title)s.%(ext)s";
            }

            return "%(title)s.%(ext)s";
        }

        private void AdicionarArgumentosDeAutenticacao(System.Collections.Generic.List<string> argumentos)
        {
            if (radCookiesNavegador.Checked)
            {
                if (cmbNavegador.SelectedItem is string navegador && !string.IsNullOrWhiteSpace(navegador))
                {
                    argumentos.Add("--cookies-from-browser");
                    argumentos.Add(navegador);
                }
            }
            else if (radCookiesArquivo.Checked)
            {
                if (!string.IsNullOrWhiteSpace(txtCookiesArquivo.Text) && File.Exists(txtCookiesArquivo.Text))
                {
                    argumentos.Add("--cookies");
                    argumentos.Add(txtCookiesArquivo.Text);
                }
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (_processoAtual is null || _processoAtual.HasExited)
                return;

            try
            {
                _canceladoPeloUsuario = true;
                AtualizarStatus("Cancelando...");
                _processoAtual.Kill(entireProcessTree: true);
            }
            catch (Exception ex)
            {
                AtualizarStatus("Não foi possível cancelar.");
                RegistrarLog("Erro ao cancelar: " + ex.Message);
            }
        }

        private bool ValidarEntradas(bool exigeFfmpeg)
        {
            if (string.IsNullOrWhiteSpace(txtUrl.Text))
            {
                MessageBox.Show(this, "Cole a URL do vídeo antes de continuar.",
                    "URL não informada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                MostrarPagina("home");
                txtUrl.Focus();
                return false;
            }

            if (!Uri.TryCreate(txtUrl.Text.Trim(), UriKind.Absolute, out _))
            {
                MessageBox.Show(this, "A URL informada não parece ser válida.",
                    "URL inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                MostrarPagina("home");
                txtUrl.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtSavePath.Text) || !Directory.Exists(txtSavePath.Text))
            {
                MessageBox.Show(this, "Escolha uma pasta de destino válida na tela \"Pasta\".",
                    "Pasta inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                MostrarPagina("pasta");
                return false;
            }

            if (!File.Exists(_ytDlpPath))
            {
                MessageBox.Show(this,
                    $"Não foi possível encontrar o yt-dlp.exe em:\n{_ytDlpPath}\n\n" +
                    "Coloque o arquivo yt-dlp.exe na mesma pasta deste programa.",
                    "yt-dlp.exe não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!File.Exists(_ffmpegPath))
            {
                if (exigeFfmpeg)
                {
                    MessageBox.Show(this,
                        "O ffmpeg.exe é obrigatório para baixar em MP3 e não foi encontrado.\n\n" +
                        "Coloque o arquivo ffmpeg.exe na mesma pasta deste programa.",
                        "ffmpeg.exe não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                var resultado = MessageBox.Show(this,
                    "O ffmpeg.exe não foi encontrado. Alguns vídeos podem baixar em qualidade " +
                    "reduzida sem ele. Deseja continuar mesmo assim?",
                    "ffmpeg.exe não encontrado", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (resultado != DialogResult.Yes)
                    return false;
            }

            if (radCookiesArquivo.Checked &&
                (string.IsNullOrWhiteSpace(txtCookiesArquivo.Text) || !File.Exists(txtCookiesArquivo.Text)))
            {
                MessageBox.Show(this,
                    "Selecione um arquivo cookies.txt válido na tela \"Configurações\".",
                    "Arquivo de cookies inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                MostrarPagina("config");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Executa o yt-dlp.exe de forma assíncrona e oculta. Não expõe a
        /// saída bruta ao usuário — apenas um status curto + porcentagem.
        /// Toda a saída é gravada silenciosamente em log.txt para suporte.
        /// </summary>
        private async System.Threading.Tasks.Task ExecutarYtDlpAsync(
            System.Collections.Generic.List<string> argumentos, string tipoParaHistorico)
        {
            _canceladoPeloUsuario = false;
            _ultimoArquivoBaixado = null;
            _arquivosBaixadosNestaSessao.Clear();
            _nomePlaylistAtual = null;
            _itemAtualPlaylist = 0;
            _totalItensPlaylist = 0;
            _ultimoErro = null;
            _avisoBotMostrado = false;

            DefinirEstadoDownloadEmAndamento(true);
            AtualizarProgresso(0);
            AtualizarStatus("Iniciando download...");
            RegistrarLog($"===== Início: {tipoParaHistorico} | {txtUrl.Text.Trim()} =====");

            var psi = new ProcessStartInfo
            {
                FileName = _ytDlpPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            foreach (var arg in argumentos)
                psi.ArgumentList.Add(arg);

            using var processo = new Process { StartInfo = psi, EnableRaisingEvents = true };
            processo.OutputDataReceived += (s, e) => ProcessarLinhaDeSaida(e.Data);
            processo.ErrorDataReceived += (s, e) => ProcessarLinhaDeSaida(e.Data);

            try
            {
                _processoAtual = processo;
                processo.Start();
                processo.BeginOutputReadLine();
                processo.BeginErrorReadLine();

                await processo.WaitForExitAsync();

                if (_canceladoPeloUsuario)
                {
                    AtualizarStatus("Download cancelado.");
                    if (string.IsNullOrEmpty(tipoParaHistorico))
                        lblAtualizarStatus.Text = "Cancelado.";
                }
                else if (processo.ExitCode == 0)
                {
                    if (string.IsNullOrEmpty(tipoParaHistorico))
                    {
                        // Ação sem histórico associado (ex.: "yt-dlp -U").
                        AtualizarStatus("Atualização concluída!");
                        lblAtualizarStatus.Text = "Tudo em dia!";
                        MostrarNotificacao("yt-dlp atualizado", "O yt-dlp foi verificado/atualizado com sucesso.");
                    }
                    else
                    {
                        AtualizarStatus("Download concluído!");
                        AtualizarProgresso(100);

                        string nomeParaNotificacao = RegistrarHistoricoDoDownload(tipoParaHistorico);
                        MostrarNotificacao("Download concluído", nomeParaNotificacao);
                    }
                }
                else
                {
                    string mensagem = TraduzirErro(_ultimoErro);
                    AtualizarStatus("Falha no download.");
                    if (string.IsNullOrEmpty(tipoParaHistorico))
                        lblAtualizarStatus.Text = "Falha ao atualizar.";
                    MostrarNotificacao("Falha no download", mensagem, erro: true);
                    MessageBox.Show(this, mensagem, "Falha no download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (System.ComponentModel.Win32Exception exWin32)
            {
                AtualizarStatus("Erro ao iniciar o yt-dlp.");
                RegistrarLog("Erro Win32: " + exWin32.Message);
                MessageBox.Show(this, "Não foi possível iniciar o yt-dlp.exe: " + exWin32.Message,
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                AtualizarStatus("Erro inesperado.");
                RegistrarLog("Erro inesperado: " + ex.Message);
            }
            finally
            {
                _processoAtual = null;
                DefinirEstadoDownloadEmAndamento(false);
            }
        }

        /// <summary>
        /// Registra o(s) item(ns) desse download no histórico e retorna um
        /// texto curto para usar na notificação do Windows.
        /// - Vídeo único: um item, como sempre foi.
        /// - Playlist + "organizar em subpastas": UM item representando a
        ///   playlist inteira (nome + quantidade de vídeos), apontando para
        ///   a subpasta criada.
        /// - Playlist sem essa opção: um item para CADA vídeo baixado
        ///   (antes, só o último vídeo da playlist entrava no histórico).
        /// </summary>
        private string RegistrarHistoricoDoDownload(string tipoParaHistorico)
        {
            bool ehPlaylist = chkPlaylist.Checked;
            var agora = DateTime.Now;

            if (!ehPlaylist)
            {
                string nomeArquivo = string.IsNullOrEmpty(_ultimoArquivoBaixado)
                    ? "(nome não identificado)"
                    : Path.GetFileName(_ultimoArquivoBaixado);

                GerenciadorHistorico.Adicionar(new HistoricoItem
                {
                    DataHora = agora,
                    Tipo = tipoParaHistorico,
                    Titulo = nomeArquivo,
                    Pasta = txtSavePath.Text,
                    Url = txtUrl.Text.Trim()
                });

                return nomeArquivo;
            }

            if (chkOrganizarPlaylist.Checked)
            {
                string nomePlaylist = string.IsNullOrWhiteSpace(_nomePlaylistAtual)
                    ? "Playlist"
                    : _nomePlaylistAtual!;
                string pastaPlaylist = Path.Combine(txtSavePath.Text, nomePlaylist);
                string titulo = $"{nomePlaylist} ({_arquivosBaixadosNestaSessao.Count} vídeos)";

                GerenciadorHistorico.Adicionar(new HistoricoItem
                {
                    DataHora = agora,
                    Tipo = tipoParaHistorico,
                    Titulo = titulo,
                    Pasta = pastaPlaylist,
                    Url = txtUrl.Text.Trim()
                });

                return titulo;
            }

            // Playlist sem agrupamento: um item de histórico por vídeo baixado.
            if (_arquivosBaixadosNestaSessao.Count == 0)
            {
                // Fallback de segurança, caso nenhuma linha "Destination:" tenha sido capturada.
                GerenciadorHistorico.Adicionar(new HistoricoItem
                {
                    DataHora = agora,
                    Tipo = tipoParaHistorico,
                    Titulo = "(itens da playlist não identificados)",
                    Pasta = txtSavePath.Text,
                    Url = txtUrl.Text.Trim()
                });
                return "Playlist concluída";
            }

            foreach (var caminho in _arquivosBaixadosNestaSessao)
            {
                GerenciadorHistorico.Adicionar(new HistoricoItem
                {
                    DataHora = agora,
                    Tipo = tipoParaHistorico,
                    Titulo = Path.GetFileName(caminho),
                    Pasta = txtSavePath.Text,
                    Url = txtUrl.Text.Trim()
                });
            }

            return $"Playlist: {_arquivosBaixadosNestaSessao.Count} vídeos baixados";
        }


        private static string TraduzirErro(string? erroBruto)
        {
            if (string.IsNullOrWhiteSpace(erroBruto))
                return "Ocorreu um erro durante o download. Tente novamente.";

            if (erroBruto.Contains("Sign in to confirm") && erroBruto.Contains("not a bot"))
            {
                return "O YouTube pediu confirmação de que você não é um robô. " +
                       "Vá em \"Configurações\" e ative os cookies do navegador ou um arquivo cookies.txt.";
            }

            if (erroBruto.Contains("ffmpeg"))
            {
                return "Houve um problema relacionado ao ffmpeg. Verifique se o ffmpeg.exe " +
                       "está na mesma pasta do programa.";
            }

            if (erroBruto.Contains("Video unavailable") || erroBruto.Contains("This video is unavailable"))
            {
                return "Este vídeo não está mais disponível ou foi removido.";
            }

            if (erroBruto.Contains("Private video"))
            {
                return "Este vídeo é privado e não pode ser baixado.";
            }

            return "Ocorreu um erro: " + erroBruto;
        }

        private void ProcessarLinhaDeSaida(string? linha)
        {
            if (string.IsNullOrWhiteSpace(linha))
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => TratarLinha(linha)));
            }
            else
            {
                TratarLinha(linha);
            }
        }

        private void TratarLinha(string linha)
        {
            RegistrarLog(linha);

            var matchProgresso = RegexProgresso.Match(linha);
            if (matchProgresso.Success && double.TryParse(
                    matchProgresso.Groups[1].Value,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double percentual))
            {
                AtualizarProgresso(percentual);

                if (chkPlaylist.Checked && _totalItensPlaylist > 0)
                {
                    AtualizarStatus($"Baixando vídeo {_itemAtualPlaylist} de {_totalItensPlaylist}... {percentual:0}%");
                }
                else
                {
                    AtualizarStatus($"Baixando... {percentual:0}%");
                }
            }

            var matchDestino = RegexDestino.Match(linha);
            if (matchDestino.Success)
            {
                string caminho = matchDestino.Groups[1].Value.Trim();
                _ultimoArquivoBaixado = caminho;

                // Evita duplicar o mesmo arquivo caso o yt-dlp imprima a linha mais de uma vez
                // (acontece, por exemplo, quando há etapas de remux/merge sobre o mesmo destino).
                if (_arquivosBaixadosNestaSessao.Count == 0 ||
                    _arquivosBaixadosNestaSessao[^1] != caminho)
                {
                    _arquivosBaixadosNestaSessao.Add(caminho);
                }
            }

            var matchPlaylistNome = RegexPlaylistNome.Match(linha);
            if (matchPlaylistNome.Success)
            {
                _nomePlaylistAtual = matchPlaylistNome.Groups[1].Value.Trim();
            }

            var matchPlaylistItem = RegexPlaylistItem.Match(linha);
            if (matchPlaylistItem.Success &&
                int.TryParse(matchPlaylistItem.Groups[1].Value, out int itemAtual) &&
                int.TryParse(matchPlaylistItem.Groups[2].Value, out int totalItens))
            {
                _itemAtualPlaylist = itemAtual;
                _totalItensPlaylist = totalItens;
                AtualizarStatus($"Baixando vídeo {itemAtual} de {totalItens}...");
            }

            if (linha.Contains("[ExtractAudio]") || linha.Contains("Extracting audio"))
                AtualizarStatus("Convertendo para MP3...");
            else if (linha.Contains("[Merger]") || linha.Contains("Merging formats"))
                AtualizarStatus("Finalizando arquivo...");
            else if (linha.Contains("Extracting URL") && !chkPlaylist.Checked)
                AtualizarStatus("Processando URL...");
            else if (linha.Contains("has already been downloaded"))
                AtualizarStatus("Arquivo já existe, pulando...");

            var matchErro = RegexErro.Match(linha);
            if (matchErro.Success)
            {
                _ultimoErro = matchErro.Groups[1].Value.Trim();
            }

            if (!_avisoBotMostrado && linha.Contains("Sign in to confirm") && linha.Contains("not a bot"))
            {
                _avisoBotMostrado = true;
            }
        }

        private void AtualizarStatus(string texto)
        {
            lblStatus.Text = texto;
        }

        private void AtualizarProgresso(double percentual)
        {
            int valor = Math.Clamp((int)Math.Round(percentual), 0, 100);
            progressBar1.Value = valor;
            lblProgressPercent.Text = $"{valor}%";
        }

        /// <summary>
        /// Grava uma linha no log de suporte (%AppData%\YtDlpGui\log.txt),
        /// sem nunca exibir isso na interface. Falhas de escrita são ignoradas.
        /// </summary>
        private static void RegistrarLog(string linha)
        {
            try
            {
                string pastaDados = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YtDlpGui");
                Directory.CreateDirectory(pastaDados);
                string caminho = Path.Combine(pastaDados, "log.txt");
                File.AppendAllText(caminho, $"[{DateTime.Now:HH:mm:ss}] {linha}{Environment.NewLine}");
            }
            catch
            {
                // Log é apenas para suporte; falha ao escrever não deve afetar o app.
            }
        }

        private void DefinirEstadoDownloadEmAndamento(bool emAndamento)
        {
            btnDownloadMp4.Enabled = !emAndamento;
            btnDownloadMp3.Enabled = !emAndamento;
            txtUrl.Enabled = !emAndamento;
            chkPlaylist.Enabled = !emAndamento;
            chkOrganizarPlaylist.Enabled = !emAndamento;
            btnAlterarLocal.Enabled = !emAndamento;
            btnAtualizarYtDlp.Enabled = !emAndamento;
            radSemCookies.Enabled = !emAndamento;
            radCookiesNavegador.Enabled = !emAndamento;
            radCookiesArquivo.Enabled = !emAndamento;
            cmbNavegador.Enabled = !emAndamento && radCookiesNavegador.Checked;
            txtCookiesArquivo.Enabled = !emAndamento && radCookiesArquivo.Checked;

            btnCancelar.Enabled = emAndamento;

            Cursor = emAndamento ? Cursors.AppStarting : Cursors.Default;
        }

        // ---------------------------------------------------------------
        // Página Configurações (autenticação + atualizar yt-dlp)
        // ---------------------------------------------------------------

        // Evita reentrância ao desmarcar os outros radios programaticamente abaixo.
        private bool _atualizandoOpcaoDeAutenticacao;

        /// <summary>
        /// O Guna2RadioButton, ao contrário do RadioButton nativo do Windows,
        /// NÃO desmarca automaticamente os "irmãos" quando um é marcado —
        /// por isso o bug relatado (marcar "cookies do navegador" e, ao
        /// voltar pra tela, aparecer "cookies.txt" marcado: os dois ficavam
        /// marcados ao mesmo tempo por baixo dos panos). Aqui forçamos a
        /// exclusividade manualmente.
        /// </summary>
        private void OpcaoDeAutenticacao_CheckedChanged(object sender, EventArgs e)
        {
            if (_atualizandoOpcaoDeAutenticacao)
                return;

            if (sender is Guna2RadioButton radioClicado && radioClicado.Checked)
            {
                _atualizandoOpcaoDeAutenticacao = true;
                if (radSemCookies != radioClicado) radSemCookies.Checked = false;
                if (radCookiesNavegador != radioClicado) radCookiesNavegador.Checked = false;
                if (radCookiesArquivo != radioClicado) radCookiesArquivo.Checked = false;
                _atualizandoOpcaoDeAutenticacao = false;
            }

            cmbNavegador.Enabled = radCookiesNavegador.Checked;
            txtCookiesArquivo.Enabled = radCookiesArquivo.Checked;
        }

        /// <summary>
        /// Abre a página oficial do yt-dlp com instruções de como exportar o
        /// arquivo cookies.txt do navegador (resolve a reclamação de que essa
        /// opção é confusa sem contexto nenhum na tela).
        /// </summary>
        private void btnAjudaCookies_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/yt-dlp/yt-dlp/wiki/Extractors#exporting-youtube-cookies",
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show(this,
                    "Não foi possível abrir o navegador. Acesse manualmente:\n" +
                    "https://github.com/yt-dlp/yt-dlp/wiki/Extractors#exporting-youtube-cookies",
                    "Ajuda", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// O campo de cookies.txt agora é clicável (só quando a opção
        /// correspondente está marcada, já que fica desabilitado caso
        /// contrário) — substitui o antigo botão "Buscar" separado, deixando
        /// a coluna mais limpa e alinhada ao restante do layout.
        /// </summary>
        private void txtCookiesArquivo_Click(object sender, EventArgs e)
        {
            using var dialogo = new OpenFileDialog
            {
                Title = "Selecione o arquivo de cookies exportado do navegador",
                Filter = "Arquivos de cookies (*.txt)|*.txt|Todos os arquivos (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialogo.ShowDialog(this) == DialogResult.OK)
            {
                txtCookiesArquivo.Text = dialogo.FileName;
            }
        }

        private async void btnAtualizarYtDlp_Click(object sender, EventArgs e)
        {
            if (!File.Exists(_ytDlpPath))
            {
                MessageBox.Show(this, $"Não foi possível encontrar o yt-dlp.exe em:\n{_ytDlpPath}",
                    "yt-dlp.exe não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lblAtualizarStatus.Text = "Verificando...";
            await ExecutarYtDlpAsync(new System.Collections.Generic.List<string> { "-U" }, tipoParaHistorico: "");
        }

        // ---------------------------------------------------------------
        // Página Histórico
        // ---------------------------------------------------------------

        private void CarregarHistorico()
        {
            listViewHistorico.Items.Clear();
            var itens = GerenciadorHistorico.Carregar();

            foreach (var item in itens)
            {
                if (string.IsNullOrEmpty(item.Tipo))
                    continue; // ignora entradas de "atualização do yt-dlp" (sem tipo)

                var linha = new ListViewItem(item.Tipo);
                linha.SubItems.Add(item.Titulo);
                linha.SubItems.Add(item.DataHora.ToString("dd/MM/yyyy HH:mm"));
                linha.Tag = item;
                listViewHistorico.Items.Add(linha);
            }

            lblHistoricoVazio.Visible = listViewHistorico.Items.Count == 0;
        }

        private void menuItemAbrirPasta_Click(object sender, EventArgs e)
        {
            if (listViewHistorico.SelectedItems.Count == 0)
                return;

            if (listViewHistorico.SelectedItems[0].Tag is HistoricoItem item)
            {
                if (!Directory.Exists(item.Pasta))
                {
                    MessageBox.Show(this, "A pasta deste item não existe mais.",
                        "Pasta não encontrada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Process.Start(new ProcessStartInfo { FileName = item.Pasta, UseShellExecute = true });
            }
        }

        private void btnLimparHistorico_Click(object sender, EventArgs e)
        {
            if (listViewHistorico.Items.Count == 0)
                return;

            var resultado = MessageBox.Show(this,
                "Apagar todo o histórico de downloads? Esta ação não pode ser desfeita.",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                GerenciadorHistorico.Limpar();
                CarregarHistorico();
            }
        }

        // ---------------------------------------------------------------
        // Verificações silenciosas de inicialização
        // ---------------------------------------------------------------

        private void VerificarDependenciasSilenciosamente()
        {
            RegistrarLog($"App iniciado. yt-dlp existe: {File.Exists(_ytDlpPath)} | ffmpeg existe: {File.Exists(_ffmpegPath)}");

            if (!File.Exists(_ytDlpPath))
            {
                AtualizarStatus("yt-dlp.exe não encontrado.");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (_processoAtual is { HasExited: false })
                {
                    _processoAtual.Kill(entireProcessTree: true);
                }

                notifyIcon1.Visible = false;
            }
            catch
            {
                // Ignora falhas ao encerrar durante o fechamento do app.
            }

            base.OnFormClosing(e);
        }
    }
}

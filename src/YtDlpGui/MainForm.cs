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
        private readonly RastreadorDeArquivos _rastreador = new();
        private string? _nomePlaylistAtual;
        private int _itemAtualPlaylist;
        private int _totalItensPlaylist;
        private string? _ultimoErro;
        private int _ultimoCodigoSaida;
        private bool _avisoBotMostrado;
        private string _urlAtual = "";
        private double _ultimoPercentualLogado = -1;

        // Trava contra duplo clique e operações simultâneas (baixar, extrair cookies, atualizar...).
        // Antes, dava para clicar duas vezes (ou em MP4 e MP3) durante a "Verificando conteúdo..."
        // e iniciar dois yt-dlp ao mesmo tempo, um deles fora do alcance do botão Cancelar.
        private bool _ocupado;

        // Autorreparo: atualizar o yt-dlp e repetir o download só UMA vez por sessão (sem laços).
        private bool _jaTentouAtualizarNestaSessao;

        // Sugestão automática de cookies: feita só uma vez por sessão, e só se o download falhar por falta de login.
        private bool _jaOfereceuCookiesNestaSessao;
        private bool _ultimoDownloadFoiMp3;

        // Cache das imagens da pasta Assets: cada arquivo é lido uma única vez e reaproveitado
        // (antes, cada troca de tela relia os .png do disco sem liberar os anteriores).
        private readonly System.Collections.Generic.Dictionary<string, System.Drawing.Image?> _cacheImagens = new();
        private readonly System.Collections.Generic.List<MemoryStream> _streamsDasImagens = new();

        // Campos da prévia de vídeo (Home)
        private System.Threading.CancellationTokenSource? _ctsPreviaCts;
        private readonly System.Net.Http.HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
        private System.Windows.Forms.Timer? _timerSpinner;
        private int _spinnerFrame;
        private static readonly string[] SpinnerFrames = { "⠋", "⠙", "⠸", "⠴", "⠦", "⠇" };

        private static readonly Regex RegexProgresso =
            new Regex(@"\[download\]\s+(\d{1,3}(?:\.\d+)?)%", RegexOptions.Compiled);
        private static readonly Regex RegexErro =
            new Regex(@"^ERROR:\s*(.+)$", RegexOptions.Compiled);
        private static readonly Regex RegexPlaylistNome =
            new Regex(@"Downloading playlist:\s*(.+)$", RegexOptions.Compiled);
        private static readonly Regex RegexPlaylistItem =
            new Regex(@"Downloading item (\d+) of (\d+)", RegexOptions.Compiled);

        public MainForm()
        {
            InitializeComponent();
            AplicarTema();

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
            RegistrarLog($"App iniciado. yt-dlp existe: {File.Exists(_ytDlpPath)} | ffmpeg existe: {File.Exists(_ffmpegPath)}");

            // O autodiagnóstico roda depois que a janela já apareceu, para não atrasar a abertura.
            BeginInvoke(new Action(async () => await ExecutarAutoDiagnosticoAsync(mostrarResumo: false)));
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

        /// <summary>
        /// Carrega uma imagem da pasta Assets, ou null se não existir/falhar.
        /// O resultado fica em cache: o arquivo é lido uma única vez e a mesma instância é
        /// reaproveitada. A imagem é lida para a memória, então o .png não fica travado no disco.
        /// Nunca chame Dispose nas imagens devolvidas: elas são compartilhadas.
        /// </summary>
        private System.Drawing.Image? CarregarImagem(string nomeArquivo)
        {
            if (_cacheImagens.TryGetValue(nomeArquivo, out var emCache))
                return emCache;

            System.Drawing.Image? imagem = null;

            try
            {
                string caminho = Path.Combine(_pastaAssets, nomeArquivo);
                if (File.Exists(caminho))
                {
                    // O stream precisa continuar aberto enquanto a imagem existir (regra do GDI+);
                    // como as imagens ficam em cache até o programa fechar, guardamos o stream também.
                    var ms = new MemoryStream(File.ReadAllBytes(caminho));
                    imagem = System.Drawing.Image.FromStream(ms);
                    _streamsDasImagens.Add(ms);
                }
                else
                {
                    GerenciadorLog.Aviso("Imagem não encontrada: " + caminho);
                }
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Não foi possível carregar a imagem '" + nomeArquivo + "'", ex);
                imagem = null;
            }

            _cacheImagens[nomeArquivo] = imagem;
            return imagem;
        }

        private void CarregarImagens()
        {
            picLogo.Image = CarregarImagem("logo_app.png");
            picLogoGrande.Image = CarregarImagem("logo_app.png");
            picIlustracaoPasta.Image = CarregarImagem("ilustracao_pasta.png");
            picLogoConfig.Image = CarregarImagem("ic_config_roxo.png");

            AplicarIcones();                 // ícones dos botões e campos (Tema.cs)
            AtualizarIconesDeNavegacao("home");
        }

        // ---------------------------------------------------------------
        // Prévia de vídeo (Home)
        // ---------------------------------------------------------------

        /// <summary>
        /// Só aceita endereços http/https. Antes, qualquer texto que o .NET considerasse uma
        /// "URL absoluta" passava (inclusive caminhos como C:\Windows ou ftp://...), disparando
        /// o yt-dlp à toa e gerando erros confusos.
        /// </summary>
        private static bool UrlValida(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Disparado pelo evento TextChanged do campo de URL (debounced via
        /// cancellation: só roda de fato se o usuário parar de digitar por
        /// 600ms, evitando chamadas ao yt-dlp a cada tecla).
        /// </summary>
        private async void txtUrl_TextChanged(object sender, EventArgs e)
        {
            // Cancela qualquer busca anterior ainda em andamento. Agora o cancelamento
            // encerra de verdade o yt-dlp da busca antiga (antes ele ficava rodando escondido).
            _ctsPreviaCts?.Cancel();
            _ctsPreviaCts = new CancellationTokenSource();
            var token = _ctsPreviaCts.Token;

            string url = txtUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(url) || !UrlValida(url))
            {
                MostrarLogoPadrao();
                return;
            }

            // Debounce: aguarda 600ms antes de consultar o yt-dlp
            try { await Task.Delay(600, token); }
            catch (OperationCanceledException) { return; }

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
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Falha ao buscar a prévia do vídeo", ex);
                if (!token.IsCancellationRequested) MostrarLogoPadrao();
            }
            finally
            {
                // Só desliga o spinner se esta ainda for a busca mais recente. Antes, uma busca
                // antiga que terminava desligava o spinner da busca nova, que ainda estava rodando.
                if (!token.IsCancellationRequested) PararSpinner();
            }
        }

        /// <summary>
        /// Roda yt-dlp --dump-json (sem baixar) e extrai título, canal e
        /// URL da thumbnail do vídeo.
        /// </summary>
        private async Task<PreviaVideo?> BuscarPreviaAsync(string url, CancellationToken token)
        {
            if (!File.Exists(_ytDlpPath)) return null;

            var psi = new ProcessStartInfo { FileName = _ytDlpPath };
            psi.ArgumentList.Add("--dump-json");
            psi.ArgumentList.Add("--no-warnings");
            psi.ArgumentList.Add("--skip-download");
            psi.ArgumentList.Add("--no-playlist");
            psi.ArgumentList.Add(url);

            // Lê stdout e stderr juntos (sem risco de travar), com tempo limite de 40s e
            // cancelamento que encerra o processo de verdade.
            var resultado = await ProcessosFilhos.ExecutarCapturandoAsync(psi, TimeSpan.FromSeconds(40), token);

            if (resultado.Cancelado || token.IsCancellationRequested) return null;

            if (resultado.TempoEsgotado)
            {
                GerenciadorLog.Aviso("A busca da prévia demorou demais e foi interrompida. URL: " + url);
                return null;
            }

            // A prévia é opcional: URL sem vídeo, sem internet etc. não são erros do programa.
            if (!resultado.Iniciou || resultado.CodigoSaida != 0 || string.IsNullOrWhiteSpace(resultado.Saida))
                return null;

            string titulo;
            string canal;
            string thumbUrl;
            string duracao;

            try
            {
                using var doc = JsonDocument.Parse(resultado.Saida);
                var root = doc.RootElement;

                titulo = root.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                canal = root.TryGetProperty("channel", out var c) ? c.GetString() ?? "" : "";
                thumbUrl = root.TryGetProperty("thumbnail", out var th) ? th.GetString() ?? "" : "";
                duracao = root.TryGetProperty("duration_string", out var d) ? d.GetString() ?? "" : "";
            }
            catch (Exception ex)
            {
                GerenciadorLog.Aviso("A prévia devolveu dados em formato inesperado: " + ex.Message);
                return null;
            }

            if (string.IsNullOrEmpty(titulo)) return null;

            Image? thumbImg = null;
            if (!string.IsNullOrEmpty(thumbUrl) && UrlValida(thumbUrl))
            {
                try
                {
                    byte[] bytes = await _httpClient.GetByteArrayAsync(thumbUrl, token);

                    // Copia para um Bitmap próprio: uma Image criada de um stream precisa que o stream
                    // continue aberto. Antes o stream era descartado logo depois, o que pode causar
                    // erro do GDI+ mais tarde, quando a imagem é desenhada na tela.
                    using var ms = new MemoryStream(bytes);
                    using var original = Image.FromStream(ms);
                    thumbImg = new Bitmap(original);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // thumbnail é opcional
                    GerenciadorLog.Atividade("Miniatura indisponível: " + ex.Message);
                }
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

            // Libera a miniatura anterior (antes, cada prévia deixava a anterior na memória).
            var anterior = picThumb.Image;
            picThumb.Image = previa.Thumbnail;
            if (anterior is not null && !ReferenceEquals(anterior, previa.Thumbnail))
                anterior.Dispose();

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
            PararSpinner();
            pnlPrevia.Visible = false;
            picLogoGrande.Visible = true;
            picLogoGrande.Image = CarregarImagem("logo_app.png");
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

        // AtualizarIconesDeNavegacao agora fica em Tema.cs (barra lateral com rótulos).

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

            AtualizarIconesDeNavegacao(pagina);

            if (pagina == "historico")
            {
                CarregarHistorico();
            }
        }

        // Os botões da barra lateral (btnNav*_Click) ficam em Tema.cs; o logo também leva ao Início.
        private void picLogo_Click(object sender, EventArgs e) => MostrarPagina("home");

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

        private async void btnDownloadMp4_Click(object sender, EventArgs e) => await IniciarDownloadAsync(mp3: false);

        private async void btnDownloadMp3_Click(object sender, EventArgs e) => await IniciarDownloadAsync(mp3: true);

        /// <summary>
        /// Fluxo único para MP4 e MP3 (antes eram dois blocos quase idênticos).
        /// Pontos de segurança:
        ///   - Trava _ocupado: um segundo clique durante a verificação não inicia outro download.
        ///   - A URL é "fotografada" uma única vez: antes ela era lida de novo DEPOIS do filtro de
        ///     conteúdo, então dava para trocar o endereço durante a verificação e burlar o filtro.
        ///   - Os controles são travados antes do filtro, não só depois de ele terminar.
        ///   - Qualquer erro vai para o log em vez de derrubar o programa (era "async void").
        /// </summary>
        private async Task IniciarDownloadAsync(bool mp3)
        {
            if (_ocupado)
                return;

            _ocupado = true;
            try
            {
                // Se o yt-dlp.exe sumiu, oferece o reparo automático antes de reclamar.
                if (!File.Exists(_ytDlpPath))
                    await OferecerBaixarYtDlpAsync("O yt-dlp.exe não foi encontrado na pasta do programa.");

                if (!ValidarEntradas(exigeFfmpeg: mp3))
                    return;

                string url = txtUrl.Text.Trim();

                DefinirEstadoDownloadEmAndamento(true);

                // ---- MÓDULO OPCIONAL: Filtro de Conteúdo (ver FiltroConteudo.cs) ----
                // Sem "filtro-conteudo.json" na pasta do app, isto não faz nada.
                if (!await PassaPeloFiltroDeConteudoAsync(url))
                    return;
                // -----------------------------------------------------------------

                _ultimoDownloadFoiMp3 = mp3;
                var argumentos = ConstruirArgumentos(url, mp3);
                await ExecutarYtDlpAsync(argumentos, mp3 ? "MP3" : "MP4", url);
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Falha ao iniciar o download", ex);
                AtualizarStatus("Erro inesperado.");
                MostrarErroInesperado();
            }
            finally
            {
                _ocupado = false;
                DefinirEstadoDownloadEmAndamento(false);
            }
        }

        private System.Collections.Generic.List<string> ConstruirArgumentos(string url, bool mp3)
        {
            var argumentos = new System.Collections.Generic.List<string> { url };

            if (mp3)
            {
                argumentos.Add("-x");
                argumentos.Add("--audio-format");
                argumentos.Add("mp3");
            }
            else
            {
                argumentos.Add("-f");
                argumentos.Add("bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/bestvideo+bestaudio/best");
                argumentos.Add("--merge-output-format");
                argumentos.Add("mp4");
            }

            argumentos.Add("--newline");
            argumentos.Add("-P");
            argumentos.Add(txtSavePath.Text);
            argumentos.Add("-o");
            argumentos.Add(ObterTemplateDeSaida());

            AdicionarArgumentoDePlaylist(argumentos);
            AdicionarArgumentosDeAutenticacao(argumentos);

            if (File.Exists(_ffmpegPath))
            {
                argumentos.Add("--ffmpeg-location");
                argumentos.Add(_ffmpegDir);
            }

            return argumentos;
        }

        private void MostrarErroInesperado()
        {
            if (IsDisposed)
                return;

            MessageBox.Show(this,
                "Ocorreu um erro inesperado, mas o programa continua funcionando.\n\n" +
                "Os detalhes técnicos foram salvos em:\n" + GerenciadorLog.CaminhoErros,
                "Erro inesperado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        private async Task<bool> PassaPeloFiltroDeConteudoAsync(string url)
        {
            try
            {
                var configFiltro = FiltroConteudo.CarregarConfiguracao();
                if (!configFiltro.Habilitado)
                    return true;

                AtualizarStatus("Verificando conteúdo...");
                var resultado = await FiltroConteudo.VerificarAsync(url, _ytDlpPath, configFiltro);

                if (resultado.Permitido)
                {
                    AtualizarStatus("Pronto para uso.");
                    return true;
                }

                RegistrarLog($"Filtro de conteúdo bloqueou: {resultado.Motivo} | URL: {url}");
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
            catch (Exception ex)
            {
                // Com o filtro LIGADO, na dúvida o download é bloqueado: é uma proteção de sala de aula.
                GerenciadorLog.Erro("Falha ao verificar o filtro de conteúdo", ex);
                AtualizarStatus("Não foi possível verificar o conteúdo.");
                if (!IsDisposed)
                {
                    MessageBox.Show(this,
                        "Não foi possível verificar o conteúdo deste endereço agora, então o download não foi iniciado.\n\n" +
                        "Tente novamente em instantes.",
                        "Verificação não concluída", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }
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

            if (!UrlValida(txtUrl.Text.Trim()))
            {
                MessageBox.Show(this,
                    "A URL informada não parece ser válida.\nUse um endereço que comece com http:// ou https://",
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

            // Pouco espaço em disco é uma causa comum de download que falha no meio.
            long? livre = AutoReparo.EspacoLivreEmBytes(txtSavePath.Text);
            if (livre is long bytesLivres && bytesLivres < 500L * 1024 * 1024)
            {
                var continuar = MessageBox.Show(this,
                    $"A pasta de destino tem pouco espaço livre (cerca de {bytesLivres / 1024 / 1024} MB). " +
                    "O download pode falhar no meio. Deseja continuar mesmo assim?",
                    "Pouco espaço em disco", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (continuar != DialogResult.Yes)
                {
                    MostrarPagina("pasta");
                    return false;
                }
            }

            return true;
        }

        private enum ResultadoExecucao
        {
            Sucesso,
            Cancelado,
            Falha,
            NaoIniciou
        }

        /// <summary>
        /// Executa o yt-dlp.exe de forma assíncrona e oculta. Não expõe a
        /// saída bruta ao usuário — apenas um status curto + porcentagem.
        /// A saída é gravada silenciosamente em log-atividade.txt (e os erros em
        /// log-erros.txt), na pasta do programa, para suporte.
        ///
        /// AUTORREPARO: se o download falhar com um erro típico de yt-dlp desatualizado,
        /// o programa atualiza o yt-dlp sozinho e repete o mesmo download, UMA vez por sessão.
        /// </summary>
        private async Task ExecutarYtDlpAsync(
            System.Collections.Generic.List<string> argumentos, string tipoParaHistorico, string url)
        {
            bool ehAtualizacao = string.IsNullOrEmpty(tipoParaHistorico);
            _urlAtual = url;

            DefinirEstadoDownloadEmAndamento(true);
            AtualizarProgresso(0);
            AtualizarStatus("Iniciando download...");
            RegistrarLog($"===== Início: {(ehAtualizacao ? "atualização do yt-dlp" : tipoParaHistorico)} | {url} =====");

            try
            {
                var resultado = await RodarYtDlpAsync(argumentos);

                // ---- AUTORREPARO: yt-dlp desatualizado -> atualiza e tenta de novo (1x por sessão) ----
                if (resultado == ResultadoExecucao.Falha && !ehAtualizacao &&
                    !_jaTentouAtualizarNestaSessao && AutoReparo.PodeSerCorrigidoComAtualizacao(_ultimoErro))
                {
                    _jaTentouAtualizarNestaSessao = true;
                    string erroOriginal = _ultimoErro ?? "";
                    int codigoOriginal = _ultimoCodigoSaida;

                    GerenciadorLog.Reparo("O download falhou com um erro típico de yt-dlp desatualizado (\"" + erroOriginal +
                                          "\"). Atualizando o yt-dlp e tentando de novo, uma única vez.");
                    AtualizarStatus("Tentando corrigir automaticamente: atualizando o yt-dlp...");

                    var atualizacao = await RodarYtDlpAsync(new System.Collections.Generic.List<string> { "-U" }, reiniciarEstado: false);

                    if (atualizacao == ResultadoExecucao.Sucesso)
                    {
                        AtualizarStatus("yt-dlp atualizado. Tentando o download de novo...");
                        AtualizarProgresso(0);
                        resultado = await RodarYtDlpAsync(argumentos);

                        if (resultado == ResultadoExecucao.Sucesso)
                            GerenciadorLog.Reparo("Download concluído depois de atualizar o yt-dlp automaticamente.");
                        else
                            GerenciadorLog.Aviso("O download continuou falhando mesmo depois de atualizar o yt-dlp.");
                    }
                    else
                    {
                        GerenciadorLog.Aviso("Não foi possível atualizar o yt-dlp automaticamente.");
                        if (atualizacao == ResultadoExecucao.Cancelado)
                            resultado = ResultadoExecucao.Cancelado;

                        // Mostra ao usuário o erro do download, não o da tentativa de atualização.
                        _ultimoErro = erroOriginal;
                        _ultimoCodigoSaida = codigoOriginal;
                    }
                }

                // ---- AUTORREPARO 2: o YouTube pediu login -> sugere copiar os cookies do navegador padrão ----
                if (resultado == ResultadoExecucao.Falha && !ehAtualizacao && _rastreador.Finais.Count == 0 &&
                    !_jaOfereceuCookiesNestaSessao && ErroPedeCookies(_ultimoErro))
                {
                    _jaOfereceuCookiesNestaSessao = true;
                    string erroAntes = _ultimoErro ?? "";

                    if (await TentarCookiesAutomaticosAsync())
                    {
                        AtualizarProgresso(0);
                        AtualizarStatus("Cookies prontos. Tentando o download de novo...");
                        resultado = await RodarYtDlpAsync(ConstruirArgumentos(url, _ultimoDownloadFoiMp3));
                    }
                    else
                    {
                        _ultimoErro = erroAntes;
                        AtualizarStatus("Falha no download.");
                    }
                }

                switch (resultado)
                {
                    case ResultadoExecucao.Cancelado:
                        AtualizarStatus("Download cancelado.");
                        if (ehAtualizacao)
                            lblAtualizarStatus.Text = "Cancelado.";
                        break;

                    case ResultadoExecucao.Sucesso:
                        if (ehAtualizacao)
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

                            string nomeParaNotificacao = RegistrarHistoricoDoDownload(tipoParaHistorico, url);
                            MostrarNotificacao("Download concluído", nomeParaNotificacao);
                        }
                        break;

                    case ResultadoExecucao.NaoIniciou:
                        AtualizarStatus("Erro ao iniciar o yt-dlp.");
                        if (ehAtualizacao)
                            lblAtualizarStatus.Text = "Falha ao atualizar.";
                        await OferecerBaixarYtDlpAsync(
                            "Não foi possível iniciar o yt-dlp.exe: " + (_ultimoErro ?? "motivo desconhecido") +
                            "\n\nO arquivo pode estar danificado ou ter sido bloqueado pelo antivírus.");
                        break;

                    default: // Falha
                        string mensagem = TraduzirErro(_ultimoErro);

                        if (!ehAtualizacao && _rastreador.Finais.Count > 0)
                        {
                            // Sucesso parcial (ex.: playlist em que alguns vídeos estavam indisponíveis):
                            // antes, tudo era tratado como falha total e nada entrava no histórico.
                            AtualizarStatus("Concluído com avisos.");
                            string nomeParcial = RegistrarHistoricoDoDownload(tipoParaHistorico, url);
                            MostrarNotificacao("Download concluído com avisos", nomeParcial, erro: true);
                            MessageBox.Show(this,
                                nomeParcial + "\n\nAlguns itens não puderam ser baixados:\n" + mensagem,
                                "Concluído com avisos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            AtualizarStatus("Falha no download.");
                            if (ehAtualizacao)
                                lblAtualizarStatus.Text = "Falha ao atualizar.";
                            MostrarNotificacao("Falha no download", mensagem, erro: true);
                            MessageBox.Show(this,
                                mensagem + "\n\nDetalhes técnicos salvos em:\n" + GerenciadorLog.CaminhoErros,
                                "Falha no download", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Erro inesperado ao executar o yt-dlp", ex);
                AtualizarStatus("Erro inesperado.");
                MostrarErroInesperado();
            }
            finally
            {
                _processoAtual = null;
                DefinirEstadoDownloadEmAndamento(false);
            }
        }

        /// <summary>
        /// Roda UMA execução do yt-dlp.exe e devolve o desfecho. Não mostra telas de resultado:
        /// quem decide o que mostrar (e se vale tentar um reparo) é o ExecutarYtDlpAsync.
        /// </summary>
        private async Task<ResultadoExecucao> RodarYtDlpAsync(
            System.Collections.Generic.List<string> argumentos, bool reiniciarEstado = true)
        {
            if (reiniciarEstado)
            {
                _canceladoPeloUsuario = false;
                _rastreador.Reiniciar();
                _nomePlaylistAtual = null;
                _itemAtualPlaylist = 0;
                _totalItensPlaylist = 0;
                _ultimoErro = null;
                _avisoBotMostrado = false;
                _ultimoPercentualLogado = -1;
            }

            _ultimoCodigoSaida = 0;

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
            // Pede ao Python do yt-dlp que escreva em UTF-8 quando a saída é redirecionada
            // (ajuda a manter acentos corretos em títulos e caminhos).
            psi.Environment["PYTHONIOENCODING"] = "utf-8";

            foreach (var arg in argumentos)
                psi.ArgumentList.Add(arg);

            using var processo = new Process { StartInfo = psi, EnableRaisingEvents = true };
            processo.OutputDataReceived += (s, e) => ProcessarLinhaDeSaida(e.Data);
            processo.ErrorDataReceived += (s, e) => ProcessarLinhaDeSaida(e.Data);

            int pid = 0;

            try
            {
                _processoAtual = processo;
                processo.Start();
                pid = processo.Id;
                ProcessosFilhos.Registrar(pid);
                processo.BeginOutputReadLine();
                processo.BeginErrorReadLine();

                await processo.WaitForExitAsync();

                _ultimoCodigoSaida = processo.ExitCode;

                if (_canceladoPeloUsuario)
                {
                    _rastreador.Encerrar(false, ExisteArquivoDoDownload);
                    return ResultadoExecucao.Cancelado;
                }

                bool deuCerto = processo.ExitCode == 0;
                _rastreador.Encerrar(deuCerto, ExisteArquivoDoDownload);

                if (deuCerto)
                    return ResultadoExecucao.Sucesso;

                GerenciadorLog.Erro(
                    $"O yt-dlp terminou com código {processo.ExitCode} (URL: {_urlAtual}). " +
                    $"Último erro informado: {_ultimoErro ?? "(nenhum capturado)"}");
                return ResultadoExecucao.Falha;
            }
            catch (System.ComponentModel.Win32Exception exWin32)
            {
                GerenciadorLog.Erro("Não foi possível iniciar o yt-dlp.exe", exWin32);
                _ultimoErro = exWin32.Message;
                return ResultadoExecucao.NaoIniciou;
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Erro inesperado durante a execução do yt-dlp", ex);
                _ultimoErro = ex.Message;
                return ResultadoExecucao.Falha;
            }
            finally
            {
                if (pid != 0)
                    ProcessosFilhos.Remover(pid);
                _processoAtual = null;
            }
        }

        private bool ExisteArquivoDoDownload(string caminho)
        {
            try
            {
                string completo = Path.IsPathRooted(caminho) ? caminho : Path.Combine(txtSavePath.Text, caminho);
                return File.Exists(completo);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reparo com consentimento: pergunta se pode baixar um yt-dlp.exe novo do GitHub
        /// oficial (conferindo o hash SHA-256 antes de usar).
        /// </summary>
        private async Task OferecerBaixarYtDlpAsync(string motivo)
        {
            if (IsDisposed)
                return;

            var resposta = MessageBox.Show(this,
                motivo + "\n\n" +
                "Posso tentar baixar agora o yt-dlp.exe oficial? O arquivo vem do GitHub do projeto yt-dlp e só é " +
                "usado depois de passar na verificação de integridade (SHA-256).\n\n" +
                "É preciso estar conectado à internet.",
                "Reparar o yt-dlp.exe", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resposta != DialogResult.Yes)
            {
                GerenciadorLog.Atividade("O usuário recusou o reparo automático do yt-dlp.exe.");
                return;
            }

            DefinirEstadoDownloadEmAndamento(true);
            btnCancelar.Enabled = false;
            AtualizarStatus("Baixando o yt-dlp.exe oficial... aguarde.");

            try
            {
                var (sucesso, mensagem) = await AutoReparo.BaixarYtDlpAsync(_ytDlpPath);
                if (IsDisposed)
                    return;

                AtualizarStatus(sucesso ? "yt-dlp.exe reparado!" : "Não foi possível reparar o yt-dlp.exe.");
                MessageBox.Show(this, mensagem,
                    sucesso ? "Reparo concluído" : "Reparo não concluído",
                    MessageBoxButtons.OK,
                    sucesso ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            finally
            {
                DefinirEstadoDownloadEmAndamento(false);
            }
        }

        /// <summary>
        /// Registra o(s) item(ns) desse download no histórico e retorna um
        /// texto curto para usar na notificação do Windows.
        /// - Vídeo único: um item.
        /// - Playlist + "organizar em subpastas": UM item representando a
        ///   playlist inteira (nome + quantidade de vídeos), apontando para
        ///   a subpasta criada.
        /// - Playlist sem essa opção: um item para CADA vídeo baixado.
        ///
        /// Os nomes vêm do RastreadorDeArquivos, que só considera os arquivos FINAIS
        /// (antes, arquivos temporários como "Aula.f140.m4a" apareciam no histórico, e cada
        /// vídeo de uma playlist entrava duas vezes). A pasta gravada é a pasta real do arquivo
        /// (o yt-dlp troca caracteres proibidos no nome da pasta, então montar o caminho
        /// "na mão" podia apontar para uma pasta que não existe).
        /// </summary>
        private string RegistrarHistoricoDoDownload(string tipoParaHistorico, string url)
        {
            var agora = DateTime.Now;
            var finais = _rastreador.Finais;
            string pastaBase = txtSavePath.Text;

            // "Playlist" marcada mas URL de vídeo único: o yt-dlp não informa nome nem total de itens.
            bool ehPlaylist = chkPlaylist.Checked &&
                              (_totalItensPlaylist > 0 || !string.IsNullOrWhiteSpace(_nomePlaylistAtual));

            if (!ehPlaylist)
            {
                string? arquivo = finais.Count > 0 ? finais[finais.Count - 1] : null;
                string nomeArquivo = arquivo is null ? "(nome não identificado)" : Path.GetFileName(arquivo);

                GerenciadorHistorico.Adicionar(new HistoricoItem
                {
                    DataHora = agora,
                    Tipo = tipoParaHistorico,
                    Titulo = nomeArquivo,
                    Pasta = PastaDoArquivo(arquivo, pastaBase),
                    Url = url
                });

                return nomeArquivo;
            }

            if (chkOrganizarPlaylist.Checked)
            {
                string nomePlaylist = string.IsNullOrWhiteSpace(_nomePlaylistAtual)
                    ? "Playlist"
                    : _nomePlaylistAtual!;
                string pastaPlaylistPrevista = Path.Combine(pastaBase, nomePlaylist);
                string titulo = $"{nomePlaylist} ({finais.Count} vídeos)";

                GerenciadorHistorico.Adicionar(new HistoricoItem
                {
                    DataHora = agora,
                    Tipo = tipoParaHistorico,
                    Titulo = titulo,
                    Pasta = finais.Count > 0 ? PastaDoArquivo(finais[0], pastaPlaylistPrevista) : pastaPlaylistPrevista,
                    Url = url
                });

                return titulo;
            }

            // Playlist sem agrupamento: um item de histórico por vídeo baixado.
            if (finais.Count == 0)
            {
                // Fallback de segurança, caso nenhum arquivo final tenha sido identificado.
                GerenciadorHistorico.Adicionar(new HistoricoItem
                {
                    DataHora = agora,
                    Tipo = tipoParaHistorico,
                    Titulo = "(itens da playlist não identificados)",
                    Pasta = pastaBase,
                    Url = url
                });
                return "Playlist concluída";
            }

            // Uma única gravação no disco para todos os itens.
            var itens = new System.Collections.Generic.List<HistoricoItem>();
            foreach (var caminho in finais)
            {
                itens.Add(new HistoricoItem
                {
                    DataHora = agora,
                    Tipo = tipoParaHistorico,
                    Titulo = Path.GetFileName(caminho),
                    Pasta = PastaDoArquivo(caminho, pastaBase),
                    Url = url
                });
            }
            GerenciadorHistorico.AdicionarVarios(itens);

            return $"Playlist: {finais.Count} vídeos baixados";
        }

        /// <summary>Pasta real onde o arquivo foi salvo, ou o valor padrão se não for possível saber.</summary>
        private static string PastaDoArquivo(string? arquivo, string padrao)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(arquivo))
                {
                    string? pasta = Path.GetDirectoryName(arquivo);
                    if (!string.IsNullOrEmpty(pasta) && Directory.Exists(pasta))
                        return pasta;
                }
            }
            catch
            {
                // Caminho em formato estranho: usa o padrão.
            }

            return padrao;
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

            if (erroBruto.Contains("confirm your age", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("age-restricted", StringComparison.OrdinalIgnoreCase))
            {
                return "Este vídeo tem restrição de idade e exige login. " +
                       "Ative os cookies do navegador ou um arquivo cookies.txt em \"Configurações\".";
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

            if (erroBruto.Contains("No space left on device", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("Errno 28"))
            {
                return "O disco ficou sem espaço livre. Libere espaço ou escolha outra pasta na tela \"Pasta\".";
            }

            if (erroBruto.Contains("Permission denied", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("Errno 13"))
            {
                return "Sem permissão para gravar na pasta de destino. Escolha outra pasta na tela \"Pasta\".";
            }

            if (erroBruto.Contains("getaddrinfo failed", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("name resolution", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("Network is unreachable", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("timed out", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("Connection reset", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("Connection aborted", StringComparison.OrdinalIgnoreCase))
            {
                return "Não foi possível conectar à internet. Verifique a conexão e tente novamente.";
            }

            if (erroBruto.Contains("HTTP Error 429", StringComparison.OrdinalIgnoreCase))
            {
                return "O site recebeu pedidos demais e pediu para esperar. Aguarde alguns minutos e tente de novo.";
            }

            if (erroBruto.Contains("Unsupported URL", StringComparison.OrdinalIgnoreCase))
            {
                return "Este endereço não é suportado pelo programa. Confira se copiou o link do vídeo.";
            }

            return "Ocorreu um erro: " + erroBruto;
        }

        private void ProcessarLinhaDeSaida(string? linha)
        {
            if (string.IsNullOrWhiteSpace(linha))
                return;

            // Este método roda numa thread do .NET (eventos do Process). Se a janela já foi fechada
            // não há onde mostrar nada: antes, isso podia causar ObjectDisposedException/InvalidOperationException.
            if (IsDisposed || Disposing)
                return;

            string texto = linha;

            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => TratarLinha(texto)));
                }
                else
                {
                    TratarLinha(texto);
                }
            }
            catch (InvalidOperationException)
            {
                // Janela sem handle, em fechamento ou já descartada (ObjectDisposedException
                // também é um InvalidOperationException).
            }
        }

        private void TratarLinha(string linha)
        {
            double percentual = 0;
            var matchProgresso = RegexProgresso.Match(linha);
            bool ehProgresso = matchProgresso.Success && double.TryParse(
                matchProgresso.Groups[1].Value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out percentual);

            if (ehProgresso)
            {
                // No log, registra o progresso só de 25 em 25% (antes cada linha de % ia para o
                // arquivo, a cada atualização, inflando o log e travando a tela com escrita em disco).
                if (percentual >= 100 || percentual < _ultimoPercentualLogado || percentual - _ultimoPercentualLogado >= 25)
                {
                    RegistrarLog(linha);
                    _ultimoPercentualLogado = percentual;
                }

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
            else
            {
                RegistrarLog(linha);
            }

            // Descobre os arquivos finais (ignora os temporários que o yt-dlp apaga depois).
            _rastreador.ProcessarLinha(linha);

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
                GerenciadorLog.Erro("O yt-dlp informou um erro (URL: " + _urlAtual + "): " + _ultimoErro);
            }

            if (!_avisoBotMostrado && linha.Contains("Sign in to confirm") && linha.Contains("not a bot"))
            {
                _avisoBotMostrado = true;
            }
        }

        private void AtualizarStatus(string texto)
        {
            if (IsDisposed)
                return;

            lblStatus.Text = texto;
        }

        private void AtualizarProgresso(double percentual)
        {
            if (IsDisposed)
                return;

            int valor = Math.Clamp((int)Math.Round(percentual), 0, 100);
            progressBar1.Value = valor;
            lblProgressPercent.Text = $"{valor}%";
        }

        /// <summary>
        /// Grava uma linha no log de atividade (log-atividade.txt, na pasta do programa),
        /// sem nunca exibir isso na interface. Falhas de escrita são tratadas pelo GerenciadorLog
        /// (que usa uma pasta alternativa se a do programa não permitir gravação).
        /// </summary>
        private static void RegistrarLog(string linha)
        {
            GerenciadorLog.Atividade(linha);
        }

        private void DefinirEstadoDownloadEmAndamento(bool emAndamento)
        {
            if (IsDisposed)
                return;

            btnDownloadMp4.Enabled = !emAndamento;
            btnDownloadMp3.Enabled = !emAndamento;
            txtUrl.Enabled = !emAndamento;
            chkPlaylist.Enabled = !emAndamento;
            chkOrganizarPlaylist.Enabled = !emAndamento;
            btnAlterarLocal.Enabled = !emAndamento;
            btnAtualizarYtDlp.Enabled = !emAndamento;
            btnExtrairCookies.Enabled = !emAndamento;
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

        /// <summary>
        /// Extrai os cookies do navegador selecionado no combo e grava num
        /// arquivo cookies.txt fixo na pasta do programa. Resolve o erro
        /// "403 Forbidden" que os usuários enfrentam com
        /// "--cookies-from-browser" lido ao vivo: além do navegador
        /// frequentemente travar o próprio arquivo de cookies enquanto está
        /// aberto, a leitura ao vivo pode falhar por decriptação (DPAPI) de
        /// forma inconsistente. Gerar um arquivo estável uma vez, e reutilizar
        /// esse arquivo daí em diante, evita os dois problemas.
        ///
        /// Segurança/robustez: tem tempo limite de 2 minutos (se o navegador travar o arquivo,
        /// antes o programa ficava esperando para sempre), o processo é registrado para ser
        /// encerrado junto com o programa, e a trava _ocupado impede iniciar outra operação junto.
        /// </summary>
        private async void btnExtrairCookies_Click(object sender, EventArgs e)
        {
            if (_ocupado)
                return;

            if (!File.Exists(_ytDlpPath))
            {
                MessageBox.Show(this, $"Não foi possível encontrar o yt-dlp.exe em:\n{_ytDlpPath}",
                    "yt-dlp.exe não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cmbNavegador.SelectedItem is not string navegador || string.IsNullOrWhiteSpace(navegador))
            {
                MessageBox.Show(this, "Selecione um navegador na lista antes de extrair os cookies.",
                    "Nenhum navegador selecionado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _ocupado = true;
            DefinirEstadoDownloadEmAndamento(true);

            try
            {
                var (sucesso, mensagem) = await ExtrairCookiesAsync(navegador);
                if (!sucesso && !IsDisposed)
                {
                    MessageBox.Show(this, mensagem, "Falha ao extrair cookies",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Erro inesperado na extração de cookies", ex);
                MostrarErroInesperado();
            }
            finally
            {
                _ocupado = false;
                DefinirEstadoDownloadEmAndamento(false);
            }
        }

        /// <summary>
        /// Núcleo da extração de cookies (usado pelo botão e pelo fluxo automático).
        /// Não mexe na trava _ocupado: quem chama é responsável por isso.
        /// Em caso de sucesso, já deixa a tela apontando para o cookies.txt gerado.
        /// Em caso de falha, devolve a mensagem amigável (sem mostrá-la).
        /// </summary>
        private static bool CookiesTxtTemCookiesDoYoutube(string caminho)
        {
            try
            {
                if (!File.Exists(caminho) || new FileInfo(caminho).Length == 0)
                    return false;
                foreach (var linha in File.ReadLines(caminho))
                {
                    if (linha.StartsWith('#')) continue;
                    if (linha.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
                        linha.Contains("google.com", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Não foi possível conferir o cookies.txt gerado", ex);
            }
            return false;
        }

        private async Task<(bool Sucesso, string Mensagem)> ExtrairCookiesAsync(string navegador)
        {
            // Normalmente é a pasta do programa; se ela não permitir gravação, usa a pasta alternativa.
            string caminhoCookies = Path.Combine(AutoReparo.PastaGravavel(), "cookies.txt");

            // Apaga a versão anterior: assim um cookies.txt velho nunca é confundido com um recém-gerado.
            try { if (File.Exists(caminhoCookies)) File.Delete(caminhoCookies); } catch { }

            lblExtrairCookiesStatus.Text = "Extraindo...";
            AtualizarStatus($"Extraindo cookies do {navegador}...");
            RegistrarLog($"Iniciando extração de cookies do navegador: {navegador}");

            var psi = new ProcessStartInfo
            {
                FileName = _ytDlpPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            psi.Environment["PYTHONIOENCODING"] = "utf-8";
            psi.ArgumentList.Add("--cookies-from-browser");
            psi.ArgumentList.Add(navegador);
            psi.ArgumentList.Add("--cookies");
            psi.ArgumentList.Add(caminhoCookies);
            psi.ArgumentList.Add("--skip-download");
            psi.ArgumentList.Add("--simulate");
            psi.ArgumentList.Add("--no-warnings");
            // Vídeo estável (o primeiro vídeo já publicado no YouTube) usado
            // só como "gatilho" para o yt-dlp processar e gravar os cookies —
            // nada é baixado (--skip-download --simulate).
            psi.ArgumentList.Add("https://www.youtube.com/watch?v=jNQXAC9IVRw");

            string erroCapturado = "";
            int pid = 0;
            using var limiteDeTempo = new CancellationTokenSource(TimeSpan.FromMinutes(2));

            try
            {
                using var processo = new Process { StartInfo = psi, EnableRaisingEvents = true };
                processo.ErrorDataReceived += (s, e2) =>
                {
                    if (string.IsNullOrWhiteSpace(e2.Data)) return;
                    RegistrarLog(e2.Data);
                    var m = RegexErro.Match(e2.Data);
                    if (m.Success) erroCapturado = m.Groups[1].Value.Trim();
                };

                _processoAtual = processo;
                processo.Start();
                pid = processo.Id;
                ProcessosFilhos.Registrar(pid);
                processo.BeginErrorReadLine();
                string saida = await processo.StandardOutput.ReadToEndAsync(limiteDeTempo.Token);
                RegistrarLog(saida);
                await processo.WaitForExitAsync(limiteDeTempo.Token);

                // O vídeo de teste serve só de "gatilho". O yt-dlp pode terminar com erro
                // ("No video formats found!") por motivos que não têm a ver com os cookies
                // (versão desatualizada, mudança no YouTube) e ainda assim já ter gravado
                // o cookies.txt. Por isso o que vale é o ARQUIVO: ele precisa existir e
                // conter cookies do YouTube/Google, independentemente do código de saída.
                bool sucesso = CookiesTxtTemCookiesDoYoutube(caminhoCookies);
                if (sucesso && processo.ExitCode != 0)
                    RegistrarLog("O yt-dlp terminou com erro no vídeo de teste, mas o cookies.txt foi gerado e será usado.");

                if (sucesso)
                {
                    // Muda automaticamente para o modo "arquivo", já apontando
                    // pro cookies.txt recém-gerado — o usuário não precisa
                    // selecionar nada manualmente depois de extrair.
                    radCookiesArquivo.Checked = true;
                    txtCookiesArquivo.Text = caminhoCookies;
                    lblExtrairCookiesStatus.Text = $"Extraído às {DateTime.Now:HH:mm} — pronto para uso.";
                    AtualizarStatus("Cookies extraídos com sucesso!");
                    RegistrarLog("Extração de cookies concluída com sucesso.");
                    MostrarNotificacao("Cookies extraídos", "O arquivo cookies.txt foi gerado com sucesso.");
                    return (true, "");
                }

                lblExtrairCookiesStatus.Text = "Falha na extração.";
                AtualizarStatus("Falha ao extrair cookies.");
                GerenciadorLog.Erro($"Falha na extração de cookies do {navegador}: {erroCapturado}");
                return (false, TraduzirErroExtracaoCookies(erroCapturado, navegador));
            }
            catch (OperationCanceledException)
            {
                ProcessosFilhos.Encerrar(_processoAtual);
                lblExtrairCookiesStatus.Text = "Falha na extração.";
                AtualizarStatus("A extração de cookies demorou demais.");
                GerenciadorLog.Erro($"A extração de cookies do {navegador} passou de 2 minutos e foi interrompida.");
                return (false,
                    $"A extração de cookies do {navegador} demorou demais e foi interrompida.\n\n" +
                    "Feche completamente o navegador (inclusive na bandeja do sistema, perto do relógio) e tente novamente.");
            }
            catch (Exception ex)
            {
                lblExtrairCookiesStatus.Text = "Falha na extração.";
                AtualizarStatus("Falha ao extrair cookies.");
                GerenciadorLog.Erro("Erro inesperado na extração de cookies", ex);
                return (false, "Erro inesperado ao extrair cookies: " + ex.Message);
            }
            finally
            {
                if (pid != 0)
                    ProcessosFilhos.Remover(pid);
                _processoAtual = null;
            }
        }

        // ---------------------------------------------------------------
        // Cookies automáticos (sugeridos quando o YouTube pede login)
        // ---------------------------------------------------------------

        /// <summary>Erros que normalmente se resolvem usando cookies de uma conta logada.</summary>
        private static bool ErroPedeCookies(string? erro)
        {
            if (string.IsNullOrWhiteSpace(erro))
                return false;

            return erro.Contains("Sign in to confirm", StringComparison.OrdinalIgnoreCase) ||
                   erro.Contains("confirm your age", StringComparison.OrdinalIgnoreCase) ||
                   erro.Contains("age-restricted", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Descobre o navegador padrão do Windows para links https (Chrome, Edge, Firefox...),
        /// no formato que o yt-dlp entende. Devolve null se não conseguir identificar.
        /// </summary>
        private static string? DetectarNavegadorPadrao()
        {
            try
            {
                using var chave = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice");
                string? progId = chave?.GetValue("ProgId") as string;
                if (string.IsNullOrEmpty(progId))
                    return null;

                string p = progId.ToLowerInvariant();
                if (p.Contains("chrome")) return "chrome";
                if (p.Contains("edge")) return "edge";
                if (p.Contains("firefox")) return "firefox";
                if (p.Contains("brave")) return "brave";
                if (p.Contains("opera")) return "opera";
                if (p.Contains("vivaldi")) return "vivaldi";
            }
            catch (Exception ex)
            {
                GerenciadorLog.Aviso("Não foi possível identificar o navegador padrão: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Chamado quando o download falha por falta de login. Sugere copiar os cookies do navegador
        /// padrão do computador e, se isso também falhar, indica uma ferramenta que exporta os cookies
        /// para .txt. Devolve true se os cookies foram extraídos (vale tentar o download de novo).
        /// </summary>
        private async Task<bool> TentarCookiesAutomaticosAsync()
        {
            if (IsDisposed)
                return false;

            string navegador = DetectarNavegadorPadrao() ?? (cmbNavegador.SelectedItem as string) ?? "chrome";

            var resposta = MessageBox.Show(this,
                "O YouTube pediu login para baixar este vídeo.\n\n" +
                $"Posso tentar copiar os cookies do {navegador} (navegador padrão deste computador) e repetir o download?\n\n" +
                "Os cookies são como uma \"chave de login\": o arquivo cookies.txt fica na pasta do programa. " +
                "Use isso apenas em um computador seu e, de preferência, com uma conta que você não se importe de expor.",
                "Usar os cookies do navegador?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resposta != DialogResult.Yes)
            {
                GerenciadorLog.Atividade("O usuário recusou a extração automática de cookies.");
                return false;
            }

            GerenciadorLog.Reparo($"O YouTube pediu login; tentando extrair cookies do navegador padrão ({navegador}).");
            var (sucesso, mensagem) = await ExtrairCookiesAsync(navegador);

            if (sucesso)
            {
                GerenciadorLog.Reparo("Cookies extraídos automaticamente; repetindo o download.");
                return true;
            }

            if (!IsDisposed)
            {
                var abrir = MessageBox.Show(this,
                    mensagem + "\n\n" +
                    "Alguns navegadores recentes (Chrome e Edge, principalmente) protegem os cookies e impedem essa cópia.\n\n" +
                    "Quer abrir a página com instruções de uma ferramenta que exporta os cookies para um arquivo .txt? " +
                    "Depois de gerar o arquivo, escolha-o em Configurações > cookies.txt.",
                    "Não foi possível copiar os cookies", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (abrir == DialogResult.Yes)
                    btnAjudaCookies_Click(this, EventArgs.Empty);
            }

            return false;
        }

        /// <summary>Traduz os erros mais comuns da extração de cookies do navegador.</summary>
        private static string TraduzirErroExtracaoCookies(string erroBruto, string navegador)
        {
            if (string.IsNullOrWhiteSpace(erroBruto))
            {
                return $"Não foi possível extrair os cookies do {navegador}. Verifique se ele está " +
                       "instalado e se você já fez login no YouTube nele.";
            }

            if (erroBruto.Contains("could not find") && erroBruto.Contains("cookies database"))
            {
                return $"Não foi encontrado um perfil do {navegador} com cookies salvos. " +
                       "Abra esse navegador, faça login no YouTube, feche-o completamente e tente novamente.";
            }

            if (erroBruto.Contains("Permission denied") || erroBruto.Contains("being used by another process"))
            {
                return $"O {navegador} está bloqueando o acesso aos cookies. Feche completamente o " +
                       "navegador (inclusive na bandeja do sistema, perto do relógio) e tente novamente.";
            }

            if (erroBruto.Contains("decrypt"))
            {
                return $"Não foi possível decriptar os cookies do {navegador} nesta conta do Windows. " +
                       "Tente fazer login no YouTube novamente nesse navegador antes de extrair.";
            }

            return $"Ocorreu um erro ao extrair os cookies do {navegador}: {erroBruto}";
        }

        private async void btnAtualizarYtDlp_Click(object sender, EventArgs e)
        {
            if (_ocupado)
                return;

            _ocupado = true;
            try
            {
                if (!File.Exists(_ytDlpPath))
                {
                    // Em vez de só reclamar, oferece o reparo automático.
                    await OferecerBaixarYtDlpAsync($"Não foi possível encontrar o yt-dlp.exe em:\n{_ytDlpPath}");
                    return;
                }

                lblAtualizarStatus.Text = "Verificando...";
                await ExecutarYtDlpAsync(new System.Collections.Generic.List<string> { "-U" }, tipoParaHistorico: "", url: "");
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Falha ao atualizar o yt-dlp", ex);
                MostrarErroInesperado();
            }
            finally
            {
                _ocupado = false;
            }
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

            listViewHistorico.AjustarColunas();
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

                AbrirNoExplorador(item.Pasta);
            }
        }

        /// <summary>Abre uma pasta no Explorador de Arquivos, sem deixar um erro derrubar o programa.</summary>
        private void AbrirNoExplorador(string pasta)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = pasta, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Não foi possível abrir a pasta '" + pasta + "'", ex);
                MessageBox.Show(this, "Não foi possível abrir a pasta:\n" + pasta,
                    "Erro ao abrir a pasta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        // Autodiagnóstico (ver AutoReparo.cs)
        // ---------------------------------------------------------------

        /// <summary>
        /// Roda as verificações de integridade (yt-dlp, ffmpeg, Assets, histórico, filtro, logs).
        /// Corrige sozinho o que é seguro corrigir; para o resto, avisa e (no caso do yt-dlp.exe)
        /// oferece o reparo. Atalhos: Ctrl+Shift+D (roda e mostra o resumo) e Ctrl+Shift+L (abre a pasta de logs).
        /// </summary>
        private async Task ExecutarAutoDiagnosticoAsync(bool mostrarResumo)
        {
            try
            {
                var resultados = await Task.Run(() => AutoReparo.VerificarInicio(_ytDlpPath, _ffmpegPath, _pastaAssets));
                if (IsDisposed)
                    return;

                int corrigidos = resultados.Count(r => r.Situacao == SituacaoVerificacao.Corrigido);

                if (!File.Exists(_ytDlpPath))
                    AtualizarStatus("yt-dlp.exe não encontrado.");
                else if (corrigidos > 0)
                    AtualizarStatus("Autodiagnóstico: " + corrigidos + " problema(s) corrigido(s) automaticamente.");

                if (mostrarResumo)
                {
                    MessageBox.Show(this,
                        AutoReparo.GerarResumo(resultados) + "\nLogs gravados em:\n" + GerenciadorLog.PastaAtual,
                        "Autodiagnóstico", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                var ytDlp = resultados.Find(r => r.Item == "yt-dlp.exe");
                if (ytDlp is not null && !_ocupado &&
                    (ytDlp.Situacao == SituacaoVerificacao.Falha || ytDlp.Situacao == SituacaoVerificacao.Aviso))
                {
                    _ocupado = true;
                    try
                    {
                        await OferecerBaixarYtDlpAsync("Problema com o yt-dlp.exe: " + ytDlp.Detalhe + ".");
                    }
                    finally
                    {
                        _ocupado = false;
                    }
                }
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Falha no autodiagnóstico", ex);
            }
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Shift | Keys.L))
            {
                AbrirNoExplorador(GerenciadorLog.PastaAtual);
                return true;
            }

            if (keyData == (Keys.Control | Keys.Shift | Keys.D))
            {
                if (!_ocupado)
                    _ = ExecutarAutoDiagnosticoAsync(mostrarResumo: true);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            bool cancelarFechamento = false;

            try
            {
                bool baixando = _processoAtual is { HasExited: false };

                // Fechar no meio de um download cancela o download e deixa arquivo incompleto:
                // antes isso acontecia sem nenhum aviso.
                if (baixando && e.CloseReason == CloseReason.UserClosing)
                {
                    var resposta = MessageBox.Show(this,
                        "Há um download em andamento. Se você fechar agora, ele será cancelado e o arquivo ficará incompleto.\n\n" +
                        "Deseja fechar mesmo assim?",
                        "Download em andamento", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (resposta != DialogResult.Yes)
                        cancelarFechamento = true;
                }

                if (!cancelarFechamento)
                {
                    _ctsPreviaCts?.Cancel(); // encerra a busca de prévia em andamento (e o yt-dlp dela)
                    _timerSpinner?.Stop();

                    if (_processoAtual is { HasExited: false })
                    {
                        _canceladoPeloUsuario = true;
                        GerenciadorLog.Aviso("O programa foi fechado com um download em andamento; o download foi cancelado.");
                        _processoAtual.Kill(entireProcessTree: true);
                    }

                    ProcessosFilhos.EncerrarTodos();
                    notifyIcon1.Visible = false;
                }
            }
            catch (Exception ex)
            {
                // Falhas ao encerrar durante o fechamento não devem impedir o app de fechar,
                // mas ficam registradas.
                GerenciadorLog.Aviso("Problema ao encerrar processos no fechamento do programa: " + ex.Message);
            }

            if (cancelarFechamento)
                e.Cancel = true;

            base.OnFormClosing(e);
        }
    }
}

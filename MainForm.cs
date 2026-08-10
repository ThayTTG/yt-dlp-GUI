using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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
        private string? _ultimoErro;
        private bool _avisoBotMostrado;

        private static readonly Regex RegexProgresso =
            new Regex(@"\[download\]\s+(\d{1,3}(?:\.\d+)?)%", RegexOptions.Compiled);
        private static readonly Regex RegexDestino =
            new Regex(@"Destination:\s*(.+)$", RegexOptions.Compiled);
        private static readonly Regex RegexErro =
            new Regex(@"^ERROR:\s*(.+)$", RegexOptions.Compiled);

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
            picLogoGrande.Image = CarregarImagem("logo_home_grande.png");
            picIlustracaoPasta.Image = CarregarImagem("ilustracao_pasta.png");

            AtualizarIconesDeNavegacao("home");
        }

        /// <summary>Troca os ícones da barra lateral entre azul (ativo) e branco (inativo).</summary>
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

            var argumentos = new System.Collections.Generic.List<string>
            {
                txtUrl.Text.Trim(),
                "-f", "bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/bestvideo+bestaudio/best",
                "--merge-output-format", "mp4",
                "--newline",
                "-P", txtSavePath.Text,
                "-o", "%(title)s.%(ext)s"
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

            var argumentos = new System.Collections.Generic.List<string>
            {
                txtUrl.Text.Trim(),
                "-x",
                "--audio-format", "mp3",
                "--newline",
                "-P", txtSavePath.Text,
                "-o", "%(title)s.%(ext)s"
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

        private void AdicionarArgumentoDePlaylist(System.Collections.Generic.List<string> argumentos)
        {
            argumentos.Add(chkPlaylist.Checked ? "--yes-playlist" : "--no-playlist");
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
                }
                else if (processo.ExitCode == 0)
                {
                    if (string.IsNullOrEmpty(tipoParaHistorico))
                    {
                        // Ação sem histórico associado (ex.: "yt-dlp -U").
                        AtualizarStatus("Atualização concluída!");
                        MostrarNotificacao("yt-dlp atualizado", "O yt-dlp foi verificado/atualizado com sucesso.");
                    }
                    else
                    {
                        AtualizarStatus("Download concluído!");
                        AtualizarProgresso(100);

                        string nomeArquivo = string.IsNullOrEmpty(_ultimoArquivoBaixado)
                            ? "(nome não identificado)"
                            : Path.GetFileName(_ultimoArquivoBaixado);

                        GerenciadorHistorico.Adicionar(new HistoricoItem
                        {
                            DataHora = DateTime.Now,
                            Tipo = tipoParaHistorico,
                            Titulo = nomeArquivo,
                            Pasta = txtSavePath.Text,
                            Url = txtUrl.Text.Trim()
                        });

                        MostrarNotificacao("Download concluído", nomeArquivo);
                    }
                }
                else
                {
                    string mensagem = TraduzirErro(_ultimoErro);
                    AtualizarStatus("Falha no download.");
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

        /// <summary>Traduz o erro mais comum do yt-dlp para uma frase amigável.</summary>
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
                AtualizarStatus($"Baixando... {percentual:0}%");
            }

            var matchDestino = RegexDestino.Match(linha);
            if (matchDestino.Success)
            {
                _ultimoArquivoBaixado = matchDestino.Groups[1].Value.Trim();
            }

            if (linha.Contains("[ExtractAudio]") || linha.Contains("Extracting audio"))
                AtualizarStatus("Convertendo para MP3...");
            else if (linha.Contains("[Merger]") || linha.Contains("Merging formats"))
                AtualizarStatus("Finalizando arquivo...");
            else if (linha.Contains("Extracting URL"))
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
            btnAlterarLocal.Enabled = !emAndamento;
            btnAtualizarYtDlp.Enabled = !emAndamento;
            radSemCookies.Enabled = !emAndamento;
            radCookiesNavegador.Enabled = !emAndamento;
            radCookiesArquivo.Enabled = !emAndamento;
            cmbNavegador.Enabled = !emAndamento && radCookiesNavegador.Checked;
            txtCookiesArquivo.Enabled = !emAndamento && radCookiesArquivo.Checked;
            btnSelecionarCookies.Enabled = !emAndamento && radCookiesArquivo.Checked;

            btnCancelar.Enabled = emAndamento;

            Cursor = emAndamento ? Cursors.AppStarting : Cursors.Default;
        }

        // ---------------------------------------------------------------
        // Página Configurações (autenticação + atualizar yt-dlp)
        // ---------------------------------------------------------------

        private void OpcaoDeAutenticacao_CheckedChanged(object sender, EventArgs e)
        {
            cmbNavegador.Enabled = radCookiesNavegador.Checked;
            txtCookiesArquivo.Enabled = radCookiesArquivo.Checked;
            btnSelecionarCookies.Enabled = radCookiesArquivo.Checked;
        }

        private void btnSelecionarCookies_Click(object sender, EventArgs e)
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

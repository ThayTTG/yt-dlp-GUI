using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YtDlpGui
{
    /// <summary>
    /// Formulário principal da aplicação.
    /// Fornece uma interface amigável para o utilitário de linha de comando yt-dlp,
    /// permitindo baixar vídeos (MP4) ou extrair áudio (MP3) do YouTube sem que o
    /// aluno precise abrir o CMD/PowerShell.
    /// </summary>
    public partial class MainForm : Form
    {
        // Caminho completo esperado para o executável do yt-dlp.
        // Por padrão, o aplicativo procura o yt-dlp.exe na MESMA PASTA do .exe da aplicação.
        private readonly string _ytDlpPath;

        // Caminho completo esperado para o executável do ffmpeg (necessário para
        // converter/mesclar áudio e vídeo, e obrigatório para gerar MP3).
        private readonly string _ffmpegPath;

        // Pasta onde o ffmpeg.exe está localizado (usada no parâmetro --ffmpeg-location).
        private readonly string _ffmpegDir;

        // Referência ao processo em execução, usada pelo botão "Cancelar".
        private Process? _processoAtual;

        // Indica se o processo atual foi encerrado a pedido do usuário
        // (usado para exibir uma mensagem adequada em vez de "erro").
        private bool _canceladoPeloUsuario;

        // Expressão regular para capturar a porcentagem de progresso que o yt-dlp
        // imprime no formato: "[download]  45.2% of ..."
        private static readonly Regex RegexProgresso =
            new Regex(@"\[download\]\s+(\d{1,3}(?:\.\d+)?)%", RegexOptions.Compiled);

        public MainForm()
        {
            InitializeComponent();

            // yt-dlp.exe e ffmpeg.exe devem estar na mesma pasta do executável da aplicação.
            string pastaApp = AppDomain.CurrentDomain.BaseDirectory;
            _ytDlpPath = Path.Combine(pastaApp, "yt-dlp.exe");
            _ffmpegPath = Path.Combine(pastaApp, "ffmpeg.exe");
            _ffmpegDir = pastaApp;

            // Define uma pasta padrão de salvamento (Downloads do usuário).
            txtSavePath.Text = ObterPastaDownloadsPadrao();
        }

        /// <summary>
        /// Evento disparado quando o formulário termina de carregar.
        /// Usado para validar se as dependências externas (yt-dlp/ffmpeg) existem.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            VerificarDependencias();
        }

        /// <summary>
        /// Retorna o caminho da pasta "Downloads" do usuário atual, com fallback
        /// para a área de trabalho caso não seja possível determiná-la.
        /// </summary>
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

        /// <summary>
        /// Verifica se o yt-dlp.exe e o ffmpeg.exe existem na pasta da aplicação
        /// e exibe um aviso no log de status caso algo esteja faltando.
        /// Não impede a abertura do programa, apenas avisa o usuário.
        /// </summary>
        private void VerificarDependencias()
        {
            AdicionarStatus("Verificando dependências...");

            if (!File.Exists(_ytDlpPath))
            {
                AdicionarStatus($"⚠️ ATENÇÃO: yt-dlp.exe não encontrado em: {_ytDlpPath}");
                AdicionarStatus("Coloque o arquivo yt-dlp.exe na mesma pasta deste programa antes de baixar.");
            }
            else
            {
                AdicionarStatus("✔ yt-dlp.exe encontrado.");
            }

            if (!File.Exists(_ffmpegPath))
            {
                AdicionarStatus($"⚠️ ATENÇÃO: ffmpeg.exe não encontrado em: {_ffmpegPath}");
                AdicionarStatus("A conversão para MP3 e a mesclagem de vídeo+áudio em MP4 podem falhar sem o ffmpeg.");
            }
            else
            {
                AdicionarStatus("✔ ffmpeg.exe encontrado.");
            }

            AdicionarStatus("Pronto para uso. Cole a URL do vídeo e escolha uma opção de download.");
        }

        /// <summary>
        /// Abre o diálogo de seleção de pasta para o usuário escolher o local de salvamento.
        /// </summary>
        private void btnSelectFolder_Click(object sender, EventArgs e)
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

        /// <summary>
        /// Botão "Baixar MP4 (Vídeo)": baixa a melhor qualidade de vídeo+áudio
        /// disponíveis, mesclando-os em um único arquivo .mp4.
        /// </summary>
        private async void btnDownloadMp4_Click(object sender, EventArgs e)
        {
            if (!ValidarEntradas())
                return;

            var argumentos = new System.Collections.Generic.List<string>
            {
                txtUrl.Text.Trim(),
                "-f", "bestvideo+bestaudio/best",
                "--merge-output-format", "mp4",
                "--newline",
                "-P", txtSavePath.Text,
                "-o", "%(title)s.%(ext)s"
            };

            AdicionarArgumentoDePlaylist(argumentos);

            if (File.Exists(_ffmpegPath))
            {
                argumentos.Add("--ffmpeg-location");
                argumentos.Add(_ffmpegDir);
            }

            await ExecutarYtDlpAsync(argumentos, "Download de vídeo (MP4)");
        }

        /// <summary>
        /// Botão "Baixar MP3 (Áudio)": extrai apenas o áudio do vídeo e converte
        /// para o formato MP3 usando o ffmpeg.
        /// </summary>
        private async void btnDownloadMp3_Click(object sender, EventArgs e)
        {
            if (!ValidarEntradas())
                return;

            var argumentos = new System.Collections.Generic.List<string>
            {
                txtUrl.Text.Trim(),
                "-x", // extrai apenas o áudio
                "--audio-format", "mp3",
                "--newline",
                "-P", txtSavePath.Text,
                "-o", "%(title)s.%(ext)s"
            };

            AdicionarArgumentoDePlaylist(argumentos);

            if (File.Exists(_ffmpegPath))
            {
                argumentos.Add("--ffmpeg-location");
                argumentos.Add(_ffmpegDir);
            }

            await ExecutarYtDlpAsync(argumentos, "Download de áudio (MP3)");
        }

        /// <summary>
        /// Adiciona à lista de argumentos a flag correta de playlist, de acordo
        /// com o estado do checkbox "Baixar playlist inteira":
        /// - Desmarcado (padrão): "--no-playlist", baixa somente o vídeo do link.
        /// - Marcado: "--yes-playlist", baixa todos os vídeos da playlist do link.
        /// </summary>
        private void AdicionarArgumentoDePlaylist(System.Collections.Generic.List<string> argumentos)
        {
            argumentos.Add(chkPlaylist.Checked ? "--yes-playlist" : "--no-playlist");
        }

        /// <summary>
        /// Botão "Cancelar": encerra o processo do yt-dlp em execução (e seus
        /// processos filhos, como o ffmpeg) de forma imediata.
        /// </summary>
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (_processoAtual is null || _processoAtual.HasExited)
                return;

            try
            {
                AdicionarStatus("⏹ Cancelando o download a pedido do usuário...");
                _canceladoPeloUsuario = true;
                _processoAtual.Kill(entireProcessTree: true);
            }
            catch (Exception ex)
            {
                AdicionarStatus($"❌ Não foi possível cancelar o processo: {ex.Message}");
            }
        }

        /// <summary>
        /// Valida os campos obrigatórios antes de iniciar um download:
        /// URL preenchida, pasta de destino válida e yt-dlp.exe presente.
        /// </summary>
        private bool ValidarEntradas()
        {
            if (string.IsNullOrWhiteSpace(txtUrl.Text))
            {
                MessageBox.Show(this, "Por favor, cole a URL do vídeo do YouTube antes de continuar.",
                    "URL não informada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrl.Focus();
                return false;
            }

            if (!Uri.TryCreate(txtUrl.Text.Trim(), UriKind.Absolute, out _))
            {
                MessageBox.Show(this, "A URL informada não parece ser válida. Verifique e tente novamente.",
                    "URL inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrl.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtSavePath.Text) || !Directory.Exists(txtSavePath.Text))
            {
                MessageBox.Show(this, "Selecione uma pasta de destino válida antes de continuar.",
                    "Pasta inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!File.Exists(_ytDlpPath))
            {
                MessageBox.Show(this,
                    $"Não foi possível encontrar o yt-dlp.exe em:\n{_ytDlpPath}\n\n" +
                    "Baixe o yt-dlp.exe (github.com/yt-dlp/yt-dlp/releases) e coloque-o " +
                    "na mesma pasta deste programa.",
                    "yt-dlp.exe não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Executa o yt-dlp.exe de forma assíncrona e totalmente oculta (sem abrir
        /// nenhuma janela de console), capturando a saída padrão e de erro em tempo
        /// real para atualizar o log de status na interface.
        /// </summary>
        /// <param name="argumentos">Lista de argumentos a passar para o yt-dlp (sem escaping manual).</param>
        /// <param name="descricaoAcao">Texto descritivo da ação, exibido no início do log.</param>
        private async System.Threading.Tasks.Task ExecutarYtDlpAsync(
            System.Collections.Generic.List<string> argumentos, string descricaoAcao)
        {
            _canceladoPeloUsuario = false;
            DefinirEstadoDownloadEmAndamento(true);
            LimparStatus();
            AdicionarStatus($"▶ Iniciando: {descricaoAcao}");
            AdicionarStatus($"URL: {txtUrl.Text.Trim()}");
            AdicionarStatus($"Destino: {txtSavePath.Text}");
            AdicionarStatus("Processando URL...");

            var psi = new ProcessStartInfo
            {
                FileName = _ytDlpPath,
                UseShellExecute = false,        // obrigatório para redirecionar a saída
                CreateNoWindow = true,           // garante que NENHUMA janela de console apareça
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            // Usar ArgumentList evita problemas de escaping manual com espaços/aspas
            // (por exemplo, em URLs com parâmetros ou em pastas com espaço no nome).
            foreach (var arg in argumentos)
            {
                psi.ArgumentList.Add(arg);
            }

            using var processo = new Process { StartInfo = psi, EnableRaisingEvents = true };

            // Os eventos abaixo são disparados em uma thread de segundo plano,
            // por isso repassamos a linha para o método que já trata a
            // atualização segura da interface (via Invoke).
            processo.OutputDataReceived += (s, e) => ProcessarLinhaDeSaida(e.Data);
            processo.ErrorDataReceived += (s, e) => ProcessarLinhaDeSaida(e.Data);

            try
            {
                _processoAtual = processo;

                processo.Start();
                processo.BeginOutputReadLine();
                processo.BeginErrorReadLine();

                // Aguarda o término do processo sem travar a interface (thread da UI livre).
                await processo.WaitForExitAsync();

                if (_canceladoPeloUsuario)
                {
                    AdicionarStatus("⏹ Download cancelado pelo usuário.");
                }
                else if (processo.ExitCode == 0)
                {
                    AdicionarStatus("✅ Download concluído com sucesso!");
                    AtualizarProgresso(100);
                }
                else
                {
                    AdicionarStatus($"⚠️ O yt-dlp terminou com código de saída {processo.ExitCode}. " +
                                     "Verifique as mensagens acima para mais detalhes.");
                }
            }
            catch (System.ComponentModel.Win32Exception exWin32)
            {
                // Ocorre tipicamente quando o executável não pode ser iniciado
                // (permissão negada, arquivo corrompido, antivírus bloqueando, etc.).
                AdicionarStatus($"❌ Erro ao iniciar o yt-dlp.exe: {exWin32.Message}");
            }
            catch (Exception ex)
            {
                AdicionarStatus($"❌ Erro inesperado: {ex.Message}");
            }
            finally
            {
                _processoAtual = null;
                DefinirEstadoDownloadEmAndamento(false);
            }
        }

        /// <summary>
        /// Trata cada linha de saída (stdout/stderr) recebida do yt-dlp,
        /// repassando-a de forma thread-safe para a interface gráfica.
        /// </summary>
        private void ProcessarLinhaDeSaida(string? linha)
        {
            if (string.IsNullOrWhiteSpace(linha))
                return;

            // Os eventos de saída do Process rodam em uma thread separada da UI;
            // por isso é necessário usar Invoke/BeginInvoke para tocar em controles.
            if (rtbStatus.IsHandleCreated && rtbStatus.InvokeRequired)
            {
                rtbStatus.BeginInvoke(new Action(() => TratarLinhaNaUi(linha)));
            }
            else
            {
                TratarLinhaNaUi(linha);
            }
        }

        /// <summary>
        /// Já rodando na thread da UI: adiciona a linha ao log e, se for uma
        /// linha de progresso do yt-dlp, atualiza a barra de progresso.
        /// </summary>
        private void TratarLinhaNaUi(string linha)
        {
            AdicionarStatus(linha);

            var match = RegexProgresso.Match(linha);
            if (match.Success && double.TryParse(
                    match.Groups[1].Value,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double percentual))
            {
                AtualizarProgresso(percentual);
            }

            // Alguns estágios comuns do yt-dlp, traduzidos para facilitar o entendimento do aluno.
            if (linha.Contains("Extracting URL"))
                AtualizarRotuloEstagio("Processando URL...");
            else if (linha.Contains("[download] Destination"))
                AtualizarRotuloEstagio("Baixando arquivo...");
            else if (linha.Contains("[ExtractAudio]") || linha.Contains("Extracting audio"))
                AtualizarRotuloEstagio("Convertendo para MP3...");
            else if (linha.Contains("[Merger]") || linha.Contains("Merging formats"))
                AtualizarRotuloEstagio("Mesclando vídeo e áudio...");
            else if (linha.Contains("has already been downloaded"))
                AtualizarRotuloEstagio("Arquivo já existente, pulando download...");
        }

        /// <summary>
        /// Atualiza o texto de status geral (não é o título do rótulo de estágio,
        /// mas sim o log completo exibido no RichTextBox).
        /// </summary>
        private void AtualizarRotuloEstagio(string texto)
        {
            // Reaproveita o título acima do log para indicar o estágio atual.
            lblStatusTitle.Text = $"Estágio do download: {texto}";
        }

        /// <summary>
        /// Adiciona uma linha de texto ao final do log de status e rola
        /// automaticamente até o final.
        /// </summary>
        private void AdicionarStatus(string mensagem)
        {
            rtbStatus.AppendText(mensagem + Environment.NewLine);
            rtbStatus.SelectionStart = rtbStatus.TextLength;
            rtbStatus.ScrollToCaret();
        }

        /// <summary>
        /// Limpa o log de status e reseta a barra de progresso.
        /// </summary>
        private void LimparStatus()
        {
            rtbStatus.Clear();
            AtualizarProgresso(0);
            lblStatusTitle.Text = "Estágio do download:";
        }

        /// <summary>
        /// Atualiza a barra de progresso e o rótulo de porcentagem.
        /// </summary>
        private void AtualizarProgresso(double percentual)
        {
            int valor = Math.Clamp((int)Math.Round(percentual), 0, 100);
            progressBar1.Value = valor;
            lblProgressPercent.Text = $"{valor}%";
        }

        /// <summary>
        /// Habilita/desabilita os controles de interação enquanto um download
        /// está em andamento, evitando que o usuário dispare execuções simultâneas.
        /// </summary>
        private void DefinirEstadoDownloadEmAndamento(bool emAndamento)
        {
            btnDownloadMp4.Enabled = !emAndamento;
            btnDownloadMp3.Enabled = !emAndamento;
            btnSelectFolder.Enabled = !emAndamento;
            txtUrl.Enabled = !emAndamento;
            chkPlaylist.Enabled = !emAndamento;

            // O botão Cancelar só faz sentido enquanto há um download em andamento.
            btnCancelar.Enabled = emAndamento;

            Cursor = emAndamento ? Cursors.AppStarting : Cursors.Default;
        }

        /// <summary>
        /// Garante que, se a janela for fechada durante um download, o processo
        /// filho do yt-dlp também seja encerrado (evita processos "fantasma").
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (_processoAtual is { HasExited: false })
                {
                    _processoAtual.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Ignora falhas ao tentar encerrar o processo durante o fechamento do app.
            }

            base.OnFormClosing(e);
        }
    }
}

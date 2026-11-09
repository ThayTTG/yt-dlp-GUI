using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using NokyLabs.UI;

namespace YtDlpGui
{
    /// <summary>Uma linha da lista do Histórico.</summary>
    public sealed record LinhaHistorico(HistoricoItem Item)
    {
        public string Tipo => Item.Tipo;
        public string Titulo => Item.Titulo;
        public string Data => Item.DataHora.ToString("dd/MM/yyyy HH:mm");
        public bool EhVideo => Item.Tipo == "MP4";
        public bool EhAudio => Item.Tipo != "MP4";
    }

    /// <summary>
    /// Janela principal: barra lateral de navegação (Início, Histórico, Pasta, Configurações)
    /// + barra de status inferior com progresso. Não exibe a saída bruta do yt-dlp (sem
    /// "terminal") — tudo é traduzido para mensagens curtas e amigáveis, e o download avisa
    /// por notificação do Windows quando termina ou falha.
    ///
    /// A lógica é a mesma do antigo MainForm.cs (WinForms), trazida para o Avalonia: o que
    /// mudou foi só a tela (MessageBox → Dialogo, Invoke → Dispatcher, diálogos de pasta e
    /// arquivo do Avalonia). As regras sem tela ficam em RegrasDoDownload.cs.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _ytDlpPath;
        private readonly string _ffmpegPath;
        private readonly string _ffmpegDir;

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
        private bool _ocupado;

        // Autorreparo: atualizar o yt-dlp e repetir o download só UMA vez por sessão (sem laços).
        private bool _jaTentouAtualizarNestaSessao;

        // Sugestão automática de cookies: feita só uma vez por sessão, e só se o download falhar por falta de login.
        private bool _jaOfereceuCookiesNestaSessao;
        private bool _ultimoDownloadFoiMp3;

        // A janela já fechou: nada mais é desenhado (as linhas do yt-dlp chegam por outra thread).
        private bool _fechada;
        // O usuário já confirmou que quer fechar no meio de um download.
        private bool _fecharConfirmado;

        // Prévia de vídeo (Início)
        private CancellationTokenSource? _ctsPreviaCts;
        private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
        private readonly DispatcherTimer _timerSpinner = new() { Interval = TimeSpan.FromMilliseconds(100) };
        private int _spinnerFrame;
        private static readonly string[] SpinnerFrames = { "⠋", "⠙", "⠸", "⠴", "⠦", "⠇" };

        public MainWindow()
        {
            InitializeComponent();
            LigarEventos();

            string pastaApp = AppDomain.CurrentDomain.BaseDirectory;
            _ytDlpPath = Path.Combine(pastaApp, "yt-dlp.exe");
            _ffmpegPath = Path.Combine(pastaApp, "ffmpeg.exe");
            _ffmpegDir = pastaApp;

            TxtPasta.Text = RegrasDoDownload.ObterPastaDownloadsPadrao();
            CmbNavegador.ItemsSource = RegrasDoDownload.Navegadores;
            CmbNavegador.SelectedIndex = 0; // "chrome"

            var versao = Assembly.GetExecutingAssembly().GetName().Version;
            if (versao is not null)
                Creditos.Version = $"versão {versao.Major}.{versao.Minor}";

            _timerSpinner.Tick += SpinnerTick;
            MostrarPagina("home");
        }

        private void LigarEventos()
        {
            foreach (var nav in new[] { NavInicio, NavHistorico, NavPasta, NavConfig })
                nav.IsCheckedChanged += (s, _) =>
                {
                    if (s is RadioButton { IsChecked: true, Tag: string pagina }) MostrarPagina(pagina);
                };

            TxtUrl.TextChanged += TxtUrl_TextChanged;
            BtnMp4.Click += async (_, _) => await IniciarDownloadAsync(mp3: false);
            BtnMp3.Click += async (_, _) => await IniciarDownloadAsync(mp3: true);
            BtnCancelar.Click += (_, _) => Cancelar();

            BtnLimparHistorico.Click += async (_, _) => await LimparHistoricoAsync();
            ListaHistorico.DoubleTapped += async (_, _) => await AbrirPastaDoItemAsync();

            BtnAlterarLocal.Click += async (_, _) => await AlterarLocalAsync();
            BtnAbrirPasta.Click += async (_, _) => await AbrirPastaDeDestinoAsync();

            RadSemCookies.IsCheckedChanged += (_, _) => AtualizarCamposDeCookies();
            RadCookiesNavegador.IsCheckedChanged += (_, _) => AtualizarCamposDeCookies();
            RadCookiesArquivo.IsCheckedChanged += (_, _) => AtualizarCamposDeCookies();
            BtnAjudaCookies.Click += async (_, _) => await AbrirAjudaCookiesAsync();
            BtnExtrairCookies.Click += async (_, _) => await ExtrairCookiesPeloBotaoAsync();
            // O campo do cookies.txt é clicável (substitui o antigo botão "Buscar").
            TxtCookiesArquivo.AddHandler(PointerReleasedEvent, async (_, _) => await EscolherArquivoCookiesAsync(),
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
            BtnAtualizarYtDlp.Click += async (_, _) => await AtualizarYtDlpPeloBotaoAsync();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            RegistrarLog($"App iniciado. yt-dlp existe: {File.Exists(_ytDlpPath)} | ffmpeg existe: {File.Exists(_ffmpegPath)}");

            // O autodiagnóstico roda depois que a janela já apareceu, para não atrasar a abertura.
            Dispatcher.UIThread.Post(async () => await ExecutarAutoDiagnosticoAsync(mostrarResumo: false), DispatcherPriority.Background);
        }

        // ---------------------------------------------------------------
        // Prévia de vídeo (Início)
        // ---------------------------------------------------------------

        /// <summary>
        /// Disparado quando o link muda (com espera: só consulta o yt-dlp se o usuário parar de
        /// digitar por 600 ms, evitando chamadas a cada tecla).
        /// </summary>
        private async void TxtUrl_TextChanged(object? sender, TextChangedEventArgs e)
        {
            // Cancela qualquer busca anterior ainda em andamento (e encerra o yt-dlp dela).
            _ctsPreviaCts?.Cancel();
            _ctsPreviaCts = new CancellationTokenSource();
            var token = _ctsPreviaCts.Token;

            string url = (TxtUrl.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(url) || !RegrasDoDownload.UrlValida(url))
            {
                MostrarLogoPadrao();
                return;
            }

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
                // Só desliga o spinner se esta ainda for a busca mais recente.
                if (!token.IsCancellationRequested) PararSpinner();
            }
        }

        /// <summary>Roda yt-dlp --dump-json (sem baixar) e extrai título, canal e miniatura do vídeo.</summary>
        private async Task<PreviaVideo?> BuscarPreviaAsync(string url, CancellationToken token)
        {
            if (!File.Exists(_ytDlpPath)) return null;

            var psi = new ProcessStartInfo { FileName = _ytDlpPath };
            foreach (var arg in RegrasDoDownload.ArgumentosDaPrevia(url))
                psi.ArgumentList.Add(arg);

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

            string titulo, canal, thumbUrl, duracao;

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

            Bitmap? thumbImg = null;
            if (!string.IsNullOrEmpty(thumbUrl) && RegrasDoDownload.UrlValida(thumbUrl))
            {
                try
                {
                    byte[] bytes = await _httpClient.GetByteArrayAsync(thumbUrl, token);
                    using var ms = new MemoryStream(bytes);
                    thumbImg = new Bitmap(ms);
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

        private sealed record PreviaVideo(string Titulo, string Canal, string Duracao, Bitmap? Thumbnail);

        /// <summary>Exibe os dados da prévia no cartão do Início.</summary>
        private void ExibirPrevia(PreviaVideo previa, CancellationToken token)
        {
            if (token.IsCancellationRequested || _fechada) return;

            PainelLogo.IsVisible = false;
            PainelPrevia.IsVisible = true;

            // Libera a miniatura anterior.
            var anterior = ImgThumb.Source as Bitmap;
            ImgThumb.Source = previa.Thumbnail;
            if (anterior is not null && !ReferenceEquals(anterior, previa.Thumbnail))
                anterior.Dispose();

            TxtTituloVideo.Text = RegrasDoDownload.EncurtarTitulo(previa.Titulo);
            TxtInfoVideo.Text = RegrasDoDownload.InfoDaPrevia(previa.Canal, previa.Duracao);
        }

        /// <summary>Volta a exibir o logo padrão e esconde a prévia.</summary>
        private void MostrarLogoPadrao()
        {
            if (_fechada) return;
            PararSpinner();
            PainelPrevia.IsVisible = false;
            PainelLogo.IsVisible = true;
        }

        private void IniciarSpinner()
        {
            if (_fechada) return;
            PainelLogo.IsVisible = false;
            PainelPrevia.IsVisible = false;
            TxtSpinner.IsVisible = true;
            _spinnerFrame = 0;
            TxtSpinner.Text = SpinnerFrames[0] + " Buscando informações...";
            _timerSpinner.Start();
        }

        private void PararSpinner()
        {
            _timerSpinner.Stop();
            if (!_fechada) TxtSpinner.IsVisible = false;
        }

        private void SpinnerTick(object? sender, EventArgs e)
        {
            _spinnerFrame = (_spinnerFrame + 1) % SpinnerFrames.Length;
            TxtSpinner.Text = SpinnerFrames[_spinnerFrame] + " Buscando informações...";
        }

        // ---------------------------------------------------------------
        // Navegação entre páginas
        // ---------------------------------------------------------------

        private void MostrarPagina(string pagina)
        {
            PaginaInicio.IsVisible = pagina == "home";
            PaginaHistorico.IsVisible = pagina == "historico";
            PaginaPasta.IsVisible = pagina == "pasta";
            PaginaConfig.IsVisible = pagina == "config";

            // Mantém a barra lateral em sincronia quando a troca vem do código (ex.: "URL inválida" leva ao Início).
            var nav = pagina switch { "historico" => NavHistorico, "pasta" => NavPasta, "config" => NavConfig, _ => NavInicio };
            if (nav.IsChecked != true) nav.IsChecked = true;

            var visivel = pagina switch { "historico" => PaginaHistorico, "pasta" => PaginaPasta, "config" => PaginaConfig, _ => PaginaInicio };
            if (visivel.Content is Control cartao) Anim.PlayEntrance(cartao);

            if (pagina == "historico")
                CarregarHistorico();
        }

        // O logo também leva ao Início.
        private void OnLogoPressed(object? sender, PointerPressedEventArgs e) => MostrarPagina("home");

        // ---------------------------------------------------------------
        // Página Pasta (local de salvamento)
        // ---------------------------------------------------------------

        private string PastaDestino => TxtPasta.Text ?? "";

        private async Task AlterarLocalAsync()
        {
            string atual = Directory.Exists(PastaDestino) ? PastaDestino : RegrasDoDownload.ObterPastaDownloadsPadrao();
            var inicial = await StorageProvider.TryGetFolderFromPathAsync(atual);

            var escolhidas = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Selecione a pasta onde o arquivo será salvo",
                AllowMultiple = false,
                SuggestedStartLocation = inicial,
            });

            var pasta = escolhidas.FirstOrDefault()?.TryGetLocalPath();
            if (!string.IsNullOrEmpty(pasta))
                TxtPasta.Text = pasta;
        }

        /// <summary>Botão da página Pasta: abre a pasta de destino no Explorador de Arquivos.</summary>
        private async Task AbrirPastaDeDestinoAsync()
        {
            string pasta = PastaDestino;
            if (string.IsNullOrWhiteSpace(pasta) || !Directory.Exists(pasta))
            {
                await Dialogo.MostrarAsync(this,
                    "Essa pasta não existe mais. Use \"Alterar pasta\" para escolher outra.",
                    "Pasta não encontrada", TipoDialogo.Aviso);
                return;
            }

            await AbrirNoExploradorAsync(pasta);
        }

        // ---------------------------------------------------------------
        // Página Início (downloads)
        // ---------------------------------------------------------------

        /// <summary>
        /// Fluxo único para MP4 e MP3. Pontos de segurança:
        ///   - Trava _ocupado: um segundo clique durante a verificação não inicia outro download.
        ///   - A URL é "fotografada" uma única vez (não dá para trocá-la durante o filtro de conteúdo).
        ///   - Os controles são travados antes do filtro, não só depois de ele terminar.
        ///   - Qualquer erro vai para o log em vez de derrubar o programa.
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

                if (!await ValidarEntradasAsync(exigeFfmpeg: mp3))
                    return;

                string url = (TxtUrl.Text ?? "").Trim();

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
                await MostrarErroInesperadoAsync();
            }
            finally
            {
                _ocupado = false;
                DefinirEstadoDownloadEmAndamento(false);
            }
        }

        private List<string> ConstruirArgumentos(string url, bool mp3) =>
            RegrasDoDownload.ConstruirArgumentos(new OpcoesDownload(
                Url: url,
                Mp3: mp3,
                PastaDestino: PastaDestino,
                BaixarPlaylist: ChkPlaylist.IsChecked == true,
                OrganizarPlaylist: ChkOrganizarPlaylist.IsChecked == true,
                Cookies: RadCookiesNavegador.IsChecked == true ? ModoCookies.Navegador
                       : RadCookiesArquivo.IsChecked == true ? ModoCookies.Arquivo
                       : ModoCookies.Nenhum,
                Navegador: CmbNavegador.SelectedItem as string,
                ArquivoCookies: TxtCookiesArquivo.Text,
                PastaFfmpeg: File.Exists(_ffmpegPath) ? _ffmpegDir : null));

        private async Task MostrarErroInesperadoAsync()
        {
            if (_fechada)
                return;

            await Dialogo.MostrarAsync(this,
                "Ocorreu um erro inesperado, mas o programa continua funcionando.\n\n" +
                "Os detalhes técnicos foram salvos em:\n" + GerenciadorLog.CaminhoErros,
                "Erro inesperado", TipoDialogo.Aviso);
        }

        // =================================================================
        // MÓDULO OPCIONAL: integração com FiltroConteudo.cs
        // Sem "filtro-conteudo.json" na pasta do .exe, CarregarConfiguracao()
        // retorna Habilitado=false e este método sempre retorna true de
        // imediato (nenhum custo extra, nenhuma chamada ao yt-dlp).
        // =================================================================

        /// <summary>
        /// Retorna true se o download pode prosseguir (filtro desligado, URL permitida, ou código
        /// mestre digitado corretamente). Retorna false se o usuário cancelou ou não sabia o código.
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

                bool liberadoPeloCodigo = await Dialogo.PedirCodigoMestreAsync(this, configFiltro, resultado.Motivo ?? "Conteúdo bloqueado.");

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
                if (!_fechada)
                {
                    await Dialogo.MostrarAsync(this,
                        "Não foi possível verificar o conteúdo deste endereço agora, então o download não foi iniciado.\n\n" +
                        "Tente novamente em instantes.",
                        "Verificação não concluída", TipoDialogo.Aviso);
                }
                return false;
            }
        }

        private void Cancelar()
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

        private async Task<bool> ValidarEntradasAsync(bool exigeFfmpeg)
        {
            string url = TxtUrl.Text ?? "";

            if (string.IsNullOrWhiteSpace(url))
            {
                await Dialogo.MostrarAsync(this, "Cole a URL do vídeo antes de continuar.", "URL não informada", TipoDialogo.Aviso);
                MostrarPagina("home");
                TxtUrl.Focus();
                return false;
            }

            if (!RegrasDoDownload.UrlValida(url.Trim()))
            {
                await Dialogo.MostrarAsync(this,
                    "A URL informada não parece ser válida.\nUse um endereço que comece com http:// ou https://",
                    "URL inválida", TipoDialogo.Aviso);
                MostrarPagina("home");
                TxtUrl.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PastaDestino) || !Directory.Exists(PastaDestino))
            {
                await Dialogo.MostrarAsync(this, "Escolha uma pasta de destino válida na tela \"Pasta\".", "Pasta inválida", TipoDialogo.Aviso);
                MostrarPagina("pasta");
                return false;
            }

            if (!File.Exists(_ytDlpPath))
            {
                await Dialogo.MostrarAsync(this,
                    $"Não foi possível encontrar o yt-dlp.exe em:\n{_ytDlpPath}\n\n" +
                    "Coloque o arquivo yt-dlp.exe na mesma pasta deste programa.",
                    "yt-dlp.exe não encontrado", TipoDialogo.Erro);
                return false;
            }

            if (!File.Exists(_ffmpegPath))
            {
                if (exigeFfmpeg)
                {
                    await Dialogo.MostrarAsync(this,
                        "O ffmpeg.exe é obrigatório para baixar em MP3 e não foi encontrado.\n\n" +
                        "Coloque o arquivo ffmpeg.exe na mesma pasta deste programa.",
                        "ffmpeg.exe não encontrado", TipoDialogo.Erro);
                    return false;
                }

                bool continuarSemFfmpeg = await Dialogo.PerguntarAsync(this,
                    "O ffmpeg.exe não foi encontrado. Alguns vídeos podem baixar em qualidade " +
                    "reduzida sem ele. Deseja continuar mesmo assim?",
                    "ffmpeg.exe não encontrado", TipoDialogo.Aviso);

                if (!continuarSemFfmpeg)
                    return false;
            }

            if (RadCookiesArquivo.IsChecked == true &&
                (string.IsNullOrWhiteSpace(TxtCookiesArquivo.Text) || !File.Exists(TxtCookiesArquivo.Text)))
            {
                await Dialogo.MostrarAsync(this,
                    "Selecione um arquivo cookies.txt válido na tela \"Configurações\".",
                    "Arquivo de cookies inválido", TipoDialogo.Aviso);
                MostrarPagina("config");
                return false;
            }

            // Pouco espaço em disco é uma causa comum de download que falha no meio.
            long? livre = AutoReparo.EspacoLivreEmBytes(PastaDestino);
            if (livre is long bytesLivres && bytesLivres < 500L * 1024 * 1024)
            {
                bool continuar = await Dialogo.PerguntarAsync(this,
                    $"A pasta de destino tem pouco espaço livre (cerca de {bytesLivres / 1024 / 1024} MB). " +
                    "O download pode falhar no meio. Deseja continuar mesmo assim?",
                    "Pouco espaço em disco", TipoDialogo.Aviso);

                if (!continuar)
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
        /// Executa o yt-dlp.exe de forma assíncrona e oculta. Não expõe a saída bruta ao usuário —
        /// apenas um status curto + porcentagem. A saída é gravada silenciosamente em
        /// log-atividade.txt (e os erros em log-erros.txt), na pasta do programa, para suporte.
        ///
        /// AUTORREPARO: se o download falhar com um erro típico de yt-dlp desatualizado,
        /// o programa atualiza o yt-dlp sozinho e repete o mesmo download, UMA vez por sessão.
        /// </summary>
        private async Task ExecutarYtDlpAsync(List<string> argumentos, string tipoParaHistorico, string url)
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

                    var atualizacao = await RodarYtDlpAsync(new List<string> { "-U" }, reiniciarEstado: false);

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
                    !_jaOfereceuCookiesNestaSessao && RegrasDoDownload.ErroPedeCookies(_ultimoErro))
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
                            TxtAtualizarStatus.Text = "Cancelado.";
                        break;

                    case ResultadoExecucao.Sucesso:
                        if (ehAtualizacao)
                        {
                            // Ação sem histórico associado (ex.: "yt-dlp -U").
                            AtualizarStatus("Atualização concluída!");
                            TxtAtualizarStatus.Text = "Tudo em dia!";
                            Notificacoes.Mostrar("yt-dlp atualizado", "O yt-dlp foi verificado/atualizado com sucesso.");
                        }
                        else
                        {
                            AtualizarStatus("Download concluído!");
                            AtualizarProgresso(100);

                            string nomeParaNotificacao = RegistrarHistoricoDoDownload(tipoParaHistorico, url);
                            Notificacoes.Mostrar("Download concluído", nomeParaNotificacao);
                        }
                        break;

                    case ResultadoExecucao.NaoIniciou:
                        AtualizarStatus("Erro ao iniciar o yt-dlp.");
                        if (ehAtualizacao)
                            TxtAtualizarStatus.Text = "Falha ao atualizar.";
                        await OferecerBaixarYtDlpAsync(
                            "Não foi possível iniciar o yt-dlp.exe: " + (_ultimoErro ?? "motivo desconhecido") +
                            "\n\nO arquivo pode estar danificado ou ter sido bloqueado pelo antivírus.");
                        break;

                    default: // Falha
                        string mensagem = RegrasDoDownload.TraduzirErro(_ultimoErro);

                        if (!ehAtualizacao && _rastreador.Finais.Count > 0)
                        {
                            // Sucesso parcial (ex.: playlist em que alguns vídeos estavam indisponíveis).
                            AtualizarStatus("Concluído com avisos.");
                            string nomeParcial = RegistrarHistoricoDoDownload(tipoParaHistorico, url);
                            Notificacoes.Mostrar("Download concluído com avisos", nomeParcial, erro: true);
                            await Dialogo.MostrarAsync(this,
                                nomeParcial + "\n\nAlguns itens não puderam ser baixados:\n" + mensagem,
                                "Concluído com avisos", TipoDialogo.Aviso);
                        }
                        else
                        {
                            AtualizarStatus("Falha no download.");
                            if (ehAtualizacao)
                                TxtAtualizarStatus.Text = "Falha ao atualizar.";
                            Notificacoes.Mostrar("Falha no download", mensagem, erro: true);
                            await Dialogo.MostrarAsync(this,
                                mensagem + "\n\nDetalhes técnicos salvos em:\n" + GerenciadorLog.CaminhoErros,
                                "Falha no download", TipoDialogo.Erro);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Erro inesperado ao executar o yt-dlp", ex);
                AtualizarStatus("Erro inesperado.");
                await MostrarErroInesperadoAsync();
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
        private async Task<ResultadoExecucao> RodarYtDlpAsync(List<string> argumentos, bool reiniciarEstado = true)
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

                // As últimas linhas chegam pelo Dispatcher: deixa a fila esvaziar antes de decidir.
                await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

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
                string completo = Path.IsPathRooted(caminho) ? caminho : Path.Combine(PastaDestino, caminho);
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
            if (_fechada)
                return;

            bool sim = await Dialogo.PerguntarAsync(this,
                motivo + "\n\n" +
                "Posso tentar baixar agora o yt-dlp.exe oficial? O arquivo vem do GitHub do projeto yt-dlp e só é " +
                "usado depois de passar na verificação de integridade (SHA-256).\n\n" +
                "É preciso estar conectado à internet.",
                "Reparar o yt-dlp.exe", TipoDialogo.Pergunta);

            if (!sim)
            {
                GerenciadorLog.Atividade("O usuário recusou o reparo automático do yt-dlp.exe.");
                return;
            }

            DefinirEstadoDownloadEmAndamento(true);
            BtnCancelar.IsEnabled = false;
            AtualizarStatus("Baixando o yt-dlp.exe oficial... aguarde.");

            try
            {
                var (sucesso, mensagem) = await AutoReparo.BaixarYtDlpAsync(_ytDlpPath);
                if (_fechada)
                    return;

                AtualizarStatus(sucesso ? "yt-dlp.exe reparado!" : "Não foi possível reparar o yt-dlp.exe.");
                await Dialogo.MostrarAsync(this, mensagem,
                    sucesso ? "Reparo concluído" : "Reparo não concluído",
                    sucesso ? TipoDialogo.Informacao : TipoDialogo.Aviso);
            }
            finally
            {
                DefinirEstadoDownloadEmAndamento(false);
            }
        }

        /// <summary>
        /// Registra o(s) item(ns) desse download no histórico e retorna um texto curto para a
        /// notificação do Windows.
        /// - Vídeo único: um item.
        /// - Playlist + "organizar em subpastas": UM item representando a playlist inteira
        ///   (nome + quantidade de vídeos), apontando para a subpasta criada.
        /// - Playlist sem essa opção: um item para CADA vídeo baixado.
        /// Os nomes vêm do RastreadorDeArquivos, que só considera os arquivos FINAIS.
        /// </summary>
        private string RegistrarHistoricoDoDownload(string tipoParaHistorico, string url)
        {
            var agora = DateTime.Now;
            var finais = _rastreador.Finais;
            string pastaBase = PastaDestino;

            // "Playlist" marcada mas URL de vídeo único: o yt-dlp não informa nome nem total de itens.
            bool ehPlaylist = ChkPlaylist.IsChecked == true &&
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
                    Pasta = RegrasDoDownload.PastaDoArquivo(arquivo, pastaBase),
                    Url = url
                });

                return nomeArquivo;
            }

            if (ChkOrganizarPlaylist.IsChecked == true)
            {
                string nomePlaylist = string.IsNullOrWhiteSpace(_nomePlaylistAtual) ? "Playlist" : _nomePlaylistAtual!;
                string pastaPlaylistPrevista = Path.Combine(pastaBase, nomePlaylist);
                string titulo = $"{nomePlaylist} ({finais.Count} vídeos)";

                GerenciadorHistorico.Adicionar(new HistoricoItem
                {
                    DataHora = agora,
                    Tipo = tipoParaHistorico,
                    Titulo = titulo,
                    Pasta = finais.Count > 0 ? RegrasDoDownload.PastaDoArquivo(finais[0], pastaPlaylistPrevista) : pastaPlaylistPrevista,
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
            var itens = new List<HistoricoItem>();
            foreach (var caminho in finais)
            {
                itens.Add(new HistoricoItem
                {
                    DataHora = agora,
                    Tipo = tipoParaHistorico,
                    Titulo = Path.GetFileName(caminho),
                    Pasta = RegrasDoDownload.PastaDoArquivo(caminho, pastaBase),
                    Url = url
                });
            }
            GerenciadorHistorico.AdicionarVarios(itens);

            return $"Playlist: {finais.Count} vídeos baixados";
        }

        /// <summary>
        /// As linhas do yt-dlp chegam numa thread do .NET (eventos do Process): passam para a
        /// thread da tela antes de mexer em qualquer coisa. Com a janela fechada, são ignoradas.
        /// </summary>
        private void ProcessarLinhaDeSaida(string? linha)
        {
            if (string.IsNullOrWhiteSpace(linha) || _fechada)
                return;

            string texto = linha;
            Dispatcher.UIThread.Post(() =>
            {
                if (!_fechada) TratarLinha(texto);
            });
        }

        private void TratarLinha(string linha)
        {
            double percentual = 0;
            var matchProgresso = RegrasDoDownload.RegexProgresso.Match(linha);
            bool ehProgresso = matchProgresso.Success && double.TryParse(
                matchProgresso.Groups[1].Value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out percentual);

            bool playlistMarcada = ChkPlaylist.IsChecked == true;

            if (ehProgresso)
            {
                // No log, registra o progresso só de 25 em 25%.
                if (percentual >= 100 || percentual < _ultimoPercentualLogado || percentual - _ultimoPercentualLogado >= 25)
                {
                    RegistrarLog(linha);
                    _ultimoPercentualLogado = percentual;
                }

                AtualizarProgresso(percentual);

                if (playlistMarcada && _totalItensPlaylist > 0)
                    AtualizarStatus($"Baixando vídeo {_itemAtualPlaylist} de {_totalItensPlaylist}... {percentual:0}%");
                else
                    AtualizarStatus($"Baixando... {percentual:0}%");
            }
            else
            {
                RegistrarLog(linha);
            }

            // Descobre os arquivos finais (ignora os temporários que o yt-dlp apaga depois).
            _rastreador.ProcessarLinha(linha);

            var matchPlaylistNome = RegrasDoDownload.RegexPlaylistNome.Match(linha);
            if (matchPlaylistNome.Success)
                _nomePlaylistAtual = matchPlaylistNome.Groups[1].Value.Trim();

            var matchPlaylistItem = RegrasDoDownload.RegexPlaylistItem.Match(linha);
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
            else if (linha.Contains("Extracting URL") && !playlistMarcada)
                AtualizarStatus("Processando URL...");
            else if (linha.Contains("has already been downloaded"))
                AtualizarStatus("Arquivo já existe, pulando...");

            var matchErro = RegrasDoDownload.RegexErro.Match(linha);
            if (matchErro.Success)
            {
                _ultimoErro = matchErro.Groups[1].Value.Trim();
                GerenciadorLog.Erro("O yt-dlp informou um erro (URL: " + _urlAtual + "): " + _ultimoErro);
            }

            if (!_avisoBotMostrado && linha.Contains("Sign in to confirm") && linha.Contains("not a bot"))
                _avisoBotMostrado = true;
        }

        private void AtualizarStatus(string texto)
        {
            if (_fechada)
                return;

            TxtStatus.Text = texto;
        }

        private void AtualizarProgresso(double percentual)
        {
            if (_fechada)
                return;

            int valor = Math.Clamp((int)Math.Round(percentual), 0, 100);
            BarraProgresso.Value = valor;
            TxtPercentual.Text = $"{valor}%";
        }

        /// <summary>
        /// Grava uma linha no log de atividade (log-atividade.txt, na pasta do programa),
        /// sem nunca exibir isso na interface.
        /// </summary>
        private static void RegistrarLog(string linha) => GerenciadorLog.Atividade(linha);

        private void DefinirEstadoDownloadEmAndamento(bool emAndamento)
        {
            if (_fechada)
                return;

            BtnMp4.IsEnabled = !emAndamento;
            BtnMp3.IsEnabled = !emAndamento;
            TxtUrl.IsEnabled = !emAndamento;
            ChkPlaylist.IsEnabled = !emAndamento;
            ChkOrganizarPlaylist.IsEnabled = !emAndamento;
            BtnAlterarLocal.IsEnabled = !emAndamento;
            BtnAtualizarYtDlp.IsEnabled = !emAndamento;
            BtnExtrairCookies.IsEnabled = !emAndamento;
            RadSemCookies.IsEnabled = !emAndamento;
            RadCookiesNavegador.IsEnabled = !emAndamento;
            RadCookiesArquivo.IsEnabled = !emAndamento;
            CmbNavegador.IsEnabled = !emAndamento && RadCookiesNavegador.IsChecked == true;
            TxtCookiesArquivo.IsEnabled = !emAndamento && RadCookiesArquivo.IsChecked == true;

            BtnCancelar.IsEnabled = emAndamento;

            Cursor = emAndamento ? new Cursor(StandardCursorType.AppStarting) : Cursor.Default;
        }

        // ---------------------------------------------------------------
        // Página Configurações (autenticação + atualizar yt-dlp)
        // ---------------------------------------------------------------

        /// <summary>
        /// Liga só o campo da opção escolhida. (No Avalonia os três botões de opção já são
        /// exclusivos pelo GroupName: o antigo bug do Guna, com dois marcados ao mesmo tempo,
        /// não tem como voltar.)
        /// </summary>
        private void AtualizarCamposDeCookies()
        {
            CmbNavegador.IsEnabled = RadCookiesNavegador.IsChecked == true && !_ocupado;
            TxtCookiesArquivo.IsEnabled = RadCookiesArquivo.IsChecked == true && !_ocupado;
        }

        /// <summary>
        /// Abre a página oficial do yt-dlp com instruções de como exportar o arquivo cookies.txt
        /// do navegador.
        /// </summary>
        private async Task AbrirAjudaCookiesAsync()
        {
            bool abriu = false;
            try
            {
                abriu = await Launcher.LaunchUriAsync(new Uri(RegrasDoDownload.UrlAjudaCookies));
            }
            catch (Exception ex)
            {
                GerenciadorLog.Aviso("Não foi possível abrir a ajuda dos cookies: " + ex.Message);
            }

            if (!abriu)
            {
                await Dialogo.MostrarAsync(this,
                    "Não foi possível abrir o navegador. Acesse manualmente:\n" + RegrasDoDownload.UrlAjudaCookies,
                    "Ajuda", TipoDialogo.Informacao);
            }
        }

        /// <summary>
        /// O campo de cookies.txt é clicável (só quando a opção correspondente está marcada).
        /// </summary>
        private async Task EscolherArquivoCookiesAsync()
        {
            if (!TxtCookiesArquivo.IsEnabled)
                return;

            var arquivos = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Selecione o arquivo de cookies exportado do navegador",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Arquivos de cookies (*.txt)") { Patterns = new[] { "*.txt" } },
                    new FilePickerFileType("Todos os arquivos (*.*)") { Patterns = new[] { "*" } },
                },
            });

            var caminho = arquivos.FirstOrDefault()?.TryGetLocalPath();
            if (!string.IsNullOrEmpty(caminho))
                TxtCookiesArquivo.Text = caminho;
        }

        /// <summary>
        /// Extrai os cookies do navegador escolhido e grava num cookies.txt fixo na pasta do
        /// programa (resolve o "403 Forbidden" de ler os cookies ao vivo). Tem tempo limite de
        /// 2 minutos, o processo é registrado para ser encerrado junto com o programa, e a trava
        /// _ocupado impede iniciar outra operação junto.
        /// </summary>
        private async Task ExtrairCookiesPeloBotaoAsync()
        {
            if (_ocupado)
                return;

            if (!File.Exists(_ytDlpPath))
            {
                await Dialogo.MostrarAsync(this, $"Não foi possível encontrar o yt-dlp.exe em:\n{_ytDlpPath}",
                    "yt-dlp.exe não encontrado", TipoDialogo.Erro);
                return;
            }

            if (CmbNavegador.SelectedItem is not string navegador || string.IsNullOrWhiteSpace(navegador))
            {
                await Dialogo.MostrarAsync(this, "Selecione um navegador na lista antes de extrair os cookies.",
                    "Nenhum navegador selecionado", TipoDialogo.Aviso);
                return;
            }

            _ocupado = true;
            DefinirEstadoDownloadEmAndamento(true);

            try
            {
                var (sucesso, mensagem) = await ExtrairCookiesAsync(navegador);
                if (!sucesso && !_fechada)
                    await Dialogo.MostrarAsync(this, mensagem, "Falha ao extrair cookies", TipoDialogo.Erro);
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Erro inesperado na extração de cookies", ex);
                await MostrarErroInesperadoAsync();
            }
            finally
            {
                _ocupado = false;
                DefinirEstadoDownloadEmAndamento(false);
            }
        }

        /// <summary>
        /// Núcleo da extração de cookies (usado pelo botão e pelo fluxo automático). Não mexe na
        /// trava _ocupado: quem chama é responsável por isso. Em caso de sucesso, já deixa a tela
        /// apontando para o cookies.txt gerado; em caso de falha, devolve a mensagem amigável.
        /// </summary>
        private async Task<(bool Sucesso, string Mensagem)> ExtrairCookiesAsync(string navegador)
        {
            // Normalmente é a pasta do programa; se ela não permitir gravação, usa a pasta alternativa.
            string caminhoCookies = Path.Combine(AutoReparo.PastaGravavel(), "cookies.txt");

            // Apaga a versão anterior: assim um cookies.txt velho nunca é confundido com um recém-gerado.
            try { if (File.Exists(caminhoCookies)) File.Delete(caminhoCookies); } catch { }

            TxtExtrairCookiesStatus.Text = "Extraindo...";
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
            foreach (var arg in RegrasDoDownload.ArgumentosDaExtracaoDeCookies(navegador, caminhoCookies))
                psi.ArgumentList.Add(arg);

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
                    var m = RegrasDoDownload.RegexErro.Match(e2.Data);
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

                // O que vale é o ARQUIVO: ele precisa existir e conter cookies do YouTube/Google,
                // independentemente do código de saída (o vídeo de teste pode falhar à toa).
                bool sucesso = RegrasDoDownload.CookiesTxtTemCookiesDoYoutube(caminhoCookies);
                if (sucesso && processo.ExitCode != 0)
                    RegistrarLog("O yt-dlp terminou com erro no vídeo de teste, mas o cookies.txt foi gerado e será usado.");

                if (sucesso)
                {
                    // Muda automaticamente para o modo "arquivo", já apontando pro cookies.txt recém-gerado.
                    RadCookiesArquivo.IsChecked = true;
                    TxtCookiesArquivo.Text = caminhoCookies;
                    TxtExtrairCookiesStatus.Text = $"Extraído às {DateTime.Now:HH:mm} — pronto para uso.";
                    AtualizarStatus("Cookies extraídos com sucesso!");
                    RegistrarLog("Extração de cookies concluída com sucesso.");
                    Notificacoes.Mostrar("Cookies extraídos", "O arquivo cookies.txt foi gerado com sucesso.");
                    return (true, "");
                }

                TxtExtrairCookiesStatus.Text = "Falha na extração.";
                AtualizarStatus("Falha ao extrair cookies.");
                GerenciadorLog.Erro($"Falha na extração de cookies do {navegador}: {erroCapturado}");
                return (false, RegrasDoDownload.TraduzirErroExtracaoCookies(erroCapturado, navegador));
            }
            catch (OperationCanceledException)
            {
                ProcessosFilhos.Encerrar(_processoAtual);
                TxtExtrairCookiesStatus.Text = "Falha na extração.";
                AtualizarStatus("A extração de cookies demorou demais.");
                GerenciadorLog.Erro($"A extração de cookies do {navegador} passou de 2 minutos e foi interrompida.");
                return (false,
                    $"A extração de cookies do {navegador} demorou demais e foi interrompida.\n\n" +
                    "Feche completamente o navegador (inclusive na bandeja do sistema, perto do relógio) e tente novamente.");
            }
            catch (Exception ex)
            {
                TxtExtrairCookiesStatus.Text = "Falha na extração.";
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

        /// <summary>
        /// Chamado quando o download falha por falta de login. Sugere copiar os cookies do navegador
        /// padrão do computador e, se isso também falhar, indica uma ferramenta que exporta os cookies
        /// para .txt. Devolve true se os cookies foram extraídos (vale tentar o download de novo).
        /// </summary>
        private async Task<bool> TentarCookiesAutomaticosAsync()
        {
            if (_fechada)
                return false;

            string navegador = RegrasDoDownload.DetectarNavegadorPadrao() ?? (CmbNavegador.SelectedItem as string) ?? "chrome";

            bool sim = await Dialogo.PerguntarAsync(this,
                "O YouTube pediu login para baixar este vídeo.\n\n" +
                $"Posso tentar copiar os cookies do {navegador} (navegador padrão deste computador) e repetir o download?\n\n" +
                "Os cookies são como uma \"chave de login\": o arquivo cookies.txt fica na pasta do programa. " +
                "Use isso apenas em um computador seu e, de preferência, com uma conta que você não se importe de expor.",
                "Usar os cookies do navegador?", TipoDialogo.Pergunta);

            if (!sim)
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

            if (!_fechada)
            {
                bool abrir = await Dialogo.PerguntarAsync(this,
                    mensagem + "\n\n" +
                    "Alguns navegadores recentes (Chrome e Edge, principalmente) protegem os cookies e impedem essa cópia.\n\n" +
                    "Quer abrir a página com instruções de uma ferramenta que exporta os cookies para um arquivo .txt? " +
                    "Depois de gerar o arquivo, escolha-o em Configurações > cookies.txt.",
                    "Não foi possível copiar os cookies", TipoDialogo.Aviso);

                if (abrir)
                    await AbrirAjudaCookiesAsync();
            }

            return false;
        }

        private async Task AtualizarYtDlpPeloBotaoAsync()
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

                TxtAtualizarStatus.Text = "Verificando...";
                await ExecutarYtDlpAsync(new List<string> { "-U" }, tipoParaHistorico: "", url: "");
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Falha ao atualizar o yt-dlp", ex);
                await MostrarErroInesperadoAsync();
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
            var linhas = GerenciadorHistorico.Carregar()
                .Where(item => !string.IsNullOrEmpty(item.Tipo)) // ignora entradas de "atualização do yt-dlp" (sem tipo)
                .Select(item => new LinhaHistorico(item))
                .ToList();

            ListaHistorico.ItemsSource = linhas;
            PainelHistoricoVazio.IsVisible = linhas.Count == 0;
            ListaHistorico.IsVisible = linhas.Count > 0;
        }

        // Item do menu do botão direito (o menu só existe quando abre, por isso o evento vem do XAML).
        private async void OnMenuAbrirPastaClick(object? sender, RoutedEventArgs e) => await AbrirPastaDoItemAsync();

        private async Task AbrirPastaDoItemAsync()
        {
            if (ListaHistorico.SelectedItem is not LinhaHistorico { Item: var item })
                return;

            if (!Directory.Exists(item.Pasta))
            {
                await Dialogo.MostrarAsync(this, "A pasta deste item não existe mais.", "Pasta não encontrada", TipoDialogo.Aviso);
                return;
            }

            await AbrirNoExploradorAsync(item.Pasta);
        }

        /// <summary>Abre uma pasta no Explorador de Arquivos, sem deixar um erro derrubar o programa.</summary>
        private async Task AbrirNoExploradorAsync(string pasta)
        {
            bool abriu = false;
            try
            {
                abriu = await Launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(pasta));
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Não foi possível abrir a pasta '" + pasta + "'", ex);
            }

            if (!abriu && !_fechada)
                await Dialogo.MostrarAsync(this, "Não foi possível abrir a pasta:\n" + pasta, "Erro ao abrir a pasta", TipoDialogo.Aviso);
        }

        private async Task LimparHistoricoAsync()
        {
            if (ListaHistorico.ItemCount == 0)
                return;

            bool sim = await Dialogo.PerguntarAsync(this,
                "Apagar todo o histórico de downloads? Esta ação não pode ser desfeita.",
                "Confirmar", TipoDialogo.Pergunta);

            if (sim)
            {
                GerenciadorHistorico.Limpar();
                CarregarHistorico();
            }
        }

        // ---------------------------------------------------------------
        // Autodiagnóstico (ver AutoReparo.cs)
        // ---------------------------------------------------------------

        /// <summary>
        /// Roda as verificações de integridade (yt-dlp, ffmpeg, histórico, filtro, logs). Corrige
        /// sozinho o que é seguro corrigir; para o resto, avisa e (no caso do yt-dlp.exe) oferece
        /// o reparo. Atalhos: Ctrl+Shift+D (roda e mostra o resumo) e Ctrl+Shift+L (abre os logs).
        /// </summary>
        private async Task ExecutarAutoDiagnosticoAsync(bool mostrarResumo)
        {
            try
            {
                var resultados = await Task.Run(() => AutoReparo.VerificarInicio(_ytDlpPath, _ffmpegPath));
                if (_fechada)
                    return;

                int corrigidos = resultados.Count(r => r.Situacao == SituacaoVerificacao.Corrigido);

                if (!File.Exists(_ytDlpPath))
                    AtualizarStatus("yt-dlp.exe não encontrado.");
                else if (corrigidos > 0)
                    AtualizarStatus("Autodiagnóstico: " + corrigidos + " problema(s) corrigido(s) automaticamente.");

                if (mostrarResumo)
                {
                    await Dialogo.MostrarAsync(this,
                        AutoReparo.GerarResumo(resultados) + "\nLogs gravados em:\n" + GerenciadorLog.PastaAtual,
                        "Autodiagnóstico", TipoDialogo.Informacao);
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

        protected override async void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            var ctrlShift = KeyModifiers.Control | KeyModifiers.Shift;
            if (e.KeyModifiers != ctrlShift)
                return;

            if (e.Key == Key.L)
            {
                e.Handled = true;
                await AbrirNoExploradorAsync(GerenciadorLog.PastaAtual);
            }
            else if (e.Key == Key.D)
            {
                e.Handled = true;
                if (!_ocupado)
                    await ExecutarAutoDiagnosticoAsync(mostrarResumo: true);
            }
        }

        // ---------------------------------------------------------------
        // Fechamento
        // ---------------------------------------------------------------

        protected override async void OnClosing(WindowClosingEventArgs e)
        {
            bool baixando = _processoAtual is { HasExited: false };

            // Fechar no meio de um download cancela o download e deixa arquivo incompleto: pergunta antes.
            if (baixando && !_fecharConfirmado && !e.IsProgrammatic)
            {
                e.Cancel = true;
                base.OnClosing(e);

                bool fecharMesmoAssim = await Dialogo.PerguntarAsync(this,
                    "Há um download em andamento. Se você fechar agora, ele será cancelado e o arquivo ficará incompleto.\n\n" +
                    "Deseja fechar mesmo assim?",
                    "Download em andamento", TipoDialogo.Aviso);

                if (fecharMesmoAssim)
                {
                    _fecharConfirmado = true;
                    Close();
                }
                return;
            }

            try
            {
                _fechada = true;
                _ctsPreviaCts?.Cancel(); // encerra a busca de prévia em andamento (e o yt-dlp dela)
                _timerSpinner.Stop();

                if (_processoAtual is { HasExited: false })
                {
                    _canceladoPeloUsuario = true;
                    GerenciadorLog.Aviso("O programa foi fechado com um download em andamento; o download foi cancelado.");
                    _processoAtual.Kill(entireProcessTree: true);
                }

                ProcessosFilhos.EncerrarTodos();
            }
            catch (Exception ex)
            {
                // Falhas ao encerrar durante o fechamento não devem impedir o app de fechar, mas ficam registradas.
                GerenciadorLog.Aviso("Problema ao encerrar processos no fechamento do programa: " + ex.Message);
            }

            base.OnClosing(e);
        }
    }
}

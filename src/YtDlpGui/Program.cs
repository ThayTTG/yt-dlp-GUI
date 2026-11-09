using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using NokyLabs.UI;

namespace YtDlpGui
{
    /// <summary>
    /// Classe de entrada da aplicação (ponto de partida do programa).
    /// </summary>
    internal static class Program
    {
        // Evita uma "chuva" de janelas de aviso se o mesmo problema se repetir várias vezes seguidas.
        private static DateTime _ultimoAvisoNaTela = DateTime.MinValue;

        [STAThread]
        public static void Main(string[] args)
        {
            // 1) Registro de sessão + log: tem que ser a PRIMEIRA coisa, para até um erro
            //    durante a abertura do programa ficar registrado.
            AutoReparo.IniciarSessao();

            // 2) Rede de proteção contra erros não tratados (os da interface são tratados em
            //    Dispatcher.UIThread.UnhandledException, ligado em ConfigurarProtecaoDaInterface).
            AppDomain.CurrentDomain.UnhandledException += (s, e) => TratarErroFatal(e.ExceptionObject as Exception, e.IsTerminating);
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                GerenciadorLog.Erro("Uma tarefa em segundo plano falhou sem ninguém tratar", e.Exception);
                e.SetObserved();
            };

            // 3) Garante que nenhum yt-dlp.exe fique rodando escondido depois que o programa fecha.
            AppDomain.CurrentDomain.ProcessExit += (s, e) => ProcessosFilhos.EncerrarTodos();

            try
            {
                BuildAvaloniaApp()
                    .AfterSetup(_ => ConfigurarProtecaoDaInterface())
                    .StartWithClassicDesktopLifetime(args);

                AutoReparo.MarcarEncerramentoLimpo();
            }
            catch (Exception ex)
            {
                // Erro durante a abertura ou fatal: registra e avisa em vez de sumir sem explicação.
                GerenciadorLog.Erro("Erro fatal no programa", ex);
                AvisoDoSistema(
                    "O programa precisou ser encerrado por causa de um erro inesperado.\n\n" +
                    "Os detalhes foram salvos em:\n" + GerenciadorLog.CaminhoErros);
            }
            finally
            {
                ProcessosFilhos.EncerrarTodos();
            }
        }

        // Usado também pelo previewer do Avalonia.
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseNokyLabs()
                .LogToTrace();

        /// <summary>
        /// Erros dentro da interface (botões, eventos): ficam no log e o programa CONTINUA aberto,
        /// com um aviso no máximo a cada 10 segundos. Sem isto, um erro em qualquer botão
        /// (principalmente os "async void") derrubava o programa sem deixar rastro.
        /// </summary>
        private static void ConfigurarProtecaoDaInterface()
        {
            Dispatcher.UIThread.UnhandledException += (s, e) =>
            {
                e.Handled = true;
                GerenciadorLog.Erro("Erro inesperado na interface (o programa continua aberto)", e.Exception);

                if ((DateTime.Now - _ultimoAvisoNaTela).TotalSeconds < 10)
                    return;

                _ultimoAvisoNaTela = DateTime.Now;
                var janela = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                _ = Dialogo.MostrarAsync(janela,
                    "Algo inesperado aconteceu, mas o programa continua funcionando.\n\n" +
                    "Se o problema se repetir, envie o arquivo abaixo para quem cuida do programa:\n" +
                    GerenciadorLog.CaminhoErros,
                    "YT-DLP GUI", TipoDialogo.Aviso);
            };
        }

        private static void TratarErroFatal(Exception? ex, bool encerrando)
        {
            GerenciadorLog.Erro(
                encerrando ? "Erro fatal fora da interface (o programa será encerrado)" : "Erro fora da interface",
                ex ?? new Exception("Erro desconhecido (objeto de exceção ausente)"));

            if (encerrando)
                ProcessosFilhos.EncerrarTodos();
        }

        /// <summary>
        /// Aviso de último caso, quando a interface nem chegou a abrir (ou já caiu):
        /// usa a caixa de mensagem do próprio Windows.
        /// </summary>
        private static void AvisoDoSistema(string mensagem)
        {
            try
            {
                if (OperatingSystem.IsWindows())
                    MessageBoxW(IntPtr.Zero, mensagem, "YT-DLP GUI", 0x10 /* MB_ICONERROR */);
            }
            catch
            {
                // Se nem a janela de aviso puder ser mostrada, o log já tem o registro.
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
    }
}

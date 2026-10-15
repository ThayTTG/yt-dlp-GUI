using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YtDlpGui
{
    /// <summary>
    /// Classe de entrada da aplicação (ponto de partida do programa).
    /// </summary>
    internal static class Program
    {
        // Evita uma "chuva" de janelas de aviso se o mesmo problema se repetir várias vezes seguidas.
        private static DateTime _ultimoAvisoNaTela = DateTime.MinValue;

        /// <summary>
        /// Ponto de entrada principal da aplicação.
        /// [STAThread] é obrigatório para aplicações WinForms.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // 1) Registro de sessão + log: tem que ser a PRIMEIRA coisa, para até um erro
            //    durante a abertura do programa ficar registrado.
            AutoReparo.IniciarSessao();

            // 2) Rede de proteção contra erros não tratados. Sem isto, um erro em qualquer
            //    botão (principalmente os "async void") derrubava o programa sem deixar rastro.
            //    Precisa ser configurado ANTES de criar qualquer janela.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => TratarErroDeInterface(e.Exception);
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
                // Habilita os estilos visuais modernos do Windows (bordas, botões, etc.)
                Application.EnableVisualStyles();

                // Usa o renderizador de texto do GDI+ para melhor compatibilidade
                Application.SetCompatibleTextRenderingDefault(false);

                // Inicia a aplicação exibindo o formulário principal
                Application.Run(new MainForm());

                AutoReparo.MarcarEncerramentoLimpo();
            }
            catch (Exception ex)
            {
                // Erro durante a abertura ou fatal: registra e avisa em vez de sumir sem explicação.
                GerenciadorLog.Erro("Erro fatal no programa", ex);
                MostrarAviso(
                    "O programa precisou ser encerrado por causa de um erro inesperado.\n\n" +
                    "Os detalhes foram salvos em:\n" + GerenciadorLog.CaminhoErros,
                    MessageBoxIcon.Error);
            }
            finally
            {
                ProcessosFilhos.EncerrarTodos();
            }
        }

        private static void TratarErroDeInterface(Exception ex)
        {
            GerenciadorLog.Erro("Erro inesperado na interface (o programa continua aberto)", ex);

            // Mostra a janela de aviso no máximo uma vez a cada 10 segundos.
            if ((DateTime.Now - _ultimoAvisoNaTela).TotalSeconds < 10)
                return;

            _ultimoAvisoNaTela = DateTime.Now;
            MostrarAviso(
                "Algo inesperado aconteceu, mas o programa continua funcionando.\n\n" +
                "Se o problema se repetir, envie o arquivo abaixo para quem cuida do programa:\n" +
                GerenciadorLog.CaminhoErros,
                MessageBoxIcon.Warning);
        }

        private static void TratarErroFatal(Exception? ex, bool encerrando)
        {
            GerenciadorLog.Erro(
                encerrando ? "Erro fatal fora da interface (o programa será encerrado)" : "Erro fora da interface",
                ex ?? new Exception("Erro desconhecido (objeto de exceção ausente)"));

            if (encerrando)
                ProcessosFilhos.EncerrarTodos();
        }

        private static void MostrarAviso(string mensagem, MessageBoxIcon icone)
        {
            try
            {
                MessageBox.Show(mensagem, "YtDlp GUI", MessageBoxButtons.OK, icone);
            }
            catch
            {
                // Se nem a janela de aviso puder ser mostrada, o log já tem o registro.
            }
        }
    }
}

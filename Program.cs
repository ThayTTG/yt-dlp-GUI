using System;
using System.Windows.Forms;

namespace YtDlpGui
{
    /// <summary>
    /// Classe de entrada da aplicação (ponto de partida do programa).
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Ponto de entrada principal da aplicação.
        /// [STAThread] é obrigatório para aplicações WinForms.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Habilita os estilos visuais modernos do Windows (bordas, botões, etc.)
            Application.EnableVisualStyles();

            // Usa o renderizador de texto do GDI+ para melhor compatibilidade
            Application.SetCompatibleTextRenderingDefault(false);

            // Inicia a aplicação exibindo o formulário principal
            Application.Run(new MainForm());
        }
    }
}

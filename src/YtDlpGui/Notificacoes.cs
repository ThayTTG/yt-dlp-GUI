using System;

namespace YtDlpGui
{
    /// <summary>
    /// Notificações do Windows ao terminar (ou falhar) um download, para o aluno poder minimizar
    /// a janela e ainda saber quando acabou. No Windows 10/11 aparecem como "toast" (o mesmo
    /// lugar onde os balões antigos já apareciam). Fora do Windows, não fazem nada.
    /// Uma notificação nunca derruba o programa: se falhar, só fica no log.
    /// </summary>
    public static class Notificacoes
    {
        public static void Mostrar(string titulo, string mensagem, bool erro = false)
        {
            try
            {
#if WINDOWS
                new Microsoft.Toolkit.Uwp.Notifications.ToastContentBuilder()
                    .AddText(titulo)
                    .AddText(mensagem)
                    .Show();
#endif
            }
            catch (Exception ex)
            {
                GerenciadorLog.Atividade("Notificação não exibida: " + ex.Message);
            }
        }
    }
}

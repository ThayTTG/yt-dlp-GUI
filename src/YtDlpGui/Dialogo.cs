using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace YtDlpGui
{
    public enum TipoDialogo { Informacao, Aviso, Erro, Pergunta }

    /// <summary>
    /// As janelinhas de aviso e de pergunta (substituem o MessageBox do WinForms), no visual
    /// NokyLabs. Todas são modais: a janela principal espera a resposta.
    /// </summary>
    public static class Dialogo
    {
        /// <summary>Mostra um aviso com o botão OK.</summary>
        public static Task MostrarAsync(Window? dono, string mensagem, string titulo, TipoDialogo tipo = TipoDialogo.Informacao) =>
            AbrirAsync(dono, mensagem, titulo, tipo, simNao: false);

        /// <summary>Pergunta com Sim/Não. Devolve true só para "Sim" (fechar a janela conta como "Não").</summary>
        public static Task<bool> PerguntarAsync(Window? dono, string mensagem, string titulo, TipoDialogo tipo = TipoDialogo.Pergunta) =>
            AbrirAsync(dono, mensagem, titulo, tipo, simNao: true);

        /// <summary>
        /// Pede o código mestre do professor (filtro de conteúdo). Devolve true só se o código
        /// digitado for válido; se estiver errado, avisa "Código mestre incorreto.".
        /// </summary>
        public static async Task<bool> PedirCodigoMestreAsync(Window? dono, ConfiguracaoFiltroConteudo config, string motivoBloqueio)
        {
            var campo = new TextBox { Classes = { "field" }, PasswordChar = '•', Watermark = "Código mestre" };
            var corpo = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    Texto(motivoBloqueio, secundario: true),
                    Texto("Digite o código mestre do professor para continuar:", secundario: false),
                    campo
                }
            };

            bool liberar = await AbrirAsync(dono, corpo, "Conteúdo bloqueado", TipoDialogo.Aviso,
                textoSim: "Liberar", textoNao: "Cancelar", foco: campo);
            if (!liberar)
                return false;

            bool valido = FiltroConteudo.CodigoMestreValido(campo.Text ?? "", config);
            if (!valido)
                await MostrarAsync(dono, "Código mestre incorreto.", "Acesso negado", TipoDialogo.Erro);

            return valido;
        }

        private static Task<bool> AbrirAsync(Window? dono, string mensagem, string titulo, TipoDialogo tipo, bool simNao) =>
            AbrirAsync(dono, Texto(mensagem, secundario: true), titulo, tipo,
                textoSim: simNao ? "Sim" : "OK", textoNao: simNao ? "Não" : null, foco: null);

        private static async Task<bool> AbrirAsync(Window? dono, Control corpo, string titulo, TipoDialogo tipo,
                                                   string textoSim, string? textoNao, Control? foco)
        {
            bool resposta = false;

            var (icone, cor) = tipo switch
            {
                TipoDialogo.Erro => ("IconClose", "DangerBrush"),
                TipoDialogo.Aviso => ("IconAlert", "WarningBrush"),
                TipoDialogo.Pergunta => ("IconInfo", "AccentTextBrush"),
                _ => ("IconInfo", "AccentTextBrush"),
            };
            var fundoIcone = tipo switch
            {
                TipoDialogo.Erro => "DangerSoftBrush",
                TipoDialogo.Aviso => "WarningSoftBrush",
                _ => "AccentSoftBrush",
            };

            var janela = new Window
            {
                Title = titulo,
                Width = 460,
                SizeToContent = SizeToContent.Height,
                CanResize = false,
                ShowInTaskbar = dono is null,
                WindowStartupLocation = dono is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
                Icon = dono?.Icon,
            };

            var simBotao = new Button { Classes = { "primary" }, Content = textoSim, MinWidth = 96, IsDefault = true };
            simBotao.Click += (_, _) => { resposta = true; janela.Close(); };

            var botoes = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Right };
            if (textoNao is not null)
            {
                var naoBotao = new Button { Classes = { "secondary" }, Content = textoNao, MinWidth = 96, IsCancel = true };
                naoBotao.Click += (_, _) => { resposta = false; janela.Close(); };
                botoes.Children.Add(naoBotao);
            }
            botoes.Children.Add(simBotao);

            var circulo = new Border
            {
                Width = 40, Height = 40, CornerRadius = new CornerRadius(20), VerticalAlignment = VerticalAlignment.Top,
                [!Border.BackgroundProperty] = new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension(fundoIcone),
                Child = new Viewbox
                {
                    Width = 20, Height = 20,
                    Child = new Avalonia.Controls.Shapes.Path
                    {
                        Classes = { "icon" },
                        [!Avalonia.Controls.Shapes.Path.DataProperty] = new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension(icone),
                        [!Avalonia.Controls.Shapes.Shape.StrokeProperty] = new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension(cor),
                    }
                }
            };

            var conteudo = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                Margin = new Thickness(24, 22, 24, 20),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
            };
            conteudo.Children.Add(circulo);
            Grid.SetRowSpan(circulo, 2);

            var tituloTexto = new TextBlock { Text = titulo, Classes = { "h3" }, Margin = new Thickness(16, 2, 0, 8), TextWrapping = TextWrapping.Wrap };
            Grid.SetColumn(tituloTexto, 1);
            conteudo.Children.Add(tituloTexto);

            corpo.Margin = new Thickness(16, 0, 0, 0);
            Grid.SetColumn(corpo, 1);
            Grid.SetRow(corpo, 1);
            conteudo.Children.Add(corpo);

            botoes.Margin = new Thickness(0, 20, 0, 0);
            Grid.SetRow(botoes, 2);
            Grid.SetColumnSpan(botoes, 2);
            conteudo.Children.Add(botoes);

            janela.Content = conteudo;
            janela.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Escape) { resposta = false; janela.Close(); }
            };
            janela.Opened += (_, _) => (foco ?? simBotao).Focus();

            if (dono is not null && dono.IsVisible)
                await janela.ShowDialog(dono);
            else
            {
                var fechou = new TaskCompletionSource();
                janela.Closed += (_, _) => fechou.TrySetResult();
                janela.Show();
                await fechou.Task;
            }

            return resposta;
        }

        private static TextBlock Texto(string texto, bool secundario) => new()
        {
            Text = texto,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 13.5,
            LineHeight = 20,
            [!TextBlock.ForegroundProperty] = new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension(
                secundario ? "TextSecondaryBrush" : "TextBrush"),
        };
    }
}

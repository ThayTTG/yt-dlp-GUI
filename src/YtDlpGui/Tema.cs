using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace YtDlpGui
{
    /// <summary>
    /// Parte visual da janela principal que não cabe no Designer:
    /// ícones, barra de título escura, efeitos de hover na barra lateral,
    /// menu de contexto escuro e pequenos detalhes de acabamento.
    ///
    /// Nada aqui muda o funcionamento do programa: se qualquer um destes
    /// enfeites falhar (Windows antigo, imagem faltando...), a janela continua
    /// funcionando normalmente, só com uma aparência mais simples.
    /// </summary>
    public partial class MainForm
    {
        // ---------------------------------------------------------------
        // Barra de título escura (Windows 10 1809+ / Windows 11)
        // ---------------------------------------------------------------

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int atributo, ref int valor, int tamanho);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_ANTIGO = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;

        private static int ParaCorDoWindows(Color c) => c.R | (c.G << 8) | (c.B << 16);

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            try
            {
                int ligado = 1;
                if (DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref ligado, sizeof(int)) != 0)
                    DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_ANTIGO, ref ligado, sizeof(int));

                // Só o Windows 11 entende as três abaixo; no 10 elas são ignoradas sem erro.
                int corTitulo = ParaCorDoWindows(CorSidebar);
                int corTexto = ParaCorDoWindows(CorTexto);
                int corBorda = ParaCorDoWindows(CorBorda);
                DwmSetWindowAttribute(Handle, DWMWA_CAPTION_COLOR, ref corTitulo, sizeof(int));
                DwmSetWindowAttribute(Handle, DWMWA_TEXT_COLOR, ref corTexto, sizeof(int));
                DwmSetWindowAttribute(Handle, DWMWA_BORDER_COLOR, ref corBorda, sizeof(int));
            }
            catch
            {
                // Windows sem suporte: fica a barra de título padrão.
            }
        }

        // ---------------------------------------------------------------
        // Acabamentos aplicados logo depois do InitializeComponent
        // ---------------------------------------------------------------

        private void AplicarTema()
        {
            // Linhas finas separando a barra lateral e o rodapé do conteúdo.
            pnlSidebar.Paint += (s, e) =>
            {
                using var caneta = new Pen(CorBorda);
                e.Graphics.DrawLine(caneta, pnlSidebar.Width - 1, 0, pnlSidebar.Width - 1, pnlSidebar.Height);
            };
            pnlSidebar.Resize += (s, e) => pnlSidebar.Invalidate();

            pnlStatusBar.Paint += (s, e) =>
            {
                using var caneta = new Pen(CorBorda);
                e.Graphics.DrawLine(caneta, 0, 0, pnlStatusBar.Width, 0);
            };
            pnlStatusBar.Resize += (s, e) => pnlStatusBar.Invalidate();

            // Ícone da barra lateral acende (fica branco) ao passar o mouse.
            foreach (var botao in BotoesDeNavegacao())
            {
                botao.MouseEnter += (s, e) =>
                {
                    if (!botao.Checked)
                        botao.Image = CarregarImagem($"ic_{botao.Tag}_branco.png");
                };
                botao.MouseLeave += (s, e) =>
                {
                    if (!botao.Checked)
                        botao.Image = CarregarImagem($"ic_{botao.Tag}_muted.png");
                };
            }

            // Os textos das opções de cookies também marcam a opção ao serem clicados.
            lblOpcaoArquivoCookies.Click += (s, e) => { if (radCookiesArquivo.Enabled) radCookiesArquivo.Checked = true; };
            lblOpcaoNavegadorCookies.Click += (s, e) => { if (radCookiesNavegador.Enabled) radCookiesNavegador.Checked = true; };
            lblSemCookies.Click += (s, e) => { if (radSemCookies.Enabled) radSemCookies.Checked = true; };

            // A dica "a prévia aparece aqui" acompanha o logo do estado vazio.
            picLogoGrande.VisibleChanged += (s, e) => lblDicaPrevia.Visible = picLogoGrande.Visible;

            // Menu do botão direito no histórico, no mesmo tema escuro.
            menuHistorico.Renderer = new ToolStripProfessionalRenderer(new CoresMenuEscuro());
            menuHistorico.BackColor = CorCard;
            menuHistorico.ForeColor = CorTexto;
            menuHistorico.Font = Fonte(9.75F);
            menuItemAbrirPasta.ForeColor = CorTexto;

            // Versão na base da barra lateral (vem do <Version> do .csproj).
            try
            {
                var versao = typeof(MainForm).Assembly.GetName().Version;
                if (versao is not null)
                    lblVersao.Text = $"versão {versao.Major}.{versao.Minor}";
            }
            catch
            {
                // Mantém o texto padrão do Designer.
            }
        }

        /// <summary>Coloca os ícones da pasta Assets nos botões e campos (chamado em CarregarImagens).</summary>
        private void AplicarIcones()
        {
            txtUrl.IconLeft = CarregarImagem("ic_link_muted.png");
            txtSavePath.IconLeft = CarregarImagem("ic_pasta_muted.png");

            btnDownloadMp4.Image = CarregarImagem("ic_video_branco.png");
            btnDownloadMp3.Image = CarregarImagem("ic_musica_roxo.png");
            btnLimparHistorico.Image = CarregarImagem("ic_lixeira_rosa.png");
            btnAlterarLocal.Image = CarregarImagem("ic_pasta_branco.png");
            btnAbrirPasta.Image = CarregarImagem("ic_abrir_branco.png");
            btnAtualizarYtDlp.Image = CarregarImagem("ic_atualizar_branco.png");
            btnExtrairCookies.Image = CarregarImagem("ic_cookie_roxo.png");
            menuItemAbrirPasta.Image = CarregarImagem("ic_abrir_branco.png");
        }

        private Guna2Button[] BotoesDeNavegacao() =>
            new[] { btnNavInicio, btnNavHistorico, btnNavPasta, btnNavConfig };

        /// <summary>
        /// Marca na barra lateral a página aberta: fundo destacado e ícone roxo.
        /// A "chave" de cada botão (Tag) é a mesma usada no nome da página.
        /// </summary>
        private void AtualizarIconesDeNavegacao(string paginaAtiva)
        {
            foreach (var botao in BotoesDeNavegacao())
            {
                bool ativo = (botao.Tag as string) == paginaAtiva;
                botao.Checked = ativo;
                botao.Image = CarregarImagem($"ic_{botao.Tag}_{(ativo ? "roxo" : "muted")}.png");
            }
        }

        private void btnNavInicio_Click(object sender, EventArgs e) => MostrarPagina("home");
        private void btnNavHistorico_Click(object sender, EventArgs e) => MostrarPagina("historico");
        private void btnNavPasta_Click(object sender, EventArgs e) => MostrarPagina("pasta");
        private void btnNavConfig_Click(object sender, EventArgs e) => MostrarPagina("config");

        /// <summary>Novo botão da página Pasta: abre a pasta de destino no Explorador de Arquivos.</summary>
        private void btnAbrirPasta_Click(object sender, EventArgs e)
        {
            string pasta = txtSavePath.Text;
            if (string.IsNullOrWhiteSpace(pasta) || !Directory.Exists(pasta))
            {
                MessageBox.Show(this,
                    "Essa pasta não existe mais. Use \"Alterar pasta\" para escolher outra.",
                    "Pasta não encontrada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AbrirNoExplorador(pasta);
        }
    }

    // =====================================================================
    // Lista do histórico desenhada à mão (o ListView padrão do Windows
    // não tem tema escuro): linhas mais altas, etiqueta colorida MP4/MP3,
    // destaque ao passar o mouse e barra de rolagem escura.
    // =====================================================================
    internal sealed class ListaEscura : ListView
    {
        private int _linhaSobMouse = -1;
        private readonly ImageList _alturaDaLinha = new() { ImageSize = new Size(1, 40) };
        private readonly Font _fonteEtiqueta = new("Segoe UI Semibold", 8.25F);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);

        public ListaEscura()
        {
            OwnerDraw = true;
            DoubleBuffered = true;
            // Truque clássico: a altura da "imagem" pequena define a altura das linhas.
            SmallImageList = _alturaDaLinha;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try { SetWindowTheme(Handle, "DarkMode_Explorer", null); } catch { /* barra de rolagem clara */ }
        }

        /// <summary>A coluna do nome ocupa todo o espaço que sobra entre "Tipo" e "Data".</summary>
        public void AjustarColunas()
        {
            if (Columns.Count < 3 || ClientSize.Width <= 0)
                return;

            int sobra = ClientSize.Width - Columns[0].Width - Columns[2].Width - 2;
            Columns[1].Width = Math.Max(120, sobra);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            AjustarColunas();
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true; // o cabeçalho fica oculto (HeaderStyle = None)
            base.OnDrawColumnHeader(e);
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            // Tudo é desenhado célula a célula em OnDrawSubItem.
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            if (e.Item is null || e.Header is null)
                return;

            var g = e.Graphics;
            // Na primeira coluna o Windows informa a linha inteira; corrigimos a largura.
            var celula = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Header.Width, e.Bounds.Height);

            bool selecionado = e.Item.Selected;
            bool sobMouse = e.ItemIndex == _linhaSobMouse;
            Color fundo = selecionado ? MainForm.CorNavAtivo : (sobMouse ? MainForm.CorCampoHover : BackColor);

            using (var pincel = new SolidBrush(fundo))
                g.FillRectangle(pincel, celula);
            using (var caneta = new Pen(MainForm.CorBorda))
                g.DrawLine(caneta, celula.Left, celula.Bottom - 1, celula.Right, celula.Bottom - 1);

            string texto = e.SubItem?.Text ?? "";
            const TextFormatFlags Base = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                                         TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;

            switch (e.ColumnIndex)
            {
                case 0:
                    if (selecionado)
                    {
                        using var barra = new SolidBrush(MainForm.CorAcento);
                        g.FillRectangle(barra, celula.X, celula.Y + 10, 3, celula.Height - 20);
                    }
                    DesenharEtiqueta(g, celula, e.Item.Text);
                    break;

                case 1:
                    TextRenderer.DrawText(g, texto, Font,
                        new Rectangle(celula.X + 4, celula.Y, celula.Width - 8, celula.Height),
                        MainForm.CorTexto, Base | TextFormatFlags.Left);
                    break;

                default:
                    TextRenderer.DrawText(g, texto, Font,
                        new Rectangle(celula.X, celula.Y, celula.Width - 14, celula.Height),
                        MainForm.CorTextoSecundario, Base | TextFormatFlags.Right);
                    break;
            }
        }

        private void DesenharEtiqueta(Graphics g, Rectangle celula, string texto)
        {
            bool audio = texto.Equals("MP3", StringComparison.OrdinalIgnoreCase);
            Color fundo = audio ? Color.FromArgb(70, 32, 84) : Color.FromArgb(52, 38, 100);
            Color letra = audio ? Color.FromArgb(240, 171, 252) : Color.FromArgb(196, 181, 253);

            var etiqueta = new Rectangle(celula.X + 14, celula.Y + (celula.Height - 22) / 2, 48, 22);

            var suavizacaoAnterior = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var caminho = Arredondado(etiqueta, 11))
            using (var pincel = new SolidBrush(fundo))
                g.FillPath(pincel, caminho);
            g.SmoothingMode = suavizacaoAnterior;

            TextRenderer.DrawText(g, texto, _fonteEtiqueta, etiqueta, letra,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }

        private static GraphicsPath Arredondado(Rectangle r, int raio)
        {
            int d = raio * 2;
            var caminho = new GraphicsPath();
            caminho.AddArc(r.X, r.Y, d, d, 180, 90);
            caminho.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            caminho.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            caminho.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            caminho.CloseFigure();
            return caminho;
        }

        // Destaque da linha sob o mouse (também evita um defeito antigo do
        // ListView desenhado à mão, que "apaga" linhas ao passar o mouse).
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int indice = GetItemAt(e.X, e.Y)?.Index ?? -1;
            if (indice == _linhaSobMouse)
                return;

            InvalidarLinha(_linhaSobMouse);
            _linhaSobMouse = indice;
            InvalidarLinha(indice);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            InvalidarLinha(_linhaSobMouse);
            _linhaSobMouse = -1;
        }

        private void InvalidarLinha(int indice)
        {
            if (indice >= 0 && indice < Items.Count)
                Invalidate(Items[indice].Bounds);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _alturaDaLinha.Dispose();
                _fonteEtiqueta.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>Cores do menu de contexto (botão direito) no tema escuro.</summary>
    internal sealed class CoresMenuEscuro : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => MainForm.CorCard;
        public override Color MenuBorder => MainForm.CorBordaForte;
        public override Color MenuItemBorder => MainForm.CorNavAtivo;
        public override Color MenuItemSelected => MainForm.CorNavAtivo;
        public override Color MenuItemSelectedGradientBegin => MainForm.CorNavAtivo;
        public override Color MenuItemSelectedGradientEnd => MainForm.CorNavAtivo;
        public override Color ImageMarginGradientBegin => MainForm.CorCard;
        public override Color ImageMarginGradientMiddle => MainForm.CorCard;
        public override Color ImageMarginGradientEnd => MainForm.CorCard;
    }
}

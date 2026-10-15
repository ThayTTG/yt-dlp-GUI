using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace YtDlpGui
{
    // =====================================================================
    // Esqueleto visual da janela principal (versão 2.0 — tema escuro roxo,
    // mesma família visual do launcher NokyLabs).
    //
    // Estrutura geral (tudo por Dock, sem posições "mágicas" na janela):
    //
    //   ┌──────────┬───────────────────────────────────────┐
    //   │ Sidebar  │  pnlContent (Dock Fill)               │
    //   │ (Left,   │    └ uma "página" visível por vez     │
    //   │  220px)  │      (Home, Histórico, Pasta, Config) │
    //   │          ├───────────────────────────────────────┤
    //   │          │  pnlStatusBar (Bottom, 64px)          │
    //   └──────────┴───────────────────────────────────────┘
    //
    // Os cards das páginas Home, Pasta e Configurações usam Anchor = None,
    // então ficam sempre centralizados quando a janela é maximizada.
    // O card do Histórico cresce junto com a janela (mais linhas visíveis).
    //
    // Comportamentos visuais que não cabem aqui (lista desenhada à mão,
    // barra de título escura, ícones com hover etc.) ficam em Tema.cs.
    // =====================================================================
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Paleta (roxo escuro)

        internal static readonly Color CorFundo          = Color.FromArgb(14, 11, 22);    // fundo da janela
        internal static readonly Color CorSidebar        = Color.FromArgb(19, 15, 30);    // barra lateral e rodapé
        internal static readonly Color CorCard           = Color.FromArgb(26, 21, 40);    // cards
        internal static readonly Color CorCampo          = Color.FromArgb(34, 27, 52);    // campos e seções internas
        internal static readonly Color CorCampoHover     = Color.FromArgb(44, 36, 66);
        internal static readonly Color CorBorda          = Color.FromArgb(46, 37, 69);    // bordas sutis
        internal static readonly Color CorBordaForte     = Color.FromArgb(75, 61, 112);
        internal static readonly Color CorNavAtivo       = Color.FromArgb(42, 33, 66);
        internal static readonly Color CorNavHover       = Color.FromArgb(30, 24, 46);

        internal static readonly Color CorTexto          = Color.FromArgb(237, 233, 246);
        internal static readonly Color CorTextoSecundario= Color.FromArgb(164, 155, 184);
        internal static readonly Color CorTextoApagado   = Color.FromArgb(110, 101, 133);

        internal static readonly Color CorAcento         = Color.FromArgb(139, 92, 246);  // violeta
        internal static readonly Color CorAcentoHover    = Color.FromArgb(124, 77, 235);
        internal static readonly Color CorAcentoClaro    = Color.FromArgb(178, 148, 255);
        internal static readonly Color CorAcento2        = Color.FromArgb(217, 70, 239);  // magenta (gradiente)

        internal static readonly Color CorPerigo         = Color.FromArgb(244, 63, 94);
        internal static readonly Color CorPerigoHover    = Color.FromArgb(225, 29, 72);
        internal static readonly Color CorPerigoTexto    = Color.FromArgb(251, 113, 133);

        internal static readonly Color CorDesabilitado   = Color.FromArgb(38, 31, 56);
        internal static readonly Color CorTextoDesabilitado = Color.FromArgb(96, 88, 120);

        #endregion

        #region Helpers de estilo

        private static Font Fonte(float tamanho) => new Font("Segoe UI", tamanho);
        private static Font FonteForte(float tamanho) => new Font("Segoe UI Semibold", tamanho);

        private static void EstiloTitulo(Label l, string texto)
        {
            l.AutoSize = true;
            l.Font = FonteForte(17F);
            l.ForeColor = CorTexto;
            l.Text = texto;
        }

        private static void EstiloSubtitulo(Label l, string texto)
        {
            l.AutoSize = true;
            l.Font = Fonte(9.75F);
            l.ForeColor = CorTextoSecundario;
            l.Text = texto;
        }

        private static void EstiloRotulo(Label l, string texto)
        {
            l.AutoSize = true;
            l.Font = FonteForte(9.25F);
            l.ForeColor = CorTextoSecundario;
            l.Text = texto;
        }

        private static void EstiloCard(Guna2Panel p)
        {
            p.BorderRadius = 16;
            p.FillColor = CorCard;
            p.BorderColor = CorBorda;
            p.BorderThickness = 1;
        }

        private static void EstiloSecao(Guna2Panel p)
        {
            p.BorderRadius = 12;
            p.FillColor = CorCampo;
            p.BorderColor = CorBorda;
            p.BorderThickness = 1;
        }

        private static void EstiloTextBox(Guna2TextBox c)
        {
            c.BorderRadius = 10;
            c.BorderColor = CorBorda;
            c.FillColor = CorCampo;
            c.ForeColor = CorTexto;
            c.Font = Fonte(10.5F);
            c.PlaceholderForeColor = CorTextoApagado;
            c.FocusedState.BorderColor = CorAcento;
            c.HoverState.BorderColor = CorBordaForte;
            c.DisabledState.BorderColor = CorBorda;
            c.DisabledState.FillColor = CorDesabilitado;
            c.DisabledState.ForeColor = CorTextoDesabilitado;
            c.DisabledState.PlaceholderForeColor = CorTextoDesabilitado;
        }

        /// <summary>Botão principal: violeta cheio.</summary>
        private static void EstiloPrimario(Guna2Button c)
        {
            c.BorderRadius = 12;
            c.FillColor = CorAcento;
            c.ForeColor = Color.White;
            c.Font = FonteForte(10.5F);
            c.HoverState.FillColor = CorAcentoHover;
            c.DisabledState.FillColor = CorDesabilitado;
            c.DisabledState.ForeColor = CorTextoDesabilitado;
            c.DisabledState.BorderColor = CorDesabilitado;
            c.Cursor = Cursors.Hand;
            c.Animated = true;
        }

        /// <summary>Botão secundário: escuro com borda; acende a borda no hover.</summary>
        private static void EstiloSecundario(Guna2Button c)
        {
            c.BorderRadius = 12;
            c.FillColor = CorCampo;
            c.BorderColor = CorBordaForte;
            c.BorderThickness = 1;
            c.ForeColor = CorTexto;
            c.Font = FonteForte(10F);
            c.HoverState.FillColor = CorCampoHover;
            c.HoverState.BorderColor = CorAcento;
            c.DisabledState.FillColor = CorDesabilitado;
            c.DisabledState.ForeColor = CorTextoDesabilitado;
            c.DisabledState.BorderColor = CorDesabilitado;
            c.Cursor = Cursors.Hand;
            c.Animated = true;
        }

        /// <summary>Botão de ação destrutiva (ex.: limpar histórico): contorno rosado.</summary>
        private static void EstiloPerigoContorno(Guna2Button c)
        {
            c.BorderRadius = 10;
            c.FillColor = CorCard;
            c.BorderColor = Color.FromArgb(90, 40, 60);
            c.BorderThickness = 1;
            c.ForeColor = CorPerigoTexto;
            c.Font = FonteForte(9.5F);
            c.HoverState.FillColor = Color.FromArgb(48, 22, 36);
            c.HoverState.BorderColor = CorPerigo;
            c.DisabledState.FillColor = CorCard;
            c.DisabledState.ForeColor = CorTextoDesabilitado;
            c.DisabledState.BorderColor = CorBorda;
            c.Cursor = Cursors.Hand;
            c.Animated = true;
        }

        /// <summary>Item de navegação da barra lateral (ícone + rótulo).</summary>
        private static void EstiloNav(Guna2Button c, string texto, string chaveIcone)
        {
            c.ButtonMode = Guna.UI2.WinForms.Enums.ButtonMode.RadioButton;
            c.BorderRadius = 10;
            c.FillColor = CorSidebar;
            c.ForeColor = CorTextoSecundario;
            c.Font = FonteForte(10F);
            c.HoverState.FillColor = CorNavHover;
            c.HoverState.ForeColor = CorTexto;
            c.CheckedState.FillColor = CorNavAtivo;
            c.CheckedState.ForeColor = CorTexto;
            c.ImageSize = new Size(20, 20);
            c.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            c.ImageOffset = new Point(6, 0);
            c.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            c.TextOffset = new Point(10, 0);
            c.Cursor = Cursors.Hand;
            c.Text = texto;
            c.Tag = chaveIcone;   // usado para escolher o ícone (ic_<chave>_<cor>.png)
            c.Size = new Size(188, 42);
            c.Animated = true;
        }

        private static void EstiloRadio(Guna2RadioButton r)
        {
            r.CheckedState.BorderColor = CorAcento;
            r.CheckedState.BorderThickness = 0;
            r.CheckedState.FillColor = CorAcento;
            r.CheckedState.InnerColor = Color.White;
            r.UncheckedState.BorderColor = CorBordaForte;
            r.UncheckedState.BorderThickness = 2;
            r.UncheckedState.FillColor = Color.Transparent;
            r.UncheckedState.InnerColor = Color.Transparent;
            r.Size = new Size(20, 20);
            r.Text = "";
            r.Cursor = Cursors.Hand;
        }

        private static void EstiloCheck(Guna2CheckBox c, string texto)
        {
            c.CheckedState.BorderColor = CorAcento;
            c.CheckedState.BorderRadius = 4;
            c.CheckedState.BorderThickness = 0;
            c.CheckedState.FillColor = CorAcento;
            c.UncheckedState.BorderColor = CorBordaForte;
            c.UncheckedState.BorderRadius = 4;
            c.UncheckedState.BorderThickness = 2;
            c.UncheckedState.FillColor = Color.Transparent;
            c.CheckMarkColor = Color.White;
            c.Font = Fonte(10F);
            c.ForeColor = CorTexto;
            c.Text = texto;
            c.Cursor = Cursors.Hand;
        }

        #endregion

        // ---- Estrutura ----
        private Panel pnlSidebar;
        private PictureBox picLogo;
        private Label lblMarca;
        private Label lblMarcaSub;
        private Label lblSecaoMenu;
        private Guna2Button btnNavInicio;
        private Guna2Button btnNavHistorico;
        private Guna2Button btnNavPasta;
        private Guna2Button btnNavConfig;
        private Label lblVersao;

        private Panel pnlContent;

        private Panel pnlStatusBar;
        private Label lblStatus;
        private Guna2ProgressBar progressBar1;
        private Label lblProgressPercent;
        private Guna2Button btnCancelar;
        private NotifyIcon notifyIcon1;

        // ---- Home ----
        private Panel pnlHome;
        private Guna2Panel cardHome;
        private Label lblTituloCardHome;
        private Label lblSubtituloCardHome;
        private Label lblUrlLabel;
        private Guna2TextBox txtUrl;
        // Área de prévia
        private PictureBox picLogoGrande;
        private Label lblDicaPrevia;
        private Label lblSpinner;
        private Panel pnlPrevia;
        private PictureBox picThumb;
        private Label lblTituloVideo;
        private Label lblInfoVideo;
        // Botões e opção de playlist
        private Guna2Button btnDownloadMp4;
        private Guna2Button btnDownloadMp3;
        private Guna2CheckBox chkPlaylist;

        // ---- Histórico ----
        private Panel pnlHistorico;
        private Guna2Panel cardHistorico;
        private Label lblTituloCardHistorico;
        private Label lblSubtituloCardHistorico;
        private Guna2Panel pnlCardListaHistorico;
        private ListaEscura listViewHistorico;
        private ColumnHeader colTipo;
        private ColumnHeader colNome;
        private ColumnHeader colData;
        private ContextMenuStrip menuHistorico;
        private ToolStripMenuItem menuItemAbrirPasta;
        private Guna2Button btnLimparHistorico;
        private Label lblHistoricoVazio;

        // ---- Pasta ----
        private Panel pnlPasta;
        private Guna2Panel cardPasta;
        private Label lblTituloCardPasta;
        private Label lblSubtituloCardPasta;
        private PictureBox picIlustracaoPasta;
        private Label lblLocalLabel;
        private Guna2TextBox txtSavePath;
        private Guna2Button btnAlterarLocal;
        private Guna2Button btnAbrirPasta;

        // ---- Configurações ----
        private Panel pnlConfiguracoes;
        private Guna2Panel cardConfig;
        private Label lblTituloCardConfig;
        private Label lblSubtituloCardConfig;
        private PictureBox picLogoConfig;
        private Guna2Panel secCookies;
        private Guna2Panel secPreferencias;
        // Seção da esquerda (cookies)
        private Label lblUsoCookiesTitulo;
        private Guna2Button btnAjudaCookies;
        private Label lblOpcaoArquivoCookies;
        private Guna2RadioButton radCookiesArquivo;
        private Guna2TextBox txtCookiesArquivo;
        private Label lblOpcaoNavegadorCookies;
        private Guna2RadioButton radCookiesNavegador;
        private Guna2ComboBox cmbNavegador;
        private Guna2Button btnExtrairCookies;
        private Label lblExtrairCookiesStatus;
        private Guna2RadioButton radSemCookies;
        private Label lblSemCookies;
        // Seção da direita (playlist + atualização)
        private Label lblPlaylistTitulo;
        private Label lblPlaylistSubtitulo;
        private Guna2CheckBox chkOrganizarPlaylist;
        private Label lblAtualizarTitulo;
        private Label lblAtualizarStatus;
        private Guna2Button btnAtualizarYtDlp;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // ---- instanciar todos ----
            this.pnlSidebar = new Panel();
            this.picLogo = new PictureBox();
            this.lblMarca = new Label();
            this.lblMarcaSub = new Label();
            this.lblSecaoMenu = new Label();
            this.btnNavInicio = new Guna2Button();
            this.btnNavHistorico = new Guna2Button();
            this.btnNavPasta = new Guna2Button();
            this.btnNavConfig = new Guna2Button();
            this.lblVersao = new Label();

            this.pnlContent = new Panel();

            this.pnlStatusBar = new Panel();
            this.lblStatus = new Label();
            this.progressBar1 = new Guna2ProgressBar();
            this.lblProgressPercent = new Label();
            this.btnCancelar = new Guna2Button();
            this.notifyIcon1 = new NotifyIcon(this.components);

            this.pnlHome = new Panel();
            this.cardHome = new Guna2Panel();
            this.lblTituloCardHome = new Label();
            this.lblSubtituloCardHome = new Label();
            this.lblUrlLabel = new Label();
            this.txtUrl = new Guna2TextBox();
            this.picLogoGrande = new PictureBox();
            this.lblDicaPrevia = new Label();
            this.lblSpinner = new Label();
            this.pnlPrevia = new Panel();
            this.picThumb = new PictureBox();
            this.lblTituloVideo = new Label();
            this.lblInfoVideo = new Label();
            this.btnDownloadMp4 = new Guna2Button();
            this.btnDownloadMp3 = new Guna2Button();
            this.chkPlaylist = new Guna2CheckBox();

            this.pnlHistorico = new Panel();
            this.cardHistorico = new Guna2Panel();
            this.lblTituloCardHistorico = new Label();
            this.lblSubtituloCardHistorico = new Label();
            this.pnlCardListaHistorico = new Guna2Panel();
            this.listViewHistorico = new ListaEscura();
            this.colTipo = new ColumnHeader();
            this.colNome = new ColumnHeader();
            this.colData = new ColumnHeader();
            this.menuHistorico = new ContextMenuStrip(this.components);
            this.menuItemAbrirPasta = new ToolStripMenuItem();
            this.btnLimparHistorico = new Guna2Button();
            this.lblHistoricoVazio = new Label();

            this.pnlPasta = new Panel();
            this.cardPasta = new Guna2Panel();
            this.lblTituloCardPasta = new Label();
            this.lblSubtituloCardPasta = new Label();
            this.picIlustracaoPasta = new PictureBox();
            this.lblLocalLabel = new Label();
            this.txtSavePath = new Guna2TextBox();
            this.btnAlterarLocal = new Guna2Button();
            this.btnAbrirPasta = new Guna2Button();

            this.pnlConfiguracoes = new Panel();
            this.cardConfig = new Guna2Panel();
            this.lblTituloCardConfig = new Label();
            this.lblSubtituloCardConfig = new Label();
            this.picLogoConfig = new PictureBox();
            this.secCookies = new Guna2Panel();
            this.secPreferencias = new Guna2Panel();
            this.lblUsoCookiesTitulo = new Label();
            this.btnAjudaCookies = new Guna2Button();
            this.lblOpcaoArquivoCookies = new Label();
            this.radCookiesArquivo = new Guna2RadioButton();
            this.txtCookiesArquivo = new Guna2TextBox();
            this.lblOpcaoNavegadorCookies = new Label();
            this.radCookiesNavegador = new Guna2RadioButton();
            this.cmbNavegador = new Guna2ComboBox();
            this.btnExtrairCookies = new Guna2Button();
            this.lblExtrairCookiesStatus = new Label();
            this.radSemCookies = new Guna2RadioButton();
            this.lblSemCookies = new Label();
            this.lblPlaylistTitulo = new Label();
            this.lblPlaylistSubtitulo = new Label();
            this.chkOrganizarPlaylist = new Guna2CheckBox();
            this.lblAtualizarTitulo = new Label();
            this.lblAtualizarStatus = new Label();
            this.btnAtualizarYtDlp = new Guna2Button();

            foreach (var pb in new PictureBox[]
                { picLogo, picLogoGrande, picIlustracaoPasta, picLogoConfig, picThumb })
                ((System.ComponentModel.ISupportInitialize)pb).BeginInit();
            this.menuHistorico.SuspendLayout();
            this.SuspendLayout();

            // Tamanhos de referência (a janela abre com 980 x 660 de área útil):
            //   barra lateral: 220 de largura
            //   coluna de conteúdo: 760 x 596  +  rodapé: 760 x 64

            // ==============================================================
            // BARRA LATERAL
            // ==============================================================
            this.pnlSidebar.BackColor = CorSidebar;
            this.pnlSidebar.Dock = DockStyle.Left;
            this.pnlSidebar.Size = new Size(220, 660);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Controls.AddRange(new Control[]
            {
                picLogo, lblMarca, lblMarcaSub, lblSecaoMenu,
                btnNavInicio, btnNavHistorico, btnNavPasta, btnNavConfig, lblVersao
            });

            this.picLogo.BackColor = Color.Transparent;
            this.picLogo.Cursor = Cursors.Hand;
            this.picLogo.Location = new Point(20, 22);
            this.picLogo.Size = new Size(40, 40);
            this.picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            this.picLogo.Click += new System.EventHandler(this.picLogo_Click);

            this.lblMarca.AutoSize = true;
            this.lblMarca.Font = FonteForte(11.5F);
            this.lblMarca.ForeColor = CorTexto;
            this.lblMarca.Location = new Point(68, 21);
            this.lblMarca.Text = "YT-DLP GUI";

            this.lblMarcaSub.AutoSize = true;
            this.lblMarcaSub.Font = Fonte(8.5F);
            this.lblMarcaSub.ForeColor = CorAcentoClaro;
            this.lblMarcaSub.Location = new Point(69, 43);
            this.lblMarcaSub.Text = "NokyLabs";

            this.lblSecaoMenu.AutoSize = true;
            this.lblSecaoMenu.Font = FonteForte(7.75F);
            this.lblSecaoMenu.ForeColor = CorTextoApagado;
            this.lblSecaoMenu.Location = new Point(24, 92);
            this.lblSecaoMenu.Text = "NAVEGAÇÃO";

            EstiloNav(this.btnNavInicio, "Início", "home");
            this.btnNavInicio.Location = new Point(16, 114);
            this.btnNavInicio.Name = "btnNavInicio";
            this.btnNavInicio.Click += new System.EventHandler(this.btnNavInicio_Click);

            EstiloNav(this.btnNavHistorico, "Histórico", "historico");
            this.btnNavHistorico.Location = new Point(16, 160);
            this.btnNavHistorico.Name = "btnNavHistorico";
            this.btnNavHistorico.Click += new System.EventHandler(this.btnNavHistorico_Click);

            EstiloNav(this.btnNavPasta, "Pasta de destino", "pasta");
            this.btnNavPasta.Location = new Point(16, 206);
            this.btnNavPasta.Name = "btnNavPasta";
            this.btnNavPasta.Click += new System.EventHandler(this.btnNavPasta_Click);

            EstiloNav(this.btnNavConfig, "Configurações", "config");
            this.btnNavConfig.Location = new Point(16, 252);
            this.btnNavConfig.Name = "btnNavConfig";
            this.btnNavConfig.Click += new System.EventHandler(this.btnNavConfig_Click);

            this.lblVersao.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.lblVersao.AutoSize = true;
            this.lblVersao.Font = Fonte(8.25F);
            this.lblVersao.ForeColor = CorTextoApagado;
            this.lblVersao.Location = new Point(24, 628);
            this.lblVersao.Text = "versão 2.0";

            // ==============================================================
            // ÁREA DE CONTEÚDO (as quatro páginas ficam empilhadas aqui)
            // ==============================================================
            this.pnlContent.BackColor = CorFundo;
            this.pnlContent.Dock = DockStyle.Fill;
            this.pnlContent.Size = new Size(760, 596);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Controls.AddRange(new Control[]
                { pnlConfiguracoes, pnlPasta, pnlHistorico, pnlHome });

            foreach (var pagina in new Panel[] { pnlHome, pnlHistorico, pnlPasta, pnlConfiguracoes })
            {
                pagina.BackColor = CorFundo;
                pagina.Dock = DockStyle.Fill;
                pagina.Size = new Size(760, 596);
            }

            // ==============================================================
            // HOME — baixar vídeo
            // ==============================================================
            this.pnlHome.Name = "pnlHome";
            this.pnlHome.Controls.Add(cardHome);

            EstiloCard(this.cardHome);
            this.cardHome.Anchor = AnchorStyles.None;          // fica centralizado
            this.cardHome.Location = new Point(36, 54);
            this.cardHome.Size = new Size(688, 488);
            this.cardHome.Name = "cardHome";
            // Ordem = camadas: o primeiro da lista fica por cima.
            this.cardHome.Controls.AddRange(new Control[]
            {
                lblSpinner, lblDicaPrevia, picLogoGrande, pnlPrevia,
                lblTituloCardHome, lblSubtituloCardHome, lblUrlLabel, txtUrl,
                btnDownloadMp4, btnDownloadMp3, chkPlaylist
            });

            EstiloTitulo(this.lblTituloCardHome, "Baixar vídeo");
            this.lblTituloCardHome.Location = new Point(26, 22);

            EstiloSubtitulo(this.lblSubtituloCardHome, "Cole o link de um vídeo ou playlist e escolha o formato.");
            this.lblSubtituloCardHome.Location = new Point(28, 58);

            EstiloRotulo(this.lblUrlLabel, "Link do vídeo");
            this.lblUrlLabel.Location = new Point(28, 102);

            EstiloTextBox(this.txtUrl);
            this.txtUrl.Location = new Point(28, 126);
            this.txtUrl.Size = new Size(632, 46);
            this.txtUrl.PlaceholderText = "https://www.youtube.com/watch?v=...";
            this.txtUrl.IconLeftSize = new Size(18, 18);
            this.txtUrl.IconLeftOffset = new Point(6, 0);
            this.txtUrl.TextChanged += new System.EventHandler(this.txtUrl_TextChanged);

            // --- Zona da prévia (y 188 a 348): estado vazio, carregando ou prévia ---
            this.picLogoGrande.BackColor = Color.Transparent;
            this.picLogoGrande.Location = new Point(312, 206);
            this.picLogoGrande.Size = new Size(64, 64);
            this.picLogoGrande.SizeMode = PictureBoxSizeMode.Zoom;

            this.lblDicaPrevia.AutoSize = false;
            this.lblDicaPrevia.Font = Fonte(9.5F);
            this.lblDicaPrevia.ForeColor = CorTextoApagado;
            this.lblDicaPrevia.Location = new Point(28, 282);
            this.lblDicaPrevia.Size = new Size(632, 24);
            this.lblDicaPrevia.TextAlign = ContentAlignment.MiddleCenter;
            this.lblDicaPrevia.Text = "A prévia do vídeo aparece aqui assim que você colar o link.";

            this.lblSpinner.AutoSize = false;
            this.lblSpinner.Font = new Font("Segoe UI Symbol", 11F);   // tem os caracteres ⠋⠙⠸ do spinner
            this.lblSpinner.ForeColor = CorAcentoClaro;
            this.lblSpinner.Location = new Point(28, 252);
            this.lblSpinner.Size = new Size(632, 30);
            this.lblSpinner.TextAlign = ContentAlignment.MiddleCenter;
            this.lblSpinner.Text = "⠋ Buscando informações...";
            this.lblSpinner.Visible = false;

            this.pnlPrevia.BackColor = CorCard;
            this.pnlPrevia.Location = new Point(28, 190);
            this.pnlPrevia.Size = new Size(632, 152);
            this.pnlPrevia.Visible = false;
            this.pnlPrevia.Controls.AddRange(new Control[] { picThumb, lblTituloVideo, lblInfoVideo });

            this.picThumb.BackColor = CorCampo;
            this.picThumb.Location = new Point(0, 4);
            this.picThumb.Size = new Size(256, 144);   // 16:9
            this.picThumb.SizeMode = PictureBoxSizeMode.Zoom;

            this.lblTituloVideo.AutoSize = false;
            this.lblTituloVideo.Font = FonteForte(12F);
            this.lblTituloVideo.ForeColor = CorTexto;
            this.lblTituloVideo.Location = new Point(276, 14);
            this.lblTituloVideo.Size = new Size(356, 70);
            this.lblTituloVideo.Text = "";

            this.lblInfoVideo.AutoSize = false;
            this.lblInfoVideo.AutoEllipsis = true;
            this.lblInfoVideo.Font = Fonte(9.75F);
            this.lblInfoVideo.ForeColor = CorTextoSecundario;
            this.lblInfoVideo.Location = new Point(276, 92);
            this.lblInfoVideo.Size = new Size(356, 24);
            this.lblInfoVideo.Text = "";

            // --- Botões de download ---
            EstiloPrimario(this.btnDownloadMp4);
            this.btnDownloadMp4.Location = new Point(28, 364);
            this.btnDownloadMp4.Size = new Size(308, 54);
            this.btnDownloadMp4.Text = "Baixar vídeo  ·  MP4";
            this.btnDownloadMp4.ImageSize = new Size(22, 22);
            this.btnDownloadMp4.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.btnDownloadMp4.ImageOffset = new Point(10, 0);
            this.btnDownloadMp4.Click += new System.EventHandler(this.btnDownloadMp4_Click);

            EstiloSecundario(this.btnDownloadMp3);
            this.btnDownloadMp3.Font = FonteForte(10.5F);
            this.btnDownloadMp3.Location = new Point(352, 364);
            this.btnDownloadMp3.Size = new Size(308, 54);
            this.btnDownloadMp3.Text = "Baixar áudio  ·  MP3";
            this.btnDownloadMp3.ImageSize = new Size(22, 22);
            this.btnDownloadMp3.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.btnDownloadMp3.ImageOffset = new Point(10, 0);
            this.btnDownloadMp3.Click += new System.EventHandler(this.btnDownloadMp3_Click);

            EstiloCheck(this.chkPlaylist, "Baixar a playlist inteira (quando o link for de uma playlist)");
            this.chkPlaylist.AutoSize = true;
            this.chkPlaylist.Location = new Point(28, 440);

            // ==============================================================
            // HISTÓRICO — lista desenhada no tema escuro (ver ListaEscura em Tema.cs)
            // ==============================================================
            this.pnlHistorico.Name = "pnlHistorico";
            this.pnlHistorico.Visible = false;
            this.pnlHistorico.Controls.Add(cardHistorico);

            EstiloCard(this.cardHistorico);
            this.cardHistorico.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.cardHistorico.Location = new Point(36, 28);
            this.cardHistorico.Size = new Size(688, 540);
            this.cardHistorico.Name = "cardHistorico";
            this.cardHistorico.Controls.AddRange(new Control[]
                { lblTituloCardHistorico, lblSubtituloCardHistorico, pnlCardListaHistorico, btnLimparHistorico });

            EstiloTitulo(this.lblTituloCardHistorico, "Histórico de downloads");
            this.lblTituloCardHistorico.Location = new Point(26, 22);

            EstiloSubtitulo(this.lblSubtituloCardHistorico, "Clique duas vezes num item (ou use o botão direito) para abrir a pasta dele.");
            this.lblSubtituloCardHistorico.Location = new Point(28, 58);

            EstiloSecao(this.pnlCardListaHistorico);
            this.pnlCardListaHistorico.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.pnlCardListaHistorico.Location = new Point(28, 96);
            this.pnlCardListaHistorico.Size = new Size(632, 368);
            this.pnlCardListaHistorico.Padding = new Padding(6, 8, 6, 8);
            // O aviso de "vazio" vem antes da lista para ficar por cima dela.
            this.pnlCardListaHistorico.Controls.AddRange(new Control[] { lblHistoricoVazio, listViewHistorico });

            this.lblHistoricoVazio.Anchor = AnchorStyles.None;
            this.lblHistoricoVazio.AutoSize = false;
            this.lblHistoricoVazio.BackColor = CorCampo;
            this.lblHistoricoVazio.Font = Fonte(10F);
            this.lblHistoricoVazio.ForeColor = CorTextoApagado;
            this.lblHistoricoVazio.Location = new Point(116, 160);
            this.lblHistoricoVazio.Size = new Size(400, 48);
            this.lblHistoricoVazio.TextAlign = ContentAlignment.MiddleCenter;
            this.lblHistoricoVazio.Text = "Nenhum download por aqui ainda.\nOs próximos vão aparecer nesta lista.";
            this.lblHistoricoVazio.Visible = false;

            this.listViewHistorico.BackColor = CorCampo;
            this.listViewHistorico.ForeColor = CorTexto;
            this.listViewHistorico.BorderStyle = BorderStyle.None;
            this.listViewHistorico.Dock = DockStyle.Fill;
            this.listViewHistorico.Font = Fonte(9.75F);
            this.listViewHistorico.FullRowSelect = true;
            this.listViewHistorico.GridLines = false;
            this.listViewHistorico.HeaderStyle = ColumnHeaderStyle.None;
            this.listViewHistorico.HideSelection = false;
            this.listViewHistorico.MultiSelect = false;
            this.listViewHistorico.View = View.Details;
            this.listViewHistorico.UseCompatibleStateImageBehavior = false;
            this.listViewHistorico.ContextMenuStrip = menuHistorico;
            this.listViewHistorico.Columns.AddRange(new ColumnHeader[] { colTipo, colNome, colData });
            this.listViewHistorico.DoubleClick += new System.EventHandler(this.menuItemAbrirPasta_Click);
            this.colTipo.Text = "Tipo"; this.colTipo.Width = 76;
            this.colNome.Text = "Nome"; this.colNome.Width = 380;
            this.colData.Text = "Data"; this.colData.Width = 150;

            this.menuHistorico.Items.Add(this.menuItemAbrirPasta);
            this.menuItemAbrirPasta.Text = "Abrir pasta do item";
            this.menuItemAbrirPasta.Click += new System.EventHandler(this.menuItemAbrirPasta_Click);

            EstiloPerigoContorno(this.btnLimparHistorico);
            this.btnLimparHistorico.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnLimparHistorico.Location = new Point(28, 480);
            this.btnLimparHistorico.Size = new Size(176, 38);
            this.btnLimparHistorico.Text = "Limpar histórico";
            this.btnLimparHistorico.ImageSize = new Size(16, 16);
            this.btnLimparHistorico.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.btnLimparHistorico.ImageOffset = new Point(4, 0);
            this.btnLimparHistorico.Click += new System.EventHandler(this.btnLimparHistorico_Click);

            // ==============================================================
            // PASTA — onde os arquivos serão salvos
            // ==============================================================
            this.pnlPasta.Name = "pnlPasta";
            this.pnlPasta.Visible = false;
            this.pnlPasta.Controls.Add(cardPasta);

            EstiloCard(this.cardPasta);
            this.cardPasta.Anchor = AnchorStyles.None;
            this.cardPasta.Location = new Point(36, 98);
            this.cardPasta.Size = new Size(688, 400);
            this.cardPasta.Name = "cardPasta";
            this.cardPasta.Controls.AddRange(new Control[]
            {
                lblTituloCardPasta, lblSubtituloCardPasta, picIlustracaoPasta,
                lblLocalLabel, txtSavePath, btnAlterarLocal, btnAbrirPasta
            });

            EstiloTitulo(this.lblTituloCardPasta, "Pasta de destino");
            this.lblTituloCardPasta.Location = new Point(26, 22);

            EstiloSubtitulo(this.lblSubtituloCardPasta, "Todos os vídeos e áudios baixados vão para esta pasta.");
            this.lblSubtituloCardPasta.Location = new Point(28, 58);

            this.picIlustracaoPasta.BackColor = Color.Transparent;
            this.picIlustracaoPasta.Location = new Point(296, 100);
            this.picIlustracaoPasta.Size = new Size(96, 96);
            this.picIlustracaoPasta.SizeMode = PictureBoxSizeMode.Zoom;

            EstiloRotulo(this.lblLocalLabel, "Local atual");
            this.lblLocalLabel.Location = new Point(28, 220);

            EstiloTextBox(this.txtSavePath);
            this.txtSavePath.Location = new Point(28, 244);
            this.txtSavePath.Size = new Size(632, 46);
            this.txtSavePath.ReadOnly = true;
            this.txtSavePath.IconLeftSize = new Size(18, 18);
            this.txtSavePath.IconLeftOffset = new Point(6, 0);

            EstiloPrimario(this.btnAlterarLocal);
            this.btnAlterarLocal.Location = new Point(28, 314);
            this.btnAlterarLocal.Size = new Size(200, 48);
            this.btnAlterarLocal.Text = "Alterar pasta";
            this.btnAlterarLocal.ImageSize = new Size(18, 18);
            this.btnAlterarLocal.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.btnAlterarLocal.ImageOffset = new Point(8, 0);
            this.btnAlterarLocal.Click += new System.EventHandler(this.btnAlterarLocal_Click);

            EstiloSecundario(this.btnAbrirPasta);
            this.btnAbrirPasta.Location = new Point(244, 314);
            this.btnAbrirPasta.Size = new Size(200, 48);
            this.btnAbrirPasta.Text = "Abrir no Explorer";
            this.btnAbrirPasta.ImageSize = new Size(18, 18);
            this.btnAbrirPasta.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.btnAbrirPasta.ImageOffset = new Point(8, 0);
            this.btnAbrirPasta.Click += new System.EventHandler(this.btnAbrirPasta_Click);

            // ==============================================================
            // CONFIGURAÇÕES — duas seções lado a lado
            // ==============================================================
            this.pnlConfiguracoes.Name = "pnlConfiguracoes";
            this.pnlConfiguracoes.Visible = false;
            this.pnlConfiguracoes.Controls.Add(cardConfig);

            EstiloCard(this.cardConfig);
            this.cardConfig.Anchor = AnchorStyles.None;
            this.cardConfig.Location = new Point(36, 74);
            this.cardConfig.Size = new Size(688, 448);
            this.cardConfig.Name = "cardConfig";
            this.cardConfig.Controls.AddRange(new Control[]
                { picLogoConfig, lblTituloCardConfig, lblSubtituloCardConfig, secCookies, secPreferencias });

            this.picLogoConfig.BackColor = Color.Transparent;
            this.picLogoConfig.Location = new Point(28, 26);
            this.picLogoConfig.Size = new Size(32, 32);
            this.picLogoConfig.SizeMode = PictureBoxSizeMode.Zoom;

            EstiloTitulo(this.lblTituloCardConfig, "Configurações");
            this.lblTituloCardConfig.Location = new Point(68, 22);

            EstiloSubtitulo(this.lblSubtituloCardConfig, "Login por cookies, playlists e atualização do yt-dlp.");
            this.lblSubtituloCardConfig.Location = new Point(70, 58);

            // --- Seção esquerda: cookies (308 x 324) ---
            EstiloSecao(this.secCookies);
            this.secCookies.Location = new Point(28, 96);
            this.secCookies.Size = new Size(308, 324);
            this.secCookies.Controls.AddRange(new Control[]
            {
                lblUsoCookiesTitulo, btnAjudaCookies,
                radCookiesArquivo, lblOpcaoArquivoCookies, txtCookiesArquivo,
                radCookiesNavegador, lblOpcaoNavegadorCookies, cmbNavegador,
                btnExtrairCookies, lblExtrairCookiesStatus,
                radSemCookies, lblSemCookies
            });

            this.lblUsoCookiesTitulo.AutoSize = true;
            this.lblUsoCookiesTitulo.Font = FonteForte(11F);
            this.lblUsoCookiesTitulo.ForeColor = CorTexto;
            this.lblUsoCookiesTitulo.Location = new Point(18, 16);
            this.lblUsoCookiesTitulo.Text = "Cookies (login)";

            this.btnAjudaCookies.BorderRadius = 12;
            this.btnAjudaCookies.FillColor = CorNavAtivo;
            this.btnAjudaCookies.ForeColor = CorAcentoClaro;
            this.btnAjudaCookies.Font = FonteForte(9.5F);
            this.btnAjudaCookies.HoverState.FillColor = CorAcento;
            this.btnAjudaCookies.HoverState.ForeColor = Color.White;
            this.btnAjudaCookies.Location = new Point(266, 14);
            this.btnAjudaCookies.Size = new Size(24, 24);
            this.btnAjudaCookies.Text = "?";
            this.btnAjudaCookies.Cursor = Cursors.Hand;
            this.btnAjudaCookies.Animated = true;
            this.btnAjudaCookies.Click += new System.EventHandler(this.btnAjudaCookies_Click);

            // Opção 1: arquivo cookies.txt
            EstiloRadio(this.radCookiesArquivo);
            this.radCookiesArquivo.Location = new Point(18, 54);
            this.radCookiesArquivo.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);

            this.lblOpcaoArquivoCookies.AutoSize = true;
            this.lblOpcaoArquivoCookies.Cursor = Cursors.Hand;
            this.lblOpcaoArquivoCookies.Font = Fonte(10F);
            this.lblOpcaoArquivoCookies.ForeColor = CorTexto;
            this.lblOpcaoArquivoCookies.Location = new Point(44, 53);
            this.lblOpcaoArquivoCookies.Text = "Usar um arquivo cookies.txt";

            EstiloTextBox(this.txtCookiesArquivo);
            this.txtCookiesArquivo.Cursor = Cursors.Hand;
            this.txtCookiesArquivo.Enabled = false;
            this.txtCookiesArquivo.Font = Fonte(9.5F);
            this.txtCookiesArquivo.Location = new Point(44, 80);
            this.txtCookiesArquivo.Size = new Size(246, 36);
            this.txtCookiesArquivo.PlaceholderText = "Clique para escolher o arquivo...";
            this.txtCookiesArquivo.ReadOnly = true;
            this.txtCookiesArquivo.Click += new System.EventHandler(this.txtCookiesArquivo_Click);

            // Opção 2: cookies do navegador
            EstiloRadio(this.radCookiesNavegador);
            this.radCookiesNavegador.Location = new Point(18, 134);
            this.radCookiesNavegador.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);

            this.lblOpcaoNavegadorCookies.AutoSize = true;
            this.lblOpcaoNavegadorCookies.Cursor = Cursors.Hand;
            this.lblOpcaoNavegadorCookies.Font = Fonte(10F);
            this.lblOpcaoNavegadorCookies.ForeColor = CorTexto;
            this.lblOpcaoNavegadorCookies.Location = new Point(44, 133);
            this.lblOpcaoNavegadorCookies.Text = "Usar os cookies do navegador";

            this.cmbNavegador.BackColor = Color.Transparent;
            this.cmbNavegador.BorderRadius = 10;
            this.cmbNavegador.BorderColor = CorBorda;
            this.cmbNavegador.Enabled = false;
            this.cmbNavegador.FillColor = CorCard;
            this.cmbNavegador.FocusedColor = CorAcento;
            this.cmbNavegador.FocusedState.BorderColor = CorAcento;
            this.cmbNavegador.ForeColor = CorTexto;
            this.cmbNavegador.Font = Fonte(9.5F);
            this.cmbNavegador.ItemHeight = 30;
            this.cmbNavegador.Items.AddRange(new object[] { "chrome", "edge", "firefox", "brave", "opera", "vivaldi", "chromium" });
            this.cmbNavegador.Location = new Point(44, 160);
            this.cmbNavegador.Size = new Size(246, 36);

            EstiloSecundario(this.btnExtrairCookies);
            this.btnExtrairCookies.BorderRadius = 10;
            this.btnExtrairCookies.Font = FonteForte(9F);
            this.btnExtrairCookies.Location = new Point(44, 204);
            this.btnExtrairCookies.Size = new Size(246, 34);
            this.btnExtrairCookies.Text = "Gerar cookies.txt a partir dele";
            this.btnExtrairCookies.ImageSize = new Size(16, 16);
            this.btnExtrairCookies.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.btnExtrairCookies.ImageOffset = new Point(4, 0);
            this.btnExtrairCookies.Click += new System.EventHandler(this.btnExtrairCookies_Click);

            this.lblExtrairCookiesStatus.AutoSize = false;
            this.lblExtrairCookiesStatus.AutoEllipsis = true;
            this.lblExtrairCookiesStatus.Font = Fonte(8.5F);
            this.lblExtrairCookiesStatus.ForeColor = CorTextoApagado;
            this.lblExtrairCookiesStatus.Location = new Point(44, 242);
            this.lblExtrairCookiesStatus.Size = new Size(246, 18);
            this.lblExtrairCookiesStatus.Text = "Nunca extraído nesta pasta.";

            // Opção 3: sem cookies
            EstiloRadio(this.radSemCookies);
            this.radSemCookies.Checked = true;
            this.radSemCookies.TabStop = true;
            this.radSemCookies.Location = new Point(18, 280);
            this.radSemCookies.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);

            this.lblSemCookies.AutoSize = true;
            this.lblSemCookies.Cursor = Cursors.Hand;
            this.lblSemCookies.Font = Fonte(10F);
            this.lblSemCookies.ForeColor = CorTexto;
            this.lblSemCookies.Location = new Point(44, 279);
            this.lblSemCookies.Text = "Não usar cookies";

            // --- Seção direita: playlists + atualização (308 x 324) ---
            EstiloSecao(this.secPreferencias);
            this.secPreferencias.Location = new Point(352, 96);
            this.secPreferencias.Size = new Size(308, 324);
            this.secPreferencias.Controls.AddRange(new Control[]
            {
                lblPlaylistTitulo, lblPlaylistSubtitulo, chkOrganizarPlaylist,
                lblAtualizarTitulo, lblAtualizarStatus, btnAtualizarYtDlp
            });

            this.lblPlaylistTitulo.AutoSize = true;
            this.lblPlaylistTitulo.Font = FonteForte(11F);
            this.lblPlaylistTitulo.ForeColor = CorTexto;
            this.lblPlaylistTitulo.Location = new Point(18, 16);
            this.lblPlaylistTitulo.Text = "Playlists";

            this.lblPlaylistSubtitulo.AutoSize = false;
            this.lblPlaylistSubtitulo.Font = Fonte(9.25F);
            this.lblPlaylistSubtitulo.ForeColor = CorTextoSecundario;
            this.lblPlaylistSubtitulo.Location = new Point(18, 44);
            this.lblPlaylistSubtitulo.Size = new Size(272, 20);
            this.lblPlaylistSubtitulo.Text = "Como salvar os vídeos de uma playlist";

            EstiloCheck(this.chkOrganizarPlaylist, "Criar uma subpasta por playlist");
            this.chkOrganizarPlaylist.Location = new Point(18, 74);
            this.chkOrganizarPlaylist.Size = new Size(272, 26);

            this.lblAtualizarTitulo.AutoSize = true;
            this.lblAtualizarTitulo.Font = FonteForte(11F);
            this.lblAtualizarTitulo.ForeColor = CorTexto;
            this.lblAtualizarTitulo.Location = new Point(18, 150);
            this.lblAtualizarTitulo.Text = "Atualizar o yt-dlp";

            this.lblAtualizarStatus.AutoSize = false;
            this.lblAtualizarStatus.Font = Fonte(9.25F);
            this.lblAtualizarStatus.ForeColor = CorTextoSecundario;
            this.lblAtualizarStatus.Location = new Point(18, 178);
            this.lblAtualizarStatus.Size = new Size(272, 40);
            this.lblAtualizarStatus.Text = "Se os downloads começarem a falhar, atualizar costuma resolver.";

            EstiloPrimario(this.btnAtualizarYtDlp);
            this.btnAtualizarYtDlp.Location = new Point(18, 262);
            this.btnAtualizarYtDlp.Size = new Size(272, 44);
            this.btnAtualizarYtDlp.Text = "Verificar atualização";
            this.btnAtualizarYtDlp.ImageSize = new Size(18, 18);
            this.btnAtualizarYtDlp.ImageAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.btnAtualizarYtDlp.ImageOffset = new Point(10, 0);
            this.btnAtualizarYtDlp.Click += new System.EventHandler(this.btnAtualizarYtDlp_Click);

            // ==============================================================
            // RODAPÉ — status, progresso e cancelar
            // ==============================================================
            this.pnlStatusBar.BackColor = CorSidebar;
            this.pnlStatusBar.Dock = DockStyle.Bottom;
            this.pnlStatusBar.Size = new Size(760, 64);
            this.pnlStatusBar.Name = "pnlStatusBar";
            this.pnlStatusBar.Controls.AddRange(new Control[]
                { lblStatus, lblProgressPercent, progressBar1, btnCancelar });

            this.lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.lblStatus.AutoSize = false;
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Font = Fonte(9.75F);
            this.lblStatus.ForeColor = CorTexto;
            this.lblStatus.Location = new Point(24, 11);
            this.lblStatus.Size = new Size(580, 22);
            this.lblStatus.Text = "Pronto para uso.";

            this.lblProgressPercent.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.lblProgressPercent.AutoSize = false;
            this.lblProgressPercent.Font = FonteForte(9.75F);
            this.lblProgressPercent.ForeColor = CorAcentoClaro;
            this.lblProgressPercent.Location = new Point(608, 11);
            this.lblProgressPercent.Size = new Size(76, 22);
            this.lblProgressPercent.TextAlign = ContentAlignment.MiddleRight;
            this.lblProgressPercent.Text = "0%";

            this.progressBar1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.progressBar1.BorderRadius = 3;
            this.progressBar1.FillColor = CorCampo;
            this.progressBar1.ProgressColor = CorAcento;
            this.progressBar1.ProgressColor2 = CorAcento2;
            this.progressBar1.Location = new Point(24, 40);
            this.progressBar1.Size = new Size(660, 6);

            // Botão cancelar: círculo vermelho no canto direito do rodapé.
            this.btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnCancelar.BorderRadius = 18;
            this.btnCancelar.Enabled = false;
            this.btnCancelar.FillColor = CorPerigo;
            this.btnCancelar.ForeColor = Color.White;
            this.btnCancelar.Font = FonteForte(14F);
            this.btnCancelar.HoverState.FillColor = CorPerigoHover;
            this.btnCancelar.DisabledState.FillColor = CorCampo;
            this.btnCancelar.DisabledState.ForeColor = CorTextoDesabilitado;
            this.btnCancelar.DisabledState.BorderColor = CorCampo;
            this.btnCancelar.Location = new Point(700, 14);
            this.btnCancelar.Size = new Size(36, 36);
            this.btnCancelar.Text = "×";
            this.btnCancelar.Cursor = Cursors.Hand;
            this.btnCancelar.Animated = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            // ==============================================================
            // NOTIFICAÇÕES
            // ==============================================================
            this.notifyIcon1.Text = "YT-DLP GUI";
            this.notifyIcon1.Visible = false;

            // ==============================================================
            // JANELA
            // ==============================================================
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = CorFundo;
            this.ForeColor = CorTexto;
            this.ClientSize = new Size(980, 660);
            this.MinimumSize = new Size(996, 699);
            this.Font = Fonte(9F);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "YT-DLP GUI — Downloader de vídeos";
            this.Name = "MainForm";
            // Ordem importa para o Dock: o último adicionado (barra lateral) é
            // encaixado primeiro e ocupa a altura toda; o rodapé fica só embaixo
            // da coluna de conteúdo; e o conteúdo preenche o que sobrar.
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlStatusBar);
            this.Controls.Add(pnlSidebar);

            foreach (var pb in new PictureBox[]
                { picLogo, picLogoGrande, picIlustracaoPasta, picLogoConfig, picThumb })
                ((System.ComponentModel.ISupportInitialize)pb).EndInit();
            this.menuHistorico.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}

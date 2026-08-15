using Guna.UI2.WinForms;

namespace YtDlpGui
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Paleta

        private static readonly System.Drawing.Color CorFundo        = System.Drawing.Color.FromArgb(92, 127, 114);
        private static readonly System.Drawing.Color CorStatusBar     = System.Drawing.Color.FromArgb(78, 110, 98);
        private static readonly System.Drawing.Color CorSidebar       = System.Drawing.Color.FromArgb(143, 206, 187);
        private static readonly System.Drawing.Color CorCard          = System.Drawing.Color.White;
        private static readonly System.Drawing.Color CorTextoClaro    = System.Drawing.Color.White;
        private static readonly System.Drawing.Color CorTextoEscuro   = System.Drawing.Color.FromArgb(31, 41, 55);
        private static readonly System.Drawing.Color CorTextoMuted    = System.Drawing.Color.FromArgb(107, 114, 128);
        private static readonly System.Drawing.Color CorCampo         = System.Drawing.Color.FromArgb(240, 242, 240);
        private static readonly System.Drawing.Color CorAcento        = System.Drawing.Color.FromArgb(47, 125, 232);
        private static readonly System.Drawing.Color CorAcentoHover   = System.Drawing.Color.FromArgb(37, 107, 204);
        private static readonly System.Drawing.Color CorPerigo        = System.Drawing.Color.FromArgb(225, 92, 79);
        private static readonly System.Drawing.Color CorPerigoHover   = System.Drawing.Color.FromArgb(200, 72, 60);
        private static readonly System.Drawing.Color CorBotaoDisabled = System.Drawing.Color.FromArgb(210, 215, 212);
        private static readonly System.Drawing.Color CorTextoDisabled = System.Drawing.Color.FromArgb(150, 158, 154);

        #endregion

        #region Helpers de estilo

        private void EstiloTextBox(Guna2TextBox c, int radius = 18)
        {
            c.BorderRadius = radius;
            c.BorderColor = System.Drawing.Color.Transparent;
            c.FillColor = CorCampo;
            c.ForeColor = CorTextoEscuro;
            c.Font = new System.Drawing.Font("Consolas", 10F);
            c.FocusedState.BorderColor = CorAcento;
            c.PlaceholderForeColor = System.Drawing.Color.FromArgb(150, 150, 150);
        }

        private void EstiloPill(Guna2Button c)
        {
            c.BorderRadius = 30;
            c.FillColor = CorSidebar;
            c.ForeColor = CorTextoEscuro;
            c.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold);
            c.HoverState.FillColor = System.Drawing.Color.FromArgb(120, 190, 168);
            c.DisabledState.FillColor = CorBotaoDisabled;
            c.DisabledState.ForeColor = CorTextoDisabled;
            c.Animated = true;
        }

        private void EstiloSecundario(Guna2Button c)
        {
            c.BorderRadius = 16;
            c.FillColor = CorSidebar;
            c.ForeColor = CorTextoEscuro;
            c.Font = new System.Drawing.Font("Consolas", 9F);
            c.HoverState.FillColor = System.Drawing.Color.FromArgb(120, 190, 168);
            c.DisabledState.FillColor = CorBotaoDisabled;
            c.DisabledState.ForeColor = CorTextoDisabled;
            c.Animated = true;
        }

        #endregion

        // ---- Estrutura ----
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.PictureBox picNavHistorico;
        private System.Windows.Forms.PictureBox picNavPasta;
        private System.Windows.Forms.PictureBox picNavConfig;

        private System.Windows.Forms.Panel pnlContent;
        private Guna2Button btnCancelar;

        private System.Windows.Forms.Panel pnlStatusBar;
        private System.Windows.Forms.Label lblStatus;
        private Guna2ProgressBar progressBar1;
        private System.Windows.Forms.Label lblProgressPercent;
        private System.Windows.Forms.NotifyIcon notifyIcon1;

        // ---- Home ----
        private System.Windows.Forms.Panel pnlHome;
        private Guna2Panel cardHome;
        private System.Windows.Forms.Label lblTituloCardHome;
        private System.Windows.Forms.Label lblSubtituloCardHome;
        private System.Windows.Forms.Label lblUrlLabel;
        private Guna2TextBox txtUrl;
        // Área de ilustração / prévia
        private System.Windows.Forms.PictureBox picLogoGrande;
        private System.Windows.Forms.Label lblSpinner;
        private System.Windows.Forms.Panel pnlPrevia;
        private System.Windows.Forms.PictureBox picThumb;
        private System.Windows.Forms.Label lblTituloVideo;
        private System.Windows.Forms.Label lblInfoVideo;
        // Botões e toggle
        private Guna2Button btnDownloadMp4;
        private Guna2Button btnDownloadMp3;
        private Guna2CheckBox chkPlaylist;

        // ---- Histórico ----
        private System.Windows.Forms.Panel pnlHistorico;
        private Guna2Panel cardHistorico;
        private System.Windows.Forms.Label lblTituloCardHistorico;
        private Guna2Panel pnlCardListaHistorico;
        private System.Windows.Forms.ListView listViewHistorico;
        private System.Windows.Forms.ColumnHeader colTipo;
        private System.Windows.Forms.ColumnHeader colNome;
        private System.Windows.Forms.ColumnHeader colData;
        private System.Windows.Forms.ContextMenuStrip menuHistorico;
        private System.Windows.Forms.ToolStripMenuItem menuItemAbrirPasta;
        private Guna2Button btnLimparHistorico;
        private System.Windows.Forms.Label lblHistoricoVazio;

        // ---- Pasta ----
        private System.Windows.Forms.Panel pnlPasta;
        private Guna2Panel cardPasta;
        private System.Windows.Forms.Label lblTituloCardPasta;
        private System.Windows.Forms.PictureBox picIlustracaoPasta;
        private System.Windows.Forms.Label lblLocalLabel;
        private Guna2TextBox txtSavePath;
        private Guna2Button btnAlterarLocal;

        // ---- Configurações ----
        private System.Windows.Forms.Panel pnlConfiguracoes;
        private Guna2Panel cardConfig;
        private System.Windows.Forms.Label lblTituloCardConfig;
        private System.Windows.Forms.PictureBox picLogoConfig;
        // Coluna esquerda (cookies)
        private System.Windows.Forms.Label lblUsoCookiesTitulo;
        private Guna2Button btnAjudaCookies;
        private System.Windows.Forms.Label lblOpcaoArquivoCookies;
        private Guna2RadioButton radCookiesArquivo;
        private Guna2TextBox txtCookiesArquivo;
        private System.Windows.Forms.Label lblOpcaoNavegadorCookies;
        private Guna2RadioButton radCookiesNavegador;
        private Guna2ComboBox cmbNavegador;
        private Guna2RadioButton radSemCookies;
        private System.Windows.Forms.Label lblSemCookies;
        // Coluna direita (playlist + atualização)
        private System.Windows.Forms.Label lblPlaylistTitulo;
        private System.Windows.Forms.Label lblPlaylistSubtitulo;
        private Guna2CheckBox chkOrganizarPlaylist;
        private System.Windows.Forms.Label lblAtualizarTitulo;
        private System.Windows.Forms.Label lblAtualizarStatus;
        private Guna2Button btnAtualizarYtDlp;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // ---- instanciar todos ----
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.picNavHistorico = new System.Windows.Forms.PictureBox();
            this.picNavPasta = new System.Windows.Forms.PictureBox();
            this.picNavConfig = new System.Windows.Forms.PictureBox();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.btnCancelar = new Guna2Button();
            this.pnlStatusBar = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.progressBar1 = new Guna2ProgressBar();
            this.lblProgressPercent = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);

            this.pnlHome = new System.Windows.Forms.Panel();
            this.cardHome = new Guna2Panel();
            this.lblTituloCardHome = new System.Windows.Forms.Label();
            this.lblSubtituloCardHome = new System.Windows.Forms.Label();
            this.lblUrlLabel = new System.Windows.Forms.Label();
            this.txtUrl = new Guna2TextBox();
            this.picLogoGrande = new System.Windows.Forms.PictureBox();
            this.lblSpinner = new System.Windows.Forms.Label();
            this.pnlPrevia = new System.Windows.Forms.Panel();
            this.picThumb = new System.Windows.Forms.PictureBox();
            this.lblTituloVideo = new System.Windows.Forms.Label();
            this.lblInfoVideo = new System.Windows.Forms.Label();
            this.btnDownloadMp4 = new Guna2Button();
            this.btnDownloadMp3 = new Guna2Button();
            this.chkPlaylist = new Guna2CheckBox();

            this.pnlHistorico = new System.Windows.Forms.Panel();
            this.cardHistorico = new Guna2Panel();
            this.lblTituloCardHistorico = new System.Windows.Forms.Label();
            this.pnlCardListaHistorico = new Guna2Panel();
            this.listViewHistorico = new System.Windows.Forms.ListView();
            this.colTipo = new System.Windows.Forms.ColumnHeader();
            this.colNome = new System.Windows.Forms.ColumnHeader();
            this.colData = new System.Windows.Forms.ColumnHeader();
            this.menuHistorico = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemAbrirPasta = new System.Windows.Forms.ToolStripMenuItem();
            this.btnLimparHistorico = new Guna2Button();
            this.lblHistoricoVazio = new System.Windows.Forms.Label();

            this.pnlPasta = new System.Windows.Forms.Panel();
            this.cardPasta = new Guna2Panel();
            this.lblTituloCardPasta = new System.Windows.Forms.Label();
            this.picIlustracaoPasta = new System.Windows.Forms.PictureBox();
            this.lblLocalLabel = new System.Windows.Forms.Label();
            this.txtSavePath = new Guna2TextBox();
            this.btnAlterarLocal = new Guna2Button();

            this.pnlConfiguracoes = new System.Windows.Forms.Panel();
            this.cardConfig = new Guna2Panel();
            this.lblTituloCardConfig = new System.Windows.Forms.Label();
            this.picLogoConfig = new System.Windows.Forms.PictureBox();
            this.lblUsoCookiesTitulo = new System.Windows.Forms.Label();
            this.btnAjudaCookies = new Guna2Button();
            this.lblOpcaoArquivoCookies = new System.Windows.Forms.Label();
            this.radCookiesArquivo = new Guna2RadioButton();
            this.txtCookiesArquivo = new Guna2TextBox();
            this.lblOpcaoNavegadorCookies = new System.Windows.Forms.Label();
            this.radCookiesNavegador = new Guna2RadioButton();
            this.cmbNavegador = new Guna2ComboBox();
            this.radSemCookies = new Guna2RadioButton();
            this.lblSemCookies = new System.Windows.Forms.Label();
            this.lblPlaylistTitulo = new System.Windows.Forms.Label();
            this.lblPlaylistSubtitulo = new System.Windows.Forms.Label();
            this.chkOrganizarPlaylist = new Guna2CheckBox();
            this.lblAtualizarTitulo = new System.Windows.Forms.Label();
            this.lblAtualizarStatus = new System.Windows.Forms.Label();
            this.btnAtualizarYtDlp = new Guna2Button();

            foreach (var pb in new System.Windows.Forms.PictureBox[]
                { picLogo, picNavHistorico, picNavPasta, picNavConfig,
                  picLogoGrande, picIlustracaoPasta, picLogoConfig, picThumb })
                ((System.ComponentModel.ISupportInitialize)pb).BeginInit();
            this.menuHistorico.SuspendLayout();
            this.SuspendLayout();

            // ==============================================================
            // SIDEBAR
            // ==============================================================
            this.pnlSidebar.BackColor = CorSidebar;
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Width = 90;
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Controls.AddRange(new System.Windows.Forms.Control[]
                { picLogo, picNavHistorico, picNavPasta, picNavConfig });

            this.picLogo.BackColor = System.Drawing.Color.Transparent;
            this.picLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picLogo.Location = new System.Drawing.Point(13, 13);
            this.picLogo.Size = new System.Drawing.Size(64, 64);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.Click += new System.EventHandler(this.picLogo_Click);

            foreach (var (pb, y, name) in new[]
            {
                (picNavHistorico, 448, "picNavHistorico"),
                (picNavPasta,     512, "picNavPasta"),
                (picNavConfig,    576, "picNavConfig"),
            })
            {
                pb.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
                pb.BackColor = System.Drawing.Color.Transparent;
                pb.Cursor = System.Windows.Forms.Cursors.Hand;
                pb.Location = new System.Drawing.Point(21, y);
                pb.Size = new System.Drawing.Size(48, 48);
                pb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
                pb.Name = name;
            }
            this.picNavHistorico.Click += new System.EventHandler(this.picNavHistorico_Click);
            this.picNavPasta.Click += new System.EventHandler(this.picNavPasta_Click);
            this.picNavConfig.Click += new System.EventHandler(this.picNavConfig_Click);

            // ==============================================================
            // CONTENT AREA
            // ==============================================================
            this.pnlContent.Location = new System.Drawing.Point(90, 0);
            this.pnlContent.Size = new System.Drawing.Size(730, 550);
            this.pnlContent.Anchor = ((System.Windows.Forms.AnchorStyles)
                (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom |
                 System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.pnlContent.BackColor = CorFundo;
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Controls.AddRange(new System.Windows.Forms.Control[]
                { pnlConfiguracoes, pnlPasta, pnlHistorico, pnlHome, btnCancelar });

            // Botão cancelar — círculo vermelho flutuante, sempre na frente
            this.btnCancelar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.btnCancelar.BorderRadius = 28;
            this.btnCancelar.Enabled = false;
            this.btnCancelar.FillColor = CorPerigo;
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Font = new System.Drawing.Font("Consolas", 14F, System.Drawing.FontStyle.Bold);
            this.btnCancelar.HoverState.FillColor = CorPerigoHover;
            this.btnCancelar.DisabledState.FillColor = System.Drawing.Color.FromArgb(190, 150, 145);
            this.btnCancelar.DisabledState.ForeColor = System.Drawing.Color.FromArgb(230, 210, 208);
            this.btnCancelar.Location = new System.Drawing.Point(654, 474);
            this.btnCancelar.Size = new System.Drawing.Size(56, 56);
            this.btnCancelar.Text = "X";
            this.btnCancelar.Animated = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            this.btnCancelar.BringToFront();

            // ==============================================================
            // HOME — card centralizado com prévia de vídeo
            // ==============================================================
            this.pnlHome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHome.BackColor = System.Drawing.Color.Transparent;
            this.pnlHome.Name = "pnlHome";
            this.pnlHome.Controls.AddRange(new System.Windows.Forms.Control[] { cardHome });

            this.cardHome.Anchor = ((System.Windows.Forms.AnchorStyles)
                (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom |
                 System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.cardHome.BorderRadius = 24;
            this.cardHome.FillColor = CorCard;
            this.cardHome.Location = new System.Drawing.Point(30, 30);
            this.cardHome.Size = new System.Drawing.Size(670, 490);
            this.cardHome.Name = "cardHome";
            this.cardHome.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblTituloCardHome, lblSubtituloCardHome,
                lblUrlLabel, txtUrl,
                picLogoGrande, lblSpinner, pnlPrevia,
                btnDownloadMp4, btnDownloadMp3, chkPlaylist
            });

            this.lblTituloCardHome.AutoSize = true;
            this.lblTituloCardHome.Font = new System.Drawing.Font("Consolas", 18F, System.Drawing.FontStyle.Bold);
            this.lblTituloCardHome.ForeColor = CorTextoEscuro;
            this.lblTituloCardHome.Location = new System.Drawing.Point(30, 26);
            this.lblTituloCardHome.Text = "Baixar vídeo";

            this.lblSubtituloCardHome.AutoSize = true;
            this.lblSubtituloCardHome.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblSubtituloCardHome.ForeColor = CorTextoMuted;
            this.lblSubtituloCardHome.Location = new System.Drawing.Point(30, 60);
            this.lblSubtituloCardHome.Text = "Cole o link e escolha o formato";

            this.lblUrlLabel.AutoSize = true;
            this.lblUrlLabel.Font = new System.Drawing.Font("Consolas", 11F);
            this.lblUrlLabel.ForeColor = CorTextoEscuro;
            this.lblUrlLabel.Location = new System.Drawing.Point(30, 98);
            this.lblUrlLabel.Text = "URL do vídeo:";

            this.txtUrl.Anchor = ((System.Windows.Forms.AnchorStyles)
                (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left |
                 System.Windows.Forms.AnchorStyles.Right));
            this.txtUrl.Location = new System.Drawing.Point(30, 124);
            this.txtUrl.Size = new System.Drawing.Size(610, 40);
            this.txtUrl.PlaceholderText = "https://youtube.com/watch?v=...";
            this.EstiloTextBox(this.txtUrl, 20);
            this.txtUrl.TextChanged += new System.EventHandler(this.txtUrl_TextChanged);

            // Logo padrão (visível quando não há URL)
            this.picLogoGrande.Location = new System.Drawing.Point(245, 178);
            this.picLogoGrande.Size = new System.Drawing.Size(160, 160);
            this.picLogoGrande.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogoGrande.BackColor = System.Drawing.Color.Transparent;

            // Spinner de loading
            this.lblSpinner.AutoSize = true;
            this.lblSpinner.Font = new System.Drawing.Font("Consolas", 13F);
            this.lblSpinner.ForeColor = CorTextoMuted;
            this.lblSpinner.Location = new System.Drawing.Point(140, 234);
            this.lblSpinner.Text = "⠋ Buscando informações...";
            this.lblSpinner.Visible = false;

            // Painel de prévia (thumbnail + título + info)
            this.pnlPrevia.Location = new System.Drawing.Point(30, 178);
            this.pnlPrevia.Size = new System.Drawing.Size(610, 150);
            this.pnlPrevia.BackColor = System.Drawing.Color.FromArgb(240, 243, 241);
            var pnlPreviaRound = this.pnlPrevia;
            pnlPreviaRound.Visible = false;
            this.pnlPrevia.Controls.AddRange(new System.Windows.Forms.Control[]
                { picThumb, lblTituloVideo, lblInfoVideo });

            this.picThumb.BackColor = System.Drawing.Color.FromArgb(200, 206, 203);
            this.picThumb.Location = new System.Drawing.Point(12, 12);
            this.picThumb.Size = new System.Drawing.Size(180, 126);
            this.picThumb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            this.lblTituloVideo.AutoSize = false;
            this.lblTituloVideo.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Bold);
            this.lblTituloVideo.ForeColor = CorTextoEscuro;
            this.lblTituloVideo.Location = new System.Drawing.Point(205, 20);
            this.lblTituloVideo.Size = new System.Drawing.Size(395, 60);
            this.lblTituloVideo.Text = "";

            this.lblInfoVideo.AutoSize = true;
            this.lblInfoVideo.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblInfoVideo.ForeColor = CorTextoMuted;
            this.lblInfoVideo.Location = new System.Drawing.Point(205, 88);
            this.lblInfoVideo.Text = "";

            // Botões de download
            this.btnDownloadMp4.Location = new System.Drawing.Point(30, 350);
            this.btnDownloadMp4.Size = new System.Drawing.Size(290, 60);
            this.btnDownloadMp4.Text = "Baixar .MP4";
            this.EstiloPill(this.btnDownloadMp4);
            this.btnDownloadMp4.Click += new System.EventHandler(this.btnDownloadMp4_Click);

            this.btnDownloadMp3.Location = new System.Drawing.Point(348, 350);
            this.btnDownloadMp3.Size = new System.Drawing.Size(290, 60);
            this.btnDownloadMp3.Text = "Baixar .MP3";
            this.EstiloPill(this.btnDownloadMp3);
            this.btnDownloadMp3.Click += new System.EventHandler(this.btnDownloadMp3_Click);

            this.chkPlaylist.AutoSize = true;
            this.chkPlaylist.CheckedState.BorderColor = CorAcento;
            this.chkPlaylist.CheckedState.FillColor = CorAcento;
            this.chkPlaylist.Font = new System.Drawing.Font("Consolas", 11F);
            this.chkPlaylist.ForeColor = CorTextoEscuro;
            this.chkPlaylist.Location = new System.Drawing.Point(30, 430);
            this.chkPlaylist.Text = "Baixar Playlist";

            // ==============================================================
            // HISTÓRICO — card com lista
            // ==============================================================
            this.pnlHistorico.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHistorico.BackColor = System.Drawing.Color.Transparent;
            this.pnlHistorico.Name = "pnlHistorico";
            this.pnlHistorico.Visible = false;
            this.pnlHistorico.Controls.Add(cardHistorico);

            this.cardHistorico.Anchor = ((System.Windows.Forms.AnchorStyles)
                (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom |
                 System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.cardHistorico.BorderRadius = 24;
            this.cardHistorico.FillColor = CorCard;
            this.cardHistorico.Location = new System.Drawing.Point(30, 30);
            this.cardHistorico.Size = new System.Drawing.Size(670, 490);
            this.cardHistorico.Name = "cardHistorico";
            this.cardHistorico.Controls.AddRange(new System.Windows.Forms.Control[]
                { lblTituloCardHistorico, pnlCardListaHistorico, btnLimparHistorico, lblHistoricoVazio });

            this.lblTituloCardHistorico.AutoSize = true;
            this.lblTituloCardHistorico.Font = new System.Drawing.Font("Consolas", 18F, System.Drawing.FontStyle.Bold);
            this.lblTituloCardHistorico.ForeColor = CorTextoEscuro;
            this.lblTituloCardHistorico.Location = new System.Drawing.Point(30, 26);
            this.lblTituloCardHistorico.Text = "Histórico de Downloads";

            this.pnlCardListaHistorico.Anchor = ((System.Windows.Forms.AnchorStyles)
                (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom |
                 System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.pnlCardListaHistorico.BorderRadius = 16;
            this.pnlCardListaHistorico.FillColor = System.Drawing.Color.FromArgb(248, 250, 248);
            this.pnlCardListaHistorico.Location = new System.Drawing.Point(30, 80);
            this.pnlCardListaHistorico.Size = new System.Drawing.Size(610, 360);
            this.pnlCardListaHistorico.Controls.Add(listViewHistorico);

            this.listViewHistorico.BackColor = System.Drawing.Color.FromArgb(248, 250, 248);
            this.listViewHistorico.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewHistorico.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewHistorico.Font = new System.Drawing.Font("Consolas", 10F);
            this.listViewHistorico.ForeColor = CorTextoEscuro;
            this.listViewHistorico.FullRowSelect = true;
            this.listViewHistorico.GridLines = false;
            this.listViewHistorico.HideSelection = false;
            this.listViewHistorico.MultiSelect = false;
            this.listViewHistorico.View = System.Windows.Forms.View.Details;
            this.listViewHistorico.UseCompatibleStateImageBehavior = false;
            this.listViewHistorico.ContextMenuStrip = menuHistorico;
            this.listViewHistorico.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
                { colTipo, colNome, colData });
            this.listViewHistorico.DoubleClick += new System.EventHandler(this.menuItemAbrirPasta_Click);
            this.colTipo.Text = "Tipo"; this.colTipo.Width = 70;
            this.colNome.Text = "Nome"; this.colNome.Width = 350;
            this.colData.Text = "Data";  this.colData.Width = 160;

            this.menuHistorico.Items.Add(this.menuItemAbrirPasta);
            this.menuItemAbrirPasta.Text = "Abrir pasta do item";
            this.menuItemAbrirPasta.Click += new System.EventHandler(this.menuItemAbrirPasta_Click);

            this.lblHistoricoVazio.AutoSize = true;
            this.lblHistoricoVazio.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblHistoricoVazio.ForeColor = CorTextoMuted;
            this.lblHistoricoVazio.Location = new System.Drawing.Point(50, 106);
            this.lblHistoricoVazio.Text = "Nenhum download foi registrado ainda.";
            this.lblHistoricoVazio.Visible = false;

            this.btnLimparHistorico.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.btnLimparHistorico.Location = new System.Drawing.Point(30, 452);
            this.btnLimparHistorico.Size = new System.Drawing.Size(160, 36);
            this.btnLimparHistorico.Text = "Limpar Histórico";
            this.EstiloSecundario(this.btnLimparHistorico);
            this.btnLimparHistorico.Click += new System.EventHandler(this.btnLimparHistorico_Click);

            // ==============================================================
            // PASTA — card com campo de salvamento
            // ==============================================================
            this.pnlPasta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPasta.BackColor = System.Drawing.Color.Transparent;
            this.pnlPasta.Name = "pnlPasta";
            this.pnlPasta.Visible = false;
            this.pnlPasta.Controls.Add(cardPasta);

            this.cardPasta.Anchor = ((System.Windows.Forms.AnchorStyles)
                (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom |
                 System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.cardPasta.BorderRadius = 24;
            this.cardPasta.FillColor = CorCard;
            this.cardPasta.Location = new System.Drawing.Point(30, 30);
            this.cardPasta.Size = new System.Drawing.Size(670, 490);
            this.cardPasta.Name = "cardPasta";
            this.cardPasta.Controls.AddRange(new System.Windows.Forms.Control[]
                { lblTituloCardPasta, picIlustracaoPasta, lblLocalLabel, txtSavePath, btnAlterarLocal });

            this.lblTituloCardPasta.AutoSize = true;
            this.lblTituloCardPasta.Font = new System.Drawing.Font("Consolas", 18F, System.Drawing.FontStyle.Bold);
            this.lblTituloCardPasta.ForeColor = CorTextoEscuro;
            this.lblTituloCardPasta.Location = new System.Drawing.Point(30, 26);
            this.lblTituloCardPasta.Text = "Local de salvamento";

            this.picIlustracaoPasta.Location = new System.Drawing.Point(240, 90);
            this.picIlustracaoPasta.Size = new System.Drawing.Size(190, 190);
            this.picIlustracaoPasta.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picIlustracaoPasta.BackColor = System.Drawing.Color.Transparent;

            this.lblLocalLabel.AutoSize = true;
            this.lblLocalLabel.Font = new System.Drawing.Font("Consolas", 11F);
            this.lblLocalLabel.ForeColor = CorTextoEscuro;
            this.lblLocalLabel.Location = new System.Drawing.Point(30, 300);
            this.lblLocalLabel.Text = "Local de Salvamento:";

            this.txtSavePath.Anchor = ((System.Windows.Forms.AnchorStyles)
                (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left |
                 System.Windows.Forms.AnchorStyles.Right));
            this.txtSavePath.Location = new System.Drawing.Point(30, 328);
            this.txtSavePath.Size = new System.Drawing.Size(610, 40);
            this.txtSavePath.ReadOnly = true;
            this.EstiloTextBox(this.txtSavePath, 20);

            this.btnAlterarLocal.Location = new System.Drawing.Point(190, 390);
            this.btnAlterarLocal.Size = new System.Drawing.Size(290, 60);
            this.btnAlterarLocal.Text = "Alterar Local";
            this.EstiloPill(this.btnAlterarLocal);
            this.btnAlterarLocal.Click += new System.EventHandler(this.btnAlterarLocal_Click);

            // ==============================================================
            // CONFIGURAÇÕES — card 2 colunas
            // ==============================================================
            this.pnlConfiguracoes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlConfiguracoes.BackColor = System.Drawing.Color.Transparent;
            this.pnlConfiguracoes.Name = "pnlConfiguracoes";
            this.pnlConfiguracoes.Visible = false;
            this.pnlConfiguracoes.Controls.Add(cardConfig);

            this.cardConfig.Anchor = ((System.Windows.Forms.AnchorStyles)
                (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom |
                 System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right));
            this.cardConfig.BorderRadius = 24;
            this.cardConfig.FillColor = CorCard;
            this.cardConfig.Location = new System.Drawing.Point(30, 30);
            this.cardConfig.Size = new System.Drawing.Size(670, 490);
            this.cardConfig.Name = "cardConfig";
            this.cardConfig.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblTituloCardConfig, picLogoConfig,
                // col esq
                lblUsoCookiesTitulo, btnAjudaCookies,
                lblOpcaoArquivoCookies, radCookiesArquivo, txtCookiesArquivo,
                lblOpcaoNavegadorCookies, radCookiesNavegador, cmbNavegador,
                radSemCookies, lblSemCookies,
                // col dir
                lblPlaylistTitulo, lblPlaylistSubtitulo, chkOrganizarPlaylist,
                lblAtualizarTitulo, lblAtualizarStatus, btnAtualizarYtDlp
            });

            this.lblTituloCardConfig.AutoSize = true;
            this.lblTituloCardConfig.Font = new System.Drawing.Font("Consolas", 18F, System.Drawing.FontStyle.Bold);
            this.lblTituloCardConfig.ForeColor = CorTextoEscuro;
            this.lblTituloCardConfig.Location = new System.Drawing.Point(30, 26);
            this.lblTituloCardConfig.Text = "Configurações";

            this.picLogoConfig.Location = new System.Drawing.Point(250, 60);
            this.picLogoConfig.Size = new System.Drawing.Size(120, 120);
            this.picLogoConfig.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogoConfig.BackColor = System.Drawing.Color.Transparent;

            // Coluna esquerda (cookies) — x = 30
            int cx = 30, cy = 200;
            this.lblUsoCookiesTitulo.AutoSize = true;
            this.lblUsoCookiesTitulo.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold);
            this.lblUsoCookiesTitulo.ForeColor = CorTextoEscuro;
            this.lblUsoCookiesTitulo.Location = new System.Drawing.Point(cx, cy);
            this.lblUsoCookiesTitulo.Text = "Uso de Cookies:";

            this.btnAjudaCookies.BorderRadius = 12;
            this.btnAjudaCookies.FillColor = CorAcento;
            this.btnAjudaCookies.ForeColor = System.Drawing.Color.White;
            this.btnAjudaCookies.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            this.btnAjudaCookies.HoverState.FillColor = CorAcentoHover;
            this.btnAjudaCookies.Location = new System.Drawing.Point(cx + 196, cy + 2);
            this.btnAjudaCookies.Size = new System.Drawing.Size(26, 26);
            this.btnAjudaCookies.Text = "?";
            this.btnAjudaCookies.Animated = true;
            this.btnAjudaCookies.Click += new System.EventHandler(this.btnAjudaCookies_Click);

            cy += 34;
            this.lblOpcaoArquivoCookies.AutoSize = true;
            this.lblOpcaoArquivoCookies.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblOpcaoArquivoCookies.ForeColor = CorTextoMuted;
            this.lblOpcaoArquivoCookies.Location = new System.Drawing.Point(cx, cy);
            this.lblOpcaoArquivoCookies.Text = "Usar arquivo cookies.txt";

            cy += 24;
            this.radCookiesArquivo.CheckedState.BorderColor = CorAcento;
            this.radCookiesArquivo.CheckedState.FillColor = CorAcento;
            this.radCookiesArquivo.Location = new System.Drawing.Point(cx, cy);
            this.radCookiesArquivo.Size = new System.Drawing.Size(30, 30);
            this.radCookiesArquivo.Text = "";
            this.radCookiesArquivo.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);

            this.txtCookiesArquivo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.txtCookiesArquivo.Enabled = false;
            this.txtCookiesArquivo.Location = new System.Drawing.Point(cx + 40, cy);
            this.txtCookiesArquivo.Size = new System.Drawing.Size(255, 32);
            this.txtCookiesArquivo.PlaceholderText = "Toque para selecionar...";
            this.txtCookiesArquivo.ReadOnly = true;
            this.EstiloTextBox(this.txtCookiesArquivo, 16);
            this.txtCookiesArquivo.Click += new System.EventHandler(this.txtCookiesArquivo_Click);

            cy += 44;
            this.lblOpcaoNavegadorCookies.AutoSize = true;
            this.lblOpcaoNavegadorCookies.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblOpcaoNavegadorCookies.ForeColor = CorTextoMuted;
            this.lblOpcaoNavegadorCookies.Location = new System.Drawing.Point(cx, cy);
            this.lblOpcaoNavegadorCookies.Text = "Usar cookies do navegador";

            cy += 24;
            this.radCookiesNavegador.CheckedState.BorderColor = CorAcento;
            this.radCookiesNavegador.CheckedState.FillColor = CorAcento;
            this.radCookiesNavegador.Location = new System.Drawing.Point(cx, cy);
            this.radCookiesNavegador.Size = new System.Drawing.Size(30, 30);
            this.radCookiesNavegador.Text = "";
            this.radCookiesNavegador.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);

            this.cmbNavegador.BackColor = System.Drawing.Color.Transparent;
            this.cmbNavegador.BorderRadius = 16;
            this.cmbNavegador.BorderColor = System.Drawing.Color.Transparent;
            this.cmbNavegador.Enabled = false;
            this.cmbNavegador.FillColor = CorCampo;
            this.cmbNavegador.ForeColor = CorTextoEscuro;
            this.cmbNavegador.Font = new System.Drawing.Font("Consolas", 10F);
            this.cmbNavegador.Items.AddRange(new object[] { "chrome","edge","firefox","brave","opera","vivaldi","chromium" });
            this.cmbNavegador.Location = new System.Drawing.Point(cx + 40, cy);
            this.cmbNavegador.Size = new System.Drawing.Size(255, 32);

            cy += 44;
            this.radSemCookies.Checked = true;
            this.radSemCookies.CheckedState.BorderColor = CorAcento;
            this.radSemCookies.CheckedState.FillColor = CorAcento;
            this.radSemCookies.Location = new System.Drawing.Point(cx, cy);
            this.radSemCookies.Size = new System.Drawing.Size(30, 30);
            this.radSemCookies.TabStop = true;
            this.radSemCookies.Text = "";
            this.radSemCookies.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);

            this.lblSemCookies.AutoSize = true;
            this.lblSemCookies.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblSemCookies.ForeColor = CorTextoEscuro;
            this.lblSemCookies.Location = new System.Drawing.Point(cx + 40, cy + 8);
            this.lblSemCookies.Text = "Não usar cookies";

            // Coluna direita (playlist + atualização) — x = 360
            int dx = 360; int dy = 200;
            this.lblPlaylistTitulo.AutoSize = true;
            this.lblPlaylistTitulo.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold);
            this.lblPlaylistTitulo.ForeColor = CorTextoEscuro;
            this.lblPlaylistTitulo.Location = new System.Drawing.Point(dx, dy);
            this.lblPlaylistTitulo.Text = "Download de Playlists:";

            dy += 30;
            this.lblPlaylistSubtitulo.AutoSize = true;
            this.lblPlaylistSubtitulo.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblPlaylistSubtitulo.ForeColor = CorTextoMuted;
            this.lblPlaylistSubtitulo.Location = new System.Drawing.Point(dx, dy);
            this.lblPlaylistSubtitulo.Text = "Escolha como salvá-las";

            dy += 28;
            this.chkOrganizarPlaylist.CheckedState.BorderColor = CorAcento;
            this.chkOrganizarPlaylist.CheckedState.FillColor = CorAcento;
            this.chkOrganizarPlaylist.Font = new System.Drawing.Font("Consolas", 10F);
            this.chkOrganizarPlaylist.ForeColor = CorTextoEscuro;
            this.chkOrganizarPlaylist.Location = new System.Drawing.Point(dx, dy);
            this.chkOrganizarPlaylist.Size = new System.Drawing.Size(280, 30);
            this.chkOrganizarPlaylist.Text = "Organizar em Subpastas";

            dy += 60;
            this.lblAtualizarTitulo.AutoSize = true;
            this.lblAtualizarTitulo.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold);
            this.lblAtualizarTitulo.ForeColor = CorTextoEscuro;
            this.lblAtualizarTitulo.Location = new System.Drawing.Point(dx, dy);
            this.lblAtualizarTitulo.Text = "Atualizar yt-dlp:";

            dy += 30;
            this.lblAtualizarStatus.AutoSize = true;
            this.lblAtualizarStatus.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblAtualizarStatus.ForeColor = CorTextoMuted;
            this.lblAtualizarStatus.Location = new System.Drawing.Point(dx, dy);
            this.lblAtualizarStatus.Text = "Verifique se há atualizações";

            dy += 28;
            this.btnAtualizarYtDlp.Location = new System.Drawing.Point(dx, dy);
            this.btnAtualizarYtDlp.Size = new System.Drawing.Size(280, 56);
            this.btnAtualizarYtDlp.Text = "Atualizar yt-dlp";
            this.EstiloPill(this.btnAtualizarYtDlp);
            this.btnAtualizarYtDlp.Click += new System.EventHandler(this.btnAtualizarYtDlp_Click);

            // ==============================================================
            // BARRA DE STATUS
            // ==============================================================
            this.pnlStatusBar.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.pnlStatusBar.BackColor = CorStatusBar;
            this.pnlStatusBar.Location = new System.Drawing.Point(90, 550);
            this.pnlStatusBar.Size = new System.Drawing.Size(730, 90);
            this.pnlStatusBar.Name = "pnlStatusBar";
            this.pnlStatusBar.Controls.AddRange(new System.Windows.Forms.Control[]
                { lblStatus, progressBar1, lblProgressPercent });

            this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Font = new System.Drawing.Font("Consolas", 11F);
            this.lblStatus.ForeColor = CorTextoClaro;
            this.lblStatus.Location = new System.Drawing.Point(20, 12);
            this.lblStatus.Size = new System.Drawing.Size(620, 24);
            this.lblStatus.Text = "Pronto para uso.";

            this.progressBar1.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.progressBar1.BorderRadius = 0;
            this.progressBar1.FillColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.progressBar1.ProgressColor = CorAcento;
            this.progressBar1.ProgressColor2 = CorAcento;
            this.progressBar1.Location = new System.Drawing.Point(0, 64);
            this.progressBar1.Size = new System.Drawing.Size(730, 26);

            this.lblProgressPercent.Anchor = System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Right;
            this.lblProgressPercent.AutoSize = true;
            this.lblProgressPercent.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblProgressPercent.ForeColor = CorTextoClaro;
            this.lblProgressPercent.Location = new System.Drawing.Point(660, 15);
            this.lblProgressPercent.Text = "0%";

            // ==============================================================
            // NOTIFICAÇÕES
            // ==============================================================
            this.notifyIcon1.Text = "YtDlp GUI";
            this.notifyIcon1.Visible = false;

            // ==============================================================
            // MAINFORM
            // ==============================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = CorFundo;
            this.ClientSize = new System.Drawing.Size(820, 640);
            this.MinimumSize = new System.Drawing.Size(700, 560);
            this.Font = new System.Drawing.Font("Consolas", 9F);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Downloader de Vídeos - yt-dlp GUI";
            this.Name = "MainForm";
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlStatusBar);
            this.Controls.Add(pnlSidebar);

            foreach (var pb in new System.Windows.Forms.PictureBox[]
                { picLogo, picNavHistorico, picNavPasta, picNavConfig,
                  picLogoGrande, picIlustracaoPasta, picLogoConfig, picThumb })
                ((System.ComponentModel.ISupportInitialize)pb).EndInit();
            this.menuHistorico.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}

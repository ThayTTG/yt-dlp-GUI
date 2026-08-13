using Guna.UI2.WinForms;

namespace YtDlpGui
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Paleta de cores (baseada no mockup: sálvia + menta + azul)

        private static readonly System.Drawing.Color CorFundo = System.Drawing.Color.FromArgb(92, 127, 114);
        private static readonly System.Drawing.Color CorFundoStatusBar = System.Drawing.Color.FromArgb(78, 110, 98);
        private static readonly System.Drawing.Color CorSidebar = System.Drawing.Color.FromArgb(143, 206, 187);
        private static readonly System.Drawing.Color CorCartao = System.Drawing.Color.White;
        private static readonly System.Drawing.Color CorTextoClaro = System.Drawing.Color.White;
        private static readonly System.Drawing.Color CorTextoEscuro = System.Drawing.Color.FromArgb(31, 41, 55);
        private static readonly System.Drawing.Color CorAcento = System.Drawing.Color.FromArgb(47, 125, 232);
        private static readonly System.Drawing.Color CorAcentoHover = System.Drawing.Color.FromArgb(37, 107, 204);
        private static readonly System.Drawing.Color CorPerigo = System.Drawing.Color.FromArgb(225, 92, 79);
        private static readonly System.Drawing.Color CorPerigoHover = System.Drawing.Color.FromArgb(200, 72, 60);
        private static readonly System.Drawing.Color CorDesabilitadoFundo = System.Drawing.Color.FromArgb(180, 190, 186);
        private static readonly System.Drawing.Color CorDesabilitadoTexto = System.Drawing.Color.FromArgb(120, 130, 126);

        #endregion

        #region Estrutura (barra lateral, área de conteúdo, barra de status)

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

        #endregion

        #region Página Home

        private System.Windows.Forms.Panel pnlHome;
        private System.Windows.Forms.PictureBox picLogoGrande;
        private System.Windows.Forms.Label lblUrl;
        private Guna2TextBox txtUrl;
        private Guna2Button btnDownloadMp4;
        private Guna2Button btnDownloadMp3;
        private Guna2CheckBox chkPlaylist;

        #endregion

        #region Página Histórico

        private System.Windows.Forms.Panel pnlHistorico;
        private System.Windows.Forms.Label lblTituloHistorico;
        private Guna2Panel pnlCardHistorico;
        private System.Windows.Forms.ListView listViewHistorico;
        private System.Windows.Forms.ColumnHeader colTipo;
        private System.Windows.Forms.ColumnHeader colNome;
        private System.Windows.Forms.ColumnHeader colData;
        private System.Windows.Forms.ContextMenuStrip menuHistorico;
        private System.Windows.Forms.ToolStripMenuItem menuItemAbrirPasta;
        private Guna2Button btnLimparHistorico;
        private System.Windows.Forms.Label lblHistoricoVazio;

        #endregion

        #region Página Pasta (local de salvamento)

        private System.Windows.Forms.Panel pnlPasta;
        private System.Windows.Forms.PictureBox picIlustracaoPasta;
        private System.Windows.Forms.Label lblLocalTitulo;
        private Guna2TextBox txtSavePath;
        private Guna2Button btnAlterarLocal;

        #endregion

        #region Página Configurações (autenticação por cookies)

        private System.Windows.Forms.Panel pnlConfiguracoes;
        private System.Windows.Forms.Label lblTituloConfig;
        private System.Windows.Forms.Label lblCookiesArquivoTitulo;
        private Guna2Button btnAjudaCookies;
        private Guna2RadioButton radCookiesArquivo;
        private Guna2TextBox txtCookiesArquivo;
        private Guna2Button btnSelecionarCookies;
        private System.Windows.Forms.Label lblCookiesNavegadorTitulo;
        private Guna2RadioButton radCookiesNavegador;
        private Guna2ComboBox cmbNavegador;
        private Guna2RadioButton radSemCookies;
        private System.Windows.Forms.Label lblSemCookies;
        private System.Windows.Forms.Label lblPlaylistConfigTitulo;
        private Guna2CheckBox chkOrganizarPlaylist;
        private System.Windows.Forms.Label lblPlaylistConfigDescricao;
        private Guna2Button btnAtualizarYtDlp;

        #endregion

        /// <summary>Aplica o estilo padrão de campo de texto (Guna2TextBox).</summary>
        private void EstilizarCampoTexto(Guna2TextBox campo, int borderRadius = 20)
        {
            campo.BorderRadius = borderRadius;
            campo.BorderColor = System.Drawing.Color.Transparent;
            campo.FillColor = CorCartao;
            campo.ForeColor = CorTextoEscuro;
            campo.Font = new System.Drawing.Font("Consolas", 10F);
            campo.FocusedState.BorderColor = CorAcento;
            campo.PlaceholderForeColor = System.Drawing.Color.FromArgb(150, 150, 150);
        }

        /// <summary>Aplica o estilo padrão de botão "pill" mint (ações principais das páginas).</summary>
        private void EstilizarBotaoPill(Guna2Button botao)
        {
            botao.BorderRadius = 32;
            botao.FillColor = CorSidebar;
            botao.ForeColor = CorTextoEscuro;
            botao.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold);
            botao.HoverState.FillColor = System.Drawing.Color.FromArgb(120, 190, 168);
            botao.DisabledState.FillColor = CorDesabilitadoFundo;
            botao.DisabledState.ForeColor = CorDesabilitadoTexto;
            botao.ImageAlign = System.Windows.Forms.HorizontalAlignment.Center;
            botao.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            botao.Animated = true;
        }

        /// <summary>Aplica o estilo padrão de botão pequeno (ações secundárias).</summary>
        private void EstilizarBotaoPequeno(Guna2Button botao)
        {
            botao.BorderRadius = 18;
            botao.FillColor = CorSidebar;
            botao.ForeColor = CorTextoEscuro;
            botao.Font = new System.Drawing.Font("Consolas", 9F);
            botao.HoverState.FillColor = System.Drawing.Color.FromArgb(120, 190, 168);
            botao.DisabledState.FillColor = CorDesabilitadoFundo;
            botao.DisabledState.ForeColor = CorDesabilitadoTexto;
            botao.Animated = true;
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
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
            this.picLogoGrande = new System.Windows.Forms.PictureBox();
            this.lblUrl = new System.Windows.Forms.Label();
            this.txtUrl = new Guna2TextBox();
            this.btnDownloadMp4 = new Guna2Button();
            this.btnDownloadMp3 = new Guna2Button();
            this.chkPlaylist = new Guna2CheckBox();

            this.pnlHistorico = new System.Windows.Forms.Panel();
            this.lblTituloHistorico = new System.Windows.Forms.Label();
            this.pnlCardHistorico = new Guna2Panel();
            this.listViewHistorico = new System.Windows.Forms.ListView();
            this.colTipo = new System.Windows.Forms.ColumnHeader();
            this.colNome = new System.Windows.Forms.ColumnHeader();
            this.colData = new System.Windows.Forms.ColumnHeader();
            this.menuHistorico = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemAbrirPasta = new System.Windows.Forms.ToolStripMenuItem();
            this.btnLimparHistorico = new Guna2Button();
            this.lblHistoricoVazio = new System.Windows.Forms.Label();

            this.pnlPasta = new System.Windows.Forms.Panel();
            this.picIlustracaoPasta = new System.Windows.Forms.PictureBox();
            this.lblLocalTitulo = new System.Windows.Forms.Label();
            this.txtSavePath = new Guna2TextBox();
            this.btnAlterarLocal = new Guna2Button();

            this.pnlConfiguracoes = new System.Windows.Forms.Panel();
            this.lblTituloConfig = new System.Windows.Forms.Label();
            this.lblCookiesArquivoTitulo = new System.Windows.Forms.Label();
            this.btnAjudaCookies = new Guna2Button();
            this.radCookiesArquivo = new Guna2RadioButton();
            this.txtCookiesArquivo = new Guna2TextBox();
            this.btnSelecionarCookies = new Guna2Button();
            this.lblCookiesNavegadorTitulo = new System.Windows.Forms.Label();
            this.radCookiesNavegador = new Guna2RadioButton();
            this.cmbNavegador = new Guna2ComboBox();
            this.radSemCookies = new Guna2RadioButton();
            this.lblSemCookies = new System.Windows.Forms.Label();
            this.lblPlaylistConfigTitulo = new System.Windows.Forms.Label();
            this.chkOrganizarPlaylist = new Guna2CheckBox();
            this.lblPlaylistConfigDescricao = new System.Windows.Forms.Label();
            this.btnAtualizarYtDlp = new Guna2Button();

            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNavHistorico)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNavPasta)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNavConfig)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLogoGrande)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIlustracaoPasta)).BeginInit();
            this.menuHistorico.SuspendLayout();
            this.SuspendLayout();

            // =================================================================
            // pnlSidebar
            // =================================================================
            this.pnlSidebar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlSidebar.BackColor = CorSidebar;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(90, 640);
            this.pnlSidebar.TabIndex = 0;
            this.pnlSidebar.Controls.Add(this.picLogo);
            this.pnlSidebar.Controls.Add(this.picNavHistorico);
            this.pnlSidebar.Controls.Add(this.picNavPasta);
            this.pnlSidebar.Controls.Add(this.picNavConfig);
            //
            // picLogo (clique = voltar para a Home)
            //
            this.picLogo.BackColor = System.Drawing.Color.Transparent;
            this.picLogo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picLogo.Location = new System.Drawing.Point(13, 13);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(64, 64);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            this.picLogo.Click += new System.EventHandler(this.picLogo_Click);
            //
            // picNavHistorico
            //
            this.picNavHistorico.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.picNavHistorico.BackColor = System.Drawing.Color.Transparent;
            this.picNavHistorico.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picNavHistorico.Location = new System.Drawing.Point(21, 448);
            this.picNavHistorico.Name = "picNavHistorico";
            this.picNavHistorico.Size = new System.Drawing.Size(48, 48);
            this.picNavHistorico.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picNavHistorico.TabIndex = 1;
            this.picNavHistorico.TabStop = false;
            this.picNavHistorico.Click += new System.EventHandler(this.picNavHistorico_Click);
            //
            // picNavPasta
            //
            this.picNavPasta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.picNavPasta.BackColor = System.Drawing.Color.Transparent;
            this.picNavPasta.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picNavPasta.Location = new System.Drawing.Point(21, 512);
            this.picNavPasta.Name = "picNavPasta";
            this.picNavPasta.Size = new System.Drawing.Size(48, 48);
            this.picNavPasta.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picNavPasta.TabIndex = 2;
            this.picNavPasta.TabStop = false;
            this.picNavPasta.Click += new System.EventHandler(this.picNavPasta_Click);
            //
            // picNavConfig
            //
            this.picNavConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.picNavConfig.BackColor = System.Drawing.Color.Transparent;
            this.picNavConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picNavConfig.Location = new System.Drawing.Point(21, 576);
            this.picNavConfig.Name = "picNavConfig";
            this.picNavConfig.Size = new System.Drawing.Size(48, 48);
            this.picNavConfig.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picNavConfig.TabIndex = 3;
            this.picNavConfig.TabStop = false;
            this.picNavConfig.Click += new System.EventHandler(this.picNavConfig_Click);

            // =================================================================
            // pnlContent (área central, troca de "página" conforme navegação)
            // =================================================================
            this.pnlContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlContent.BackColor = CorFundo;
            this.pnlContent.Location = new System.Drawing.Point(90, 0);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(730, 550);
            this.pnlContent.TabIndex = 1;
            this.pnlContent.Controls.Add(this.pnlConfiguracoes);
            this.pnlContent.Controls.Add(this.pnlPasta);
            this.pnlContent.Controls.Add(this.pnlHistorico);
            this.pnlContent.Controls.Add(this.pnlHome);
            this.pnlContent.Controls.Add(this.btnCancelar);
            //
            // btnCancelar (círculo vermelho "X", flutuante no canto inferior direito)
            //
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.BorderRadius = 28;
            this.btnCancelar.Enabled = false;
            this.btnCancelar.FillColor = CorPerigo;
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Font = new System.Drawing.Font("Consolas", 14F, System.Drawing.FontStyle.Bold);
            this.btnCancelar.HoverState.FillColor = CorPerigoHover;
            this.btnCancelar.DisabledState.FillColor = System.Drawing.Color.FromArgb(190, 150, 145);
            this.btnCancelar.DisabledState.ForeColor = System.Drawing.Color.FromArgb(230, 210, 208);
            this.btnCancelar.Location = new System.Drawing.Point(654, 474);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(56, 56);
            this.btnCancelar.TabIndex = 20;
            this.btnCancelar.Text = "X";
            this.btnCancelar.Animated = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);

            // =================================================================
            // pnlHome
            // =================================================================
            this.pnlHome.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlHome.BackColor = System.Drawing.Color.Transparent;
            this.pnlHome.Location = new System.Drawing.Point(0, 0);
            this.pnlHome.Name = "pnlHome";
            this.pnlHome.Size = new System.Drawing.Size(730, 550);
            this.pnlHome.TabIndex = 0;
            this.pnlHome.Controls.Add(this.chkPlaylist);
            this.pnlHome.Controls.Add(this.btnDownloadMp3);
            this.pnlHome.Controls.Add(this.btnDownloadMp4);
            this.pnlHome.Controls.Add(this.txtUrl);
            this.pnlHome.Controls.Add(this.lblUrl);
            this.pnlHome.Controls.Add(this.picLogoGrande);
            //
            // picLogoGrande
            //
            this.picLogoGrande.Location = new System.Drawing.Point(275, 40);
            this.picLogoGrande.Name = "picLogoGrande";
            this.picLogoGrande.Size = new System.Drawing.Size(180, 180);
            this.picLogoGrande.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogoGrande.TabIndex = 0;
            this.picLogoGrande.TabStop = false;
            //
            // lblUrl
            //
            this.lblUrl.AutoSize = true;
            this.lblUrl.Font = new System.Drawing.Font("Consolas", 12F);
            this.lblUrl.ForeColor = CorTextoClaro;
            this.lblUrl.Location = new System.Drawing.Point(40, 250);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new System.Drawing.Size(230, 19);
            this.lblUrl.TabIndex = 1;
            this.lblUrl.Text = "URL do Link do Vídeo:";
            //
            // txtUrl
            //
            this.txtUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUrl.Location = new System.Drawing.Point(40, 278);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.PlaceholderText = "Cole aqui o link do vídeo do YouTube...";
            this.txtUrl.Size = new System.Drawing.Size(650, 44);
            this.txtUrl.TabIndex = 2;
            this.EstilizarCampoTexto(this.txtUrl, 22);
            //
            // btnDownloadMp4
            //
            this.btnDownloadMp4.Location = new System.Drawing.Point(40, 340);
            this.btnDownloadMp4.Name = "btnDownloadMp4";
            this.btnDownloadMp4.Size = new System.Drawing.Size(310, 64);
            this.btnDownloadMp4.TabIndex = 3;
            this.btnDownloadMp4.Text = "Baixar .MP4";
            this.EstilizarBotaoPill(this.btnDownloadMp4);
            this.btnDownloadMp4.Click += new System.EventHandler(this.btnDownloadMp4_Click);
            //
            // btnDownloadMp3
            //
            this.btnDownloadMp3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownloadMp3.Location = new System.Drawing.Point(366, 340);
            this.btnDownloadMp3.Name = "btnDownloadMp3";
            this.btnDownloadMp3.Size = new System.Drawing.Size(310, 64);
            this.btnDownloadMp3.TabIndex = 4;
            this.btnDownloadMp3.Text = "Baixar .MP3";
            this.EstilizarBotaoPill(this.btnDownloadMp3);
            this.btnDownloadMp3.Click += new System.EventHandler(this.btnDownloadMp3_Click);
            //
            // chkPlaylist
            //
            this.chkPlaylist.AutoSize = true;
            this.chkPlaylist.CheckedState.BorderColor = CorAcento;
            this.chkPlaylist.CheckedState.FillColor = CorAcento;
            this.chkPlaylist.Font = new System.Drawing.Font("Consolas", 11F);
            this.chkPlaylist.ForeColor = CorTextoClaro;
            this.chkPlaylist.Location = new System.Drawing.Point(40, 424);
            this.chkPlaylist.Name = "chkPlaylist";
            this.chkPlaylist.Size = new System.Drawing.Size(220, 30);
            this.chkPlaylist.TabIndex = 5;
            this.chkPlaylist.Text = "Baixar Playlist";

            // =================================================================
            // pnlHistorico
            // =================================================================
            this.pnlHistorico.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlHistorico.BackColor = System.Drawing.Color.Transparent;
            this.pnlHistorico.Location = new System.Drawing.Point(0, 0);
            this.pnlHistorico.Name = "pnlHistorico";
            this.pnlHistorico.Size = new System.Drawing.Size(730, 550);
            this.pnlHistorico.TabIndex = 1;
            this.pnlHistorico.Visible = false;
            this.pnlHistorico.Controls.Add(this.lblHistoricoVazio);
            this.pnlHistorico.Controls.Add(this.btnLimparHistorico);
            this.pnlHistorico.Controls.Add(this.pnlCardHistorico);
            this.pnlHistorico.Controls.Add(this.lblTituloHistorico);
            //
            // lblTituloHistorico
            //
            this.lblTituloHistorico.AutoSize = true;
            this.lblTituloHistorico.Font = new System.Drawing.Font("Consolas", 16F);
            this.lblTituloHistorico.ForeColor = CorTextoClaro;
            this.lblTituloHistorico.Location = new System.Drawing.Point(40, 30);
            this.lblTituloHistorico.Name = "lblTituloHistorico";
            this.lblTituloHistorico.Size = new System.Drawing.Size(320, 25);
            this.lblTituloHistorico.TabIndex = 0;
            this.lblTituloHistorico.Text = "Histórico de Downloads";
            //
            // pnlCardHistorico
            //
            this.pnlCardHistorico.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCardHistorico.BorderRadius = 16;
            this.pnlCardHistorico.FillColor = CorCartao;
            this.pnlCardHistorico.Location = new System.Drawing.Point(40, 90);
            this.pnlCardHistorico.Name = "pnlCardHistorico";
            this.pnlCardHistorico.Size = new System.Drawing.Size(650, 380);
            this.pnlCardHistorico.TabIndex = 1;
            this.pnlCardHistorico.Controls.Add(this.listViewHistorico);
            //
            // listViewHistorico
            //
            this.listViewHistorico.BackColor = CorCartao;
            this.listViewHistorico.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewHistorico.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTipo,
            this.colNome,
            this.colData});
            this.listViewHistorico.ContextMenuStrip = this.menuHistorico;
            this.listViewHistorico.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewHistorico.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.listViewHistorico.ForeColor = CorTextoEscuro;
            this.listViewHistorico.FullRowSelect = true;
            this.listViewHistorico.GridLines = false;
            this.listViewHistorico.HideSelection = false;
            this.listViewHistorico.Location = new System.Drawing.Point(0, 0);
            this.listViewHistorico.MultiSelect = false;
            this.listViewHistorico.Name = "listViewHistorico";
            this.listViewHistorico.Padding = new System.Windows.Forms.Padding(12);
            this.listViewHistorico.Size = new System.Drawing.Size(650, 380);
            this.listViewHistorico.TabIndex = 0;
            this.listViewHistorico.UseCompatibleStateImageBehavior = false;
            this.listViewHistorico.View = System.Windows.Forms.View.Details;
            this.listViewHistorico.DoubleClick += new System.EventHandler(this.menuItemAbrirPasta_Click);
            //
            // colTipo
            //
            this.colTipo.Text = "Tipo";
            this.colTipo.Width = 80;
            //
            // colNome
            //
            this.colNome.Text = "Nome";
            this.colNome.Width = 380;
            //
            // colData
            //
            this.colData.Text = "Data";
            this.colData.Width = 160;
            //
            // menuHistorico
            //
            this.menuHistorico.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemAbrirPasta});
            this.menuHistorico.Name = "menuHistorico";
            this.menuHistorico.Size = new System.Drawing.Size(180, 26);
            //
            // menuItemAbrirPasta
            //
            this.menuItemAbrirPasta.Name = "menuItemAbrirPasta";
            this.menuItemAbrirPasta.Text = "Abrir pasta do item";
            this.menuItemAbrirPasta.Click += new System.EventHandler(this.menuItemAbrirPasta_Click);
            //
            // lblHistoricoVazio
            //
            this.lblHistoricoVazio.AutoSize = true;
            this.lblHistoricoVazio.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.lblHistoricoVazio.ForeColor = CorTextoEscuro;
            this.lblHistoricoVazio.Location = new System.Drawing.Point(60, 118);
            this.lblHistoricoVazio.Name = "lblHistoricoVazio";
            this.lblHistoricoVazio.Size = new System.Drawing.Size(260, 19);
            this.lblHistoricoVazio.TabIndex = 2;
            this.lblHistoricoVazio.Text = "Nenhum download foi registrado ainda.";
            this.lblHistoricoVazio.Visible = false;
            //
            // btnLimparHistorico
            //
            this.btnLimparHistorico.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLimparHistorico.Location = new System.Drawing.Point(40, 480);
            this.btnLimparHistorico.Name = "btnLimparHistorico";
            this.btnLimparHistorico.Size = new System.Drawing.Size(150, 36);
            this.btnLimparHistorico.TabIndex = 3;
            this.btnLimparHistorico.Text = "Limpar Histórico";
            this.EstilizarBotaoPequeno(this.btnLimparHistorico);
            this.btnLimparHistorico.Click += new System.EventHandler(this.btnLimparHistorico_Click);

            // =================================================================
            // pnlPasta
            // =================================================================
            this.pnlPasta.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlPasta.BackColor = System.Drawing.Color.Transparent;
            this.pnlPasta.Location = new System.Drawing.Point(0, 0);
            this.pnlPasta.Name = "pnlPasta";
            this.pnlPasta.Size = new System.Drawing.Size(730, 550);
            this.pnlPasta.TabIndex = 2;
            this.pnlPasta.Visible = false;
            this.pnlPasta.Controls.Add(this.btnAlterarLocal);
            this.pnlPasta.Controls.Add(this.txtSavePath);
            this.pnlPasta.Controls.Add(this.lblLocalTitulo);
            this.pnlPasta.Controls.Add(this.picIlustracaoPasta);
            //
            // picIlustracaoPasta
            //
            this.picIlustracaoPasta.Location = new System.Drawing.Point(275, 40);
            this.picIlustracaoPasta.Name = "picIlustracaoPasta";
            this.picIlustracaoPasta.Size = new System.Drawing.Size(180, 180);
            this.picIlustracaoPasta.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picIlustracaoPasta.TabIndex = 0;
            this.picIlustracaoPasta.TabStop = false;
            //
            // lblLocalTitulo
            //
            this.lblLocalTitulo.AutoSize = true;
            this.lblLocalTitulo.Font = new System.Drawing.Font("Consolas", 12F);
            this.lblLocalTitulo.ForeColor = CorTextoClaro;
            this.lblLocalTitulo.Location = new System.Drawing.Point(40, 250);
            this.lblLocalTitulo.Name = "lblLocalTitulo";
            this.lblLocalTitulo.Size = new System.Drawing.Size(210, 19);
            this.lblLocalTitulo.TabIndex = 1;
            this.lblLocalTitulo.Text = "Local de Salvamento:";
            //
            // txtSavePath
            //
            this.txtSavePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSavePath.Location = new System.Drawing.Point(40, 278);
            this.txtSavePath.Name = "txtSavePath";
            this.txtSavePath.ReadOnly = true;
            this.txtSavePath.Size = new System.Drawing.Size(650, 44);
            this.txtSavePath.TabIndex = 2;
            this.EstilizarCampoTexto(this.txtSavePath, 22);
            //
            // btnAlterarLocal
            //
            this.btnAlterarLocal.Location = new System.Drawing.Point(213, 340);
            this.btnAlterarLocal.Name = "btnAlterarLocal";
            this.btnAlterarLocal.Size = new System.Drawing.Size(304, 64);
            this.btnAlterarLocal.TabIndex = 3;
            this.btnAlterarLocal.Text = "Alterar Local";
            this.EstilizarBotaoPill(this.btnAlterarLocal);
            this.btnAlterarLocal.Click += new System.EventHandler(this.btnAlterarLocal_Click);

            // =================================================================
            // pnlConfiguracoes
            // =================================================================
            this.pnlConfiguracoes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlConfiguracoes.BackColor = System.Drawing.Color.Transparent;
            this.pnlConfiguracoes.Location = new System.Drawing.Point(0, 0);
            this.pnlConfiguracoes.Name = "pnlConfiguracoes";
            this.pnlConfiguracoes.Size = new System.Drawing.Size(730, 550);
            this.pnlConfiguracoes.TabIndex = 3;
            this.pnlConfiguracoes.Visible = false;
            this.pnlConfiguracoes.Controls.Add(this.btnAtualizarYtDlp);
            this.pnlConfiguracoes.Controls.Add(this.lblPlaylistConfigDescricao);
            this.pnlConfiguracoes.Controls.Add(this.chkOrganizarPlaylist);
            this.pnlConfiguracoes.Controls.Add(this.lblPlaylistConfigTitulo);
            this.pnlConfiguracoes.Controls.Add(this.lblSemCookies);
            this.pnlConfiguracoes.Controls.Add(this.radSemCookies);
            this.pnlConfiguracoes.Controls.Add(this.cmbNavegador);
            this.pnlConfiguracoes.Controls.Add(this.radCookiesNavegador);
            this.pnlConfiguracoes.Controls.Add(this.lblCookiesNavegadorTitulo);
            this.pnlConfiguracoes.Controls.Add(this.btnSelecionarCookies);
            this.pnlConfiguracoes.Controls.Add(this.txtCookiesArquivo);
            this.pnlConfiguracoes.Controls.Add(this.radCookiesArquivo);
            this.pnlConfiguracoes.Controls.Add(this.btnAjudaCookies);
            this.pnlConfiguracoes.Controls.Add(this.lblCookiesArquivoTitulo);
            this.pnlConfiguracoes.Controls.Add(this.lblTituloConfig);
            //
            // lblTituloConfig
            //
            this.lblTituloConfig.AutoSize = true;
            this.lblTituloConfig.Font = new System.Drawing.Font("Consolas", 16F);
            this.lblTituloConfig.ForeColor = CorTextoClaro;
            this.lblTituloConfig.Location = new System.Drawing.Point(40, 30);
            this.lblTituloConfig.Name = "lblTituloConfig";
            this.lblTituloConfig.Size = new System.Drawing.Size(200, 25);
            this.lblTituloConfig.TabIndex = 0;
            this.lblTituloConfig.Text = "Configurações";
            //
            // lblCookiesArquivoTitulo
            //
            this.lblCookiesArquivoTitulo.AutoSize = true;
            this.lblCookiesArquivoTitulo.Font = new System.Drawing.Font("Consolas", 11F);
            this.lblCookiesArquivoTitulo.ForeColor = CorTextoClaro;
            this.lblCookiesArquivoTitulo.Location = new System.Drawing.Point(40, 90);
            this.lblCookiesArquivoTitulo.Name = "lblCookiesArquivoTitulo";
            this.lblCookiesArquivoTitulo.Size = new System.Drawing.Size(260, 18);
            this.lblCookiesArquivoTitulo.TabIndex = 1;
            this.lblCookiesArquivoTitulo.Text = "Usar arquivo cookies.txt exportado";
            //
            // btnAjudaCookies
            //
            this.btnAjudaCookies.BorderRadius = 13;
            this.btnAjudaCookies.FillColor = System.Drawing.Color.FromArgb(255, 255, 255, 60);
            this.btnAjudaCookies.ForeColor = CorTextoClaro;
            this.btnAjudaCookies.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            this.btnAjudaCookies.HoverState.FillColor = System.Drawing.Color.FromArgb(255, 255, 255, 100);
            this.btnAjudaCookies.Location = new System.Drawing.Point(320, 88);
            this.btnAjudaCookies.Name = "btnAjudaCookies";
            this.btnAjudaCookies.Size = new System.Drawing.Size(26, 26);
            this.btnAjudaCookies.TabIndex = 1;
            this.btnAjudaCookies.Text = "?";
            this.btnAjudaCookies.Animated = true;
            this.btnAjudaCookies.Click += new System.EventHandler(this.btnAjudaCookies_Click);
            //
            // radCookiesArquivo
            //
            this.radCookiesArquivo.CheckedState.BorderColor = CorAcento;
            this.radCookiesArquivo.CheckedState.FillColor = CorAcento;
            this.radCookiesArquivo.Location = new System.Drawing.Point(40, 122);
            this.radCookiesArquivo.Name = "radCookiesArquivo";
            this.radCookiesArquivo.Size = new System.Drawing.Size(36, 36);
            this.radCookiesArquivo.TabIndex = 2;
            this.radCookiesArquivo.Text = "";
            this.radCookiesArquivo.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);
            //
            // txtCookiesArquivo
            //
            this.txtCookiesArquivo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCookiesArquivo.Enabled = false;
            this.txtCookiesArquivo.Location = new System.Drawing.Point(90, 120);
            this.txtCookiesArquivo.Name = "txtCookiesArquivo";
            this.txtCookiesArquivo.ReadOnly = true;
            this.txtCookiesArquivo.PlaceholderText = "Nenhum arquivo selecionado — clique em \"Buscar\" ou toque em \"?\" para ajuda";
            this.txtCookiesArquivo.Size = new System.Drawing.Size(500, 40);
            this.txtCookiesArquivo.TabIndex = 3;
            this.EstilizarCampoTexto(this.txtCookiesArquivo, 18);
            //
            // btnSelecionarCookies
            //
            this.btnSelecionarCookies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelecionarCookies.Enabled = false;
            this.btnSelecionarCookies.Location = new System.Drawing.Point(600, 120);
            this.btnSelecionarCookies.Name = "btnSelecionarCookies";
            this.btnSelecionarCookies.Size = new System.Drawing.Size(90, 40);
            this.btnSelecionarCookies.TabIndex = 4;
            this.btnSelecionarCookies.Text = "Buscar";
            this.EstilizarBotaoPequeno(this.btnSelecionarCookies);
            this.btnSelecionarCookies.Click += new System.EventHandler(this.btnSelecionarCookies_Click);
            //
            // lblCookiesNavegadorTitulo
            //
            this.lblCookiesNavegadorTitulo.AutoSize = true;
            this.lblCookiesNavegadorTitulo.Font = new System.Drawing.Font("Consolas", 11F);
            this.lblCookiesNavegadorTitulo.ForeColor = CorTextoClaro;
            this.lblCookiesNavegadorTitulo.Location = new System.Drawing.Point(40, 190);
            this.lblCookiesNavegadorTitulo.Name = "lblCookiesNavegadorTitulo";
            this.lblCookiesNavegadorTitulo.Size = new System.Drawing.Size(280, 18);
            this.lblCookiesNavegadorTitulo.TabIndex = 5;
            this.lblCookiesNavegadorTitulo.Text = "Usar cookies do navegador";
            //
            // radCookiesNavegador
            //
            this.radCookiesNavegador.CheckedState.BorderColor = CorAcento;
            this.radCookiesNavegador.CheckedState.FillColor = CorAcento;
            this.radCookiesNavegador.Location = new System.Drawing.Point(40, 222);
            this.radCookiesNavegador.Name = "radCookiesNavegador";
            this.radCookiesNavegador.Size = new System.Drawing.Size(36, 36);
            this.radCookiesNavegador.TabIndex = 6;
            this.radCookiesNavegador.Text = "";
            this.radCookiesNavegador.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);
            //
            // cmbNavegador
            //
            this.cmbNavegador.BackColor = System.Drawing.Color.Transparent;
            this.cmbNavegador.BorderRadius = 18;
            this.cmbNavegador.BorderColor = System.Drawing.Color.Transparent;
            this.cmbNavegador.Enabled = false;
            this.cmbNavegador.FillColor = CorCartao;
            this.cmbNavegador.ForeColor = CorTextoEscuro;
            this.cmbNavegador.Font = new System.Drawing.Font("Consolas", 10F);
            this.cmbNavegador.Items.AddRange(new object[] {
            "chrome",
            "edge",
            "firefox",
            "brave",
            "opera",
            "vivaldi",
            "chromium"});
            this.cmbNavegador.Location = new System.Drawing.Point(90, 220);
            this.cmbNavegador.Name = "cmbNavegador";
            this.cmbNavegador.Size = new System.Drawing.Size(200, 40);
            this.cmbNavegador.TabIndex = 7;
            //
            // radSemCookies
            //
            this.radSemCookies.Checked = true;
            this.radSemCookies.CheckedState.BorderColor = CorAcento;
            this.radSemCookies.CheckedState.FillColor = CorAcento;
            this.radSemCookies.Location = new System.Drawing.Point(40, 290);
            this.radSemCookies.Name = "radSemCookies";
            this.radSemCookies.Size = new System.Drawing.Size(36, 36);
            this.radSemCookies.TabIndex = 8;
            this.radSemCookies.TabStop = true;
            this.radSemCookies.Text = "";
            this.radSemCookies.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);
            //
            // lblSemCookies
            //
            this.lblSemCookies.AutoSize = true;
            this.lblSemCookies.Font = new System.Drawing.Font("Consolas", 11F);
            this.lblSemCookies.ForeColor = CorTextoClaro;
            this.lblSemCookies.Location = new System.Drawing.Point(90, 298);
            this.lblSemCookies.Name = "lblSemCookies";
            this.lblSemCookies.Size = new System.Drawing.Size(180, 18);
            this.lblSemCookies.TabIndex = 9;
            this.lblSemCookies.Text = "Não usar cookies";
            //
            // lblPlaylistConfigTitulo
            //
            this.lblPlaylistConfigTitulo.AutoSize = true;
            this.lblPlaylistConfigTitulo.Font = new System.Drawing.Font("Consolas", 11F);
            this.lblPlaylistConfigTitulo.ForeColor = CorTextoClaro;
            this.lblPlaylistConfigTitulo.Location = new System.Drawing.Point(40, 360);
            this.lblPlaylistConfigTitulo.Name = "lblPlaylistConfigTitulo";
            this.lblPlaylistConfigTitulo.Size = new System.Drawing.Size(180, 18);
            this.lblPlaylistConfigTitulo.TabIndex = 9;
            this.lblPlaylistConfigTitulo.Text = "Downloads de playlist";
            //
            // chkOrganizarPlaylist
            //
            this.chkOrganizarPlaylist.AutoSize = true;
            this.chkOrganizarPlaylist.CheckedState.BorderColor = CorAcento;
            this.chkOrganizarPlaylist.CheckedState.FillColor = CorAcento;
            this.chkOrganizarPlaylist.Font = new System.Drawing.Font("Consolas", 10F);
            this.chkOrganizarPlaylist.ForeColor = CorTextoClaro;
            this.chkOrganizarPlaylist.Location = new System.Drawing.Point(40, 392);
            this.chkOrganizarPlaylist.Name = "chkOrganizarPlaylist";
            this.chkOrganizarPlaylist.Size = new System.Drawing.Size(500, 26);
            this.chkOrganizarPlaylist.TabIndex = 9;
            this.chkOrganizarPlaylist.Text = "Organizar cada playlist em uma subpasta própria";
            //
            // lblPlaylistConfigDescricao
            //
            this.lblPlaylistConfigDescricao.AutoSize = true;
            this.lblPlaylistConfigDescricao.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.lblPlaylistConfigDescricao.ForeColor = System.Drawing.Color.FromArgb(220, 230, 226);
            this.lblPlaylistConfigDescricao.Location = new System.Drawing.Point(40, 420);
            this.lblPlaylistConfigDescricao.MaximumSize = new System.Drawing.Size(650, 0);
            this.lblPlaylistConfigDescricao.Name = "lblPlaylistConfigDescricao";
            this.lblPlaylistConfigDescricao.Size = new System.Drawing.Size(600, 15);
            this.lblPlaylistConfigDescricao.TabIndex = 10;
            this.lblPlaylistConfigDescricao.Text = "Marcado: 1 item no Histórico por playlist. Desmarcado: 1 item por vídeo.";
            //
            // btnAtualizarYtDlp
            //
            this.btnAtualizarYtDlp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAtualizarYtDlp.Location = new System.Drawing.Point(40, 470);
            this.btnAtualizarYtDlp.Name = "btnAtualizarYtDlp";
            this.btnAtualizarYtDlp.Size = new System.Drawing.Size(200, 40);
            this.btnAtualizarYtDlp.TabIndex = 10;
            this.btnAtualizarYtDlp.Text = "Atualizar yt-dlp";
            this.EstilizarBotaoPequeno(this.btnAtualizarYtDlp);
            this.btnAtualizarYtDlp.Click += new System.EventHandler(this.btnAtualizarYtDlp_Click);

            // =================================================================
            // pnlStatusBar
            // =================================================================
            this.pnlStatusBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlStatusBar.BackColor = CorFundoStatusBar;
            this.pnlStatusBar.Location = new System.Drawing.Point(90, 550);
            this.pnlStatusBar.Name = "pnlStatusBar";
            this.pnlStatusBar.Size = new System.Drawing.Size(730, 90);
            this.pnlStatusBar.TabIndex = 2;
            this.pnlStatusBar.Controls.Add(this.lblProgressPercent);
            this.pnlStatusBar.Controls.Add(this.progressBar1);
            this.pnlStatusBar.Controls.Add(this.lblStatus);
            //
            // lblStatus
            //
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Font = new System.Drawing.Font("Consolas", 11F);
            this.lblStatus.ForeColor = CorTextoClaro;
            this.lblStatus.Location = new System.Drawing.Point(20, 12);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(600, 24);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Pronto para uso.";
            //
            // progressBar1
            //
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.BorderRadius = 0;
            this.progressBar1.FillColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.progressBar1.ProgressColor = CorAcento;
            this.progressBar1.ProgressColor2 = CorAcento;
            this.progressBar1.Location = new System.Drawing.Point(0, 64);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(730, 26);
            this.progressBar1.TabIndex = 1;
            //
            // lblProgressPercent
            //
            this.lblProgressPercent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgressPercent.AutoSize = true;
            this.lblProgressPercent.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblProgressPercent.ForeColor = CorTextoClaro;
            this.lblProgressPercent.Location = new System.Drawing.Point(660, 15);
            this.lblProgressPercent.Name = "lblProgressPercent";
            this.lblProgressPercent.Size = new System.Drawing.Size(40, 18);
            this.lblProgressPercent.TabIndex = 2;
            this.lblProgressPercent.Text = "0%";

            // =================================================================
            // notifyIcon1
            // =================================================================
            this.notifyIcon1.Text = "YtDlp GUI";
            this.notifyIcon1.Visible = false;

            // =================================================================
            // MainForm
            // =================================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = CorFundo;
            this.ClientSize = new System.Drawing.Size(820, 640);
            this.MinimumSize = new System.Drawing.Size(700, 560);
            this.Controls.Add(this.pnlStatusBar);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlSidebar);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Downloader de Vídeos - yt-dlp GUI";

            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNavHistorico)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNavPasta)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNavConfig)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLogoGrande)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIlustracaoPasta)).EndInit();
            this.menuHistorico.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}

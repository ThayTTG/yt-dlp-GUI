using Guna.UI2.WinForms;

namespace YtDlpGui
{
    partial class MainForm
    {
        /// <summary>
        /// Variável necessária para o Designer.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpa os recursos utilizados.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Paleta de cores do app (tons claros/neutros + acento azul)

        // Fundo geral da janela.
        private static readonly System.Drawing.Color CorFundo = System.Drawing.Color.FromArgb(247, 248, 250);
        // Fundo de "cards"/campos (branco).
        private static readonly System.Drawing.Color CorCartao = System.Drawing.Color.White;
        // Borda neutra usada em campos e cards.
        private static readonly System.Drawing.Color CorBorda = System.Drawing.Color.FromArgb(225, 228, 232);
        // Texto principal (escuro).
        private static readonly System.Drawing.Color CorTexto = System.Drawing.Color.FromArgb(31, 41, 55);
        // Texto secundário (cinza médio).
        private static readonly System.Drawing.Color CorTextoSecundario = System.Drawing.Color.FromArgb(107, 114, 128);
        // Cor de acento (ações principais).
        private static readonly System.Drawing.Color CorAcento = System.Drawing.Color.FromArgb(47, 111, 237);
        private static readonly System.Drawing.Color CorAcentoHover = System.Drawing.Color.FromArgb(37, 94, 209);
        private static readonly System.Drawing.Color CorAcentoPressed = System.Drawing.Color.FromArgb(30, 79, 181);
        // Cor de perigo (Cancelar).
        private static readonly System.Drawing.Color CorPerigo = System.Drawing.Color.FromArgb(239, 68, 68);
        private static readonly System.Drawing.Color CorPerigoHover = System.Drawing.Color.FromArgb(220, 38, 38);
        // Fundo de botões secundários (neutros).
        private static readonly System.Drawing.Color CorBotaoSecundario = System.Drawing.Color.FromArgb(243, 244, 246);
        // Estado desabilitado (fundo e texto claramente legíveis, ao contrário do padrão do Windows).
        private static readonly System.Drawing.Color CorDesabilitadoFundo = System.Drawing.Color.FromArgb(229, 231, 235);
        private static readonly System.Drawing.Color CorDesabilitadoTexto = System.Drawing.Color.FromArgb(156, 163, 175);

        #endregion

        #region Código gerado pelo Designer de Formulários Windows

        private System.Windows.Forms.Label lblUrl;
        private Guna2Button btnHistorico;
        private Guna2Button btnAtualizarYtDlp;
        private Guna2TextBox txtUrl;
        private System.Windows.Forms.Label lblSavePath;
        private Guna2TextBox txtSavePath;
        private Guna2Button btnSelectFolder;
        private Guna2Button btnDownloadMp4;
        private Guna2Button btnDownloadMp3;
        private Guna2Button btnCancelar;
        private Guna2CheckBox chkPlaylist;
        private Guna2GroupBox grpAutenticacao;
        private Guna2RadioButton radSemCookies;
        private Guna2RadioButton radCookiesNavegador;
        private Guna2ComboBox cmbNavegador;
        private Guna2RadioButton radCookiesArquivo;
        private Guna2TextBox txtCookiesArquivo;
        private Guna2Button btnSelecionarCookies;
        private System.Windows.Forms.Label lblStatusTitle;
        private System.Windows.Forms.RichTextBox rtbStatus;
        private Guna2ProgressBar progressBar1;
        private System.Windows.Forms.Label lblProgressPercent;

        /// <summary>
        /// Configura o visual padrão (arredondamento, cores, fonte) de um
        /// botão "primário" (ação principal, fundo na cor de acento).
        /// </summary>
        private void EstilizarBotaoPrimario(Guna2Button botao)
        {
            botao.BorderRadius = 10;
            botao.FillColor = CorAcento;
            botao.ForeColor = System.Drawing.Color.White;
            botao.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            botao.HoverState.FillColor = CorAcentoHover;
            botao.PressedColor = CorAcentoPressed;
            botao.DisabledState.FillColor = CorDesabilitadoFundo;
            botao.DisabledState.ForeColor = CorDesabilitadoTexto;
            botao.DisabledState.BorderColor = CorDesabilitadoFundo;
            botao.Animated = true;
        }

        /// <summary>
        /// Configura o visual padrão de um botão "secundário" (ações de
        /// apoio, fundo neutro com borda).
        /// </summary>
        private void EstilizarBotaoSecundario(Guna2Button botao)
        {
            botao.BorderRadius = 8;
            botao.FillColor = CorBotaoSecundario;
            botao.ForeColor = CorTexto;
            botao.BorderColor = CorBorda;
            botao.BorderThickness = 1;
            botao.Font = new System.Drawing.Font("Segoe UI", 9F);
            botao.HoverState.FillColor = System.Drawing.Color.FromArgb(233, 235, 239);
            botao.DisabledState.FillColor = CorDesabilitadoFundo;
            botao.DisabledState.ForeColor = CorDesabilitadoTexto;
            botao.DisabledState.BorderColor = CorDesabilitadoFundo;
            botao.Animated = true;
        }

        /// <summary>
        /// Configura o visual padrão de um campo de texto (Guna2TextBox).
        /// </summary>
        private void EstilizarCampoTexto(Guna2TextBox campo)
        {
            campo.BorderRadius = 8;
            campo.BorderColor = CorBorda;
            campo.FillColor = CorCartao;
            campo.ForeColor = CorTexto;
            campo.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            campo.FocusedState.BorderColor = CorAcento;
            campo.HoverState.BorderColor = System.Drawing.Color.FromArgb(190, 196, 204);
            campo.PlaceholderForeColor = CorTextoSecundario;
        }

        /// <summary>
        /// Método necessário para suporte ao Designer.
        /// Não modifique o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblUrl = new System.Windows.Forms.Label();
            this.btnHistorico = new Guna2Button();
            this.btnAtualizarYtDlp = new Guna2Button();
            this.txtUrl = new Guna2TextBox();
            this.lblSavePath = new System.Windows.Forms.Label();
            this.txtSavePath = new Guna2TextBox();
            this.btnSelectFolder = new Guna2Button();
            this.btnDownloadMp4 = new Guna2Button();
            this.btnDownloadMp3 = new Guna2Button();
            this.btnCancelar = new Guna2Button();
            this.chkPlaylist = new Guna2CheckBox();
            this.grpAutenticacao = new Guna2GroupBox();
            this.radSemCookies = new Guna2RadioButton();
            this.radCookiesNavegador = new Guna2RadioButton();
            this.cmbNavegador = new Guna2ComboBox();
            this.radCookiesArquivo = new Guna2RadioButton();
            this.txtCookiesArquivo = new Guna2TextBox();
            this.btnSelecionarCookies = new Guna2Button();
            this.lblStatusTitle = new System.Windows.Forms.Label();
            this.rtbStatus = new System.Windows.Forms.RichTextBox();
            this.progressBar1 = new Guna2ProgressBar();
            this.lblProgressPercent = new System.Windows.Forms.Label();
            this.grpAutenticacao.SuspendLayout();
            this.SuspendLayout();
            //
            // lblUrl
            //
            this.lblUrl.AutoSize = true;
            this.lblUrl.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.lblUrl.ForeColor = CorTexto;
            this.lblUrl.Location = new System.Drawing.Point(14, 15);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new System.Drawing.Size(160, 17);
            this.lblUrl.TabIndex = 0;
            this.lblUrl.Text = "URL do vídeo do YouTube:";
            //
            // btnHistorico
            //
            this.btnHistorico.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHistorico.Location = new System.Drawing.Point(478, 10);
            this.btnHistorico.Name = "btnHistorico";
            this.btnHistorico.Size = new System.Drawing.Size(90, 30);
            this.btnHistorico.TabIndex = 13;
            this.btnHistorico.Text = "Histórico";
            this.EstilizarBotaoSecundario(this.btnHistorico);
            this.btnHistorico.Click += new System.EventHandler(this.btnHistorico_Click);
            //
            // btnAtualizarYtDlp
            //
            this.btnAtualizarYtDlp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAtualizarYtDlp.Location = new System.Drawing.Point(576, 10);
            this.btnAtualizarYtDlp.Name = "btnAtualizarYtDlp";
            this.btnAtualizarYtDlp.Size = new System.Drawing.Size(110, 30);
            this.btnAtualizarYtDlp.TabIndex = 14;
            this.btnAtualizarYtDlp.Text = "Atualizar yt-dlp";
            this.EstilizarBotaoSecundario(this.btnAtualizarYtDlp);
            this.btnAtualizarYtDlp.Click += new System.EventHandler(this.btnAtualizarYtDlp_Click);
            //
            // txtUrl
            //
            this.txtUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUrl.Location = new System.Drawing.Point(14, 48);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.PlaceholderText = "Cole aqui o link do vídeo (ex: https://www.youtube.com/watch?v=...)";
            this.txtUrl.Size = new System.Drawing.Size(672, 34);
            this.txtUrl.TabIndex = 1;
            this.EstilizarCampoTexto(this.txtUrl);
            //
            // lblSavePath
            //
            this.lblSavePath.AutoSize = true;
            this.lblSavePath.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.lblSavePath.ForeColor = CorTexto;
            this.lblSavePath.Location = new System.Drawing.Point(14, 96);
            this.lblSavePath.Name = "lblSavePath";
            this.lblSavePath.Size = new System.Drawing.Size(115, 17);
            this.lblSavePath.TabIndex = 2;
            this.lblSavePath.Text = "Local de salvamento:";
            //
            // txtSavePath
            //
            this.txtSavePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSavePath.Location = new System.Drawing.Point(14, 122);
            this.txtSavePath.Name = "txtSavePath";
            this.txtSavePath.ReadOnly = true;
            this.txtSavePath.Size = new System.Drawing.Size(554, 34);
            this.txtSavePath.TabIndex = 3;
            this.EstilizarCampoTexto(this.txtSavePath);
            //
            // btnSelectFolder
            //
            this.btnSelectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectFolder.Location = new System.Drawing.Point(576, 122);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(110, 34);
            this.btnSelectFolder.TabIndex = 4;
            this.btnSelectFolder.Text = "Selecionar";
            this.EstilizarBotaoSecundario(this.btnSelectFolder);
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            //
            // btnDownloadMp4
            //
            this.btnDownloadMp4.Location = new System.Drawing.Point(14, 172);
            this.btnDownloadMp4.Name = "btnDownloadMp4";
            this.btnDownloadMp4.Size = new System.Drawing.Size(216, 46);
            this.btnDownloadMp4.TabIndex = 5;
            this.btnDownloadMp4.Text = "Baixar MP4 (Vídeo)";
            this.EstilizarBotaoPrimario(this.btnDownloadMp4);
            this.btnDownloadMp4.Click += new System.EventHandler(this.btnDownloadMp4_Click);
            //
            // btnDownloadMp3
            //
            this.btnDownloadMp3.Location = new System.Drawing.Point(242, 172);
            this.btnDownloadMp3.Name = "btnDownloadMp3";
            this.btnDownloadMp3.Size = new System.Drawing.Size(216, 46);
            this.btnDownloadMp3.TabIndex = 6;
            this.btnDownloadMp3.Text = "Baixar MP3 (Áudio)";
            this.EstilizarBotaoPrimario(this.btnDownloadMp3);
            this.btnDownloadMp3.Click += new System.EventHandler(this.btnDownloadMp3_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.Enabled = false;
            this.btnCancelar.Location = new System.Drawing.Point(470, 172);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(216, 46);
            this.btnCancelar.TabIndex = 7;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.BorderRadius = 10;
            this.btnCancelar.FillColor = CorPerigo;
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnCancelar.HoverState.FillColor = CorPerigoHover;
            this.btnCancelar.DisabledState.FillColor = CorDesabilitadoFundo;
            this.btnCancelar.DisabledState.ForeColor = CorDesabilitadoTexto;
            this.btnCancelar.DisabledState.BorderColor = CorDesabilitadoFundo;
            this.btnCancelar.Animated = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // chkPlaylist
            //
            this.chkPlaylist.AutoSize = true;
            this.chkPlaylist.CheckedState.BorderColor = CorAcento;
            this.chkPlaylist.CheckedState.FillColor = CorAcento;
            this.chkPlaylist.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkPlaylist.ForeColor = CorTexto;
            this.chkPlaylist.Location = new System.Drawing.Point(14, 234);
            this.chkPlaylist.Name = "chkPlaylist";
            this.chkPlaylist.Size = new System.Drawing.Size(300, 22);
            this.chkPlaylist.TabIndex = 8;
            this.chkPlaylist.Text = "Baixar playlist inteira (se o link for de uma playlist)";
            //
            // grpAutenticacao
            //
            this.grpAutenticacao.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.grpAutenticacao.BorderColor = CorBorda;
            this.grpAutenticacao.BorderRadius = 12;
            this.grpAutenticacao.Controls.Add(this.radSemCookies);
            this.grpAutenticacao.Controls.Add(this.radCookiesNavegador);
            this.grpAutenticacao.Controls.Add(this.cmbNavegador);
            this.grpAutenticacao.Controls.Add(this.radCookiesArquivo);
            this.grpAutenticacao.Controls.Add(this.txtCookiesArquivo);
            this.grpAutenticacao.Controls.Add(this.btnSelecionarCookies);
            this.grpAutenticacao.FillColor = CorCartao;
            this.grpAutenticacao.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.grpAutenticacao.ForeColor = CorTexto;
            this.grpAutenticacao.Location = new System.Drawing.Point(14, 272);
            this.grpAutenticacao.Name = "grpAutenticacao";
            this.grpAutenticacao.Size = new System.Drawing.Size(672, 144);
            this.grpAutenticacao.TabIndex = 15;
            this.grpAutenticacao.Text = "Autenticação  (use se o YouTube pedir \"Sign in to confirm you\'re not a bot\")";
            //
            // radSemCookies
            //
            this.radSemCookies.AutoSize = true;
            this.radSemCookies.Checked = true;
            this.radSemCookies.CheckedState.BorderColor = CorAcento;
            this.radSemCookies.CheckedState.FillColor = CorAcento;
            this.radSemCookies.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.radSemCookies.ForeColor = CorTexto;
            this.radSemCookies.Location = new System.Drawing.Point(16, 32);
            this.radSemCookies.Name = "radSemCookies";
            this.radSemCookies.Size = new System.Drawing.Size(196, 22);
            this.radSemCookies.TabIndex = 0;
            this.radSemCookies.Text = "Não usar cookies (padrão)";
            this.radSemCookies.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);
            //
            // radCookiesNavegador
            //
            this.radCookiesNavegador.AutoSize = true;
            this.radCookiesNavegador.CheckedState.BorderColor = CorAcento;
            this.radCookiesNavegador.CheckedState.FillColor = CorAcento;
            this.radCookiesNavegador.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.radCookiesNavegador.ForeColor = CorTexto;
            this.radCookiesNavegador.Location = new System.Drawing.Point(16, 66);
            this.radCookiesNavegador.Name = "radCookiesNavegador";
            this.radCookiesNavegador.Size = new System.Drawing.Size(200, 22);
            this.radCookiesNavegador.TabIndex = 1;
            this.radCookiesNavegador.Text = "Usar cookies do navegador:";
            this.radCookiesNavegador.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);
            //
            // cmbNavegador
            //
            this.cmbNavegador.BackColor = System.Drawing.Color.Transparent;
            this.cmbNavegador.BorderRadius = 8;
            this.cmbNavegador.BorderColor = CorBorda;
            this.cmbNavegador.FillColor = CorCartao;
            this.cmbNavegador.ForeColor = CorTexto;
            this.cmbNavegador.Enabled = false;
            this.cmbNavegador.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbNavegador.Items.AddRange(new object[] {
            "chrome",
            "edge",
            "firefox",
            "brave",
            "opera",
            "vivaldi",
            "chromium"});
            this.cmbNavegador.Location = new System.Drawing.Point(230, 63);
            this.cmbNavegador.Name = "cmbNavegador";
            this.cmbNavegador.Size = new System.Drawing.Size(150, 32);
            this.cmbNavegador.TabIndex = 2;
            //
            // radCookiesArquivo
            //
            this.radCookiesArquivo.AutoSize = true;
            this.radCookiesArquivo.CheckedState.BorderColor = CorAcento;
            this.radCookiesArquivo.CheckedState.FillColor = CorAcento;
            this.radCookiesArquivo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.radCookiesArquivo.ForeColor = CorTexto;
            this.radCookiesArquivo.Location = new System.Drawing.Point(16, 106);
            this.radCookiesArquivo.Name = "radCookiesArquivo";
            this.radCookiesArquivo.Size = new System.Drawing.Size(180, 22);
            this.radCookiesArquivo.TabIndex = 3;
            this.radCookiesArquivo.Text = "Usar arquivo cookies.txt:";
            this.radCookiesArquivo.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);
            //
            // txtCookiesArquivo
            //
            this.txtCookiesArquivo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCookiesArquivo.Enabled = false;
            this.txtCookiesArquivo.Location = new System.Drawing.Point(230, 100);
            this.txtCookiesArquivo.Name = "txtCookiesArquivo";
            this.txtCookiesArquivo.ReadOnly = true;
            this.txtCookiesArquivo.Size = new System.Drawing.Size(322, 32);
            this.txtCookiesArquivo.TabIndex = 4;
            this.EstilizarCampoTexto(this.txtCookiesArquivo);
            //
            // btnSelecionarCookies
            //
            this.btnSelecionarCookies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelecionarCookies.Enabled = false;
            this.btnSelecionarCookies.Location = new System.Drawing.Point(562, 99);
            this.btnSelecionarCookies.Name = "btnSelecionarCookies";
            this.btnSelecionarCookies.Size = new System.Drawing.Size(96, 34);
            this.btnSelecionarCookies.TabIndex = 5;
            this.btnSelecionarCookies.Text = "Selecionar";
            this.EstilizarBotaoSecundario(this.btnSelecionarCookies);
            this.btnSelecionarCookies.Click += new System.EventHandler(this.btnSelecionarCookies_Click);
            //
            // progressBar1
            //
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.BorderRadius = 6;
            this.progressBar1.FillColor = System.Drawing.Color.FromArgb(229, 231, 235);
            this.progressBar1.ProgressColor = CorAcento;
            this.progressBar1.ProgressColor2 = CorAcento;
            this.progressBar1.Location = new System.Drawing.Point(14, 432);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(602, 26);
            this.progressBar1.TabIndex = 16;
            //
            // lblProgressPercent
            //
            this.lblProgressPercent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgressPercent.AutoSize = true;
            this.lblProgressPercent.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.lblProgressPercent.ForeColor = CorTexto;
            this.lblProgressPercent.Location = new System.Drawing.Point(626, 437);
            this.lblProgressPercent.Name = "lblProgressPercent";
            this.lblProgressPercent.Size = new System.Drawing.Size(30, 17);
            this.lblProgressPercent.TabIndex = 17;
            this.lblProgressPercent.Text = "0%";
            //
            // lblStatusTitle
            //
            this.lblStatusTitle.AutoSize = true;
            this.lblStatusTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.lblStatusTitle.ForeColor = CorTexto;
            this.lblStatusTitle.Location = new System.Drawing.Point(14, 472);
            this.lblStatusTitle.Name = "lblStatusTitle";
            this.lblStatusTitle.Size = new System.Drawing.Size(126, 17);
            this.lblStatusTitle.TabIndex = 18;
            this.lblStatusTitle.Text = "Estágio do download:";
            //
            // rtbStatus
            //
            this.rtbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbStatus.BackColor = System.Drawing.Color.FromArgb(30, 32, 38);
            this.rtbStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbStatus.ForeColor = System.Drawing.Color.FromArgb(150, 230, 170);
            this.rtbStatus.Font = new System.Drawing.Font("Consolas", 9F);
            this.rtbStatus.Location = new System.Drawing.Point(14, 500);
            this.rtbStatus.Name = "rtbStatus";
            this.rtbStatus.ReadOnly = true;
            this.rtbStatus.Size = new System.Drawing.Size(672, 200);
            this.rtbStatus.TabIndex = 19;
            this.rtbStatus.Text = "";
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = CorFundo;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ClientSize = new System.Drawing.Size(700, 714);
            this.MinimumSize = new System.Drawing.Size(620, 600);
            this.Controls.Add(this.rtbStatus);
            this.Controls.Add(this.lblStatusTitle);
            this.Controls.Add(this.lblProgressPercent);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.grpAutenticacao);
            this.Controls.Add(this.chkPlaylist);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnDownloadMp3);
            this.Controls.Add(this.btnDownloadMp4);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.txtSavePath);
            this.Controls.Add(this.lblSavePath);
            this.Controls.Add(this.txtUrl);
            this.Controls.Add(this.btnAtualizarYtDlp);
            this.Controls.Add(this.btnHistorico);
            this.Controls.Add(this.lblUrl);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Downloader de Vídeos - yt-dlp GUI";
            this.grpAutenticacao.ResumeLayout(false);
            this.grpAutenticacao.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}

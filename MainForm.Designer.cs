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

        #region Código gerado pelo Designer de Formulários Windows

        private System.Windows.Forms.Label lblUrl;
        private System.Windows.Forms.Button btnHistorico;
        private System.Windows.Forms.Button btnAtualizarYtDlp;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Label lblSavePath;
        private System.Windows.Forms.TextBox txtSavePath;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Button btnDownloadMp4;
        private System.Windows.Forms.Button btnDownloadMp3;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.CheckBox chkPlaylist;
        private System.Windows.Forms.GroupBox grpAutenticacao;
        private System.Windows.Forms.RadioButton radSemCookies;
        private System.Windows.Forms.RadioButton radCookiesNavegador;
        private System.Windows.Forms.ComboBox cmbNavegador;
        private System.Windows.Forms.RadioButton radCookiesArquivo;
        private System.Windows.Forms.TextBox txtCookiesArquivo;
        private System.Windows.Forms.Button btnSelecionarCookies;
        private System.Windows.Forms.Label lblStatusTitle;
        private System.Windows.Forms.RichTextBox rtbStatus;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblProgressPercent;

        /// <summary>
        /// Método necessário para suporte ao Designer.
        /// Não modifique o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblUrl = new System.Windows.Forms.Label();
            this.btnHistorico = new System.Windows.Forms.Button();
            this.btnAtualizarYtDlp = new System.Windows.Forms.Button();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.lblSavePath = new System.Windows.Forms.Label();
            this.txtSavePath = new System.Windows.Forms.TextBox();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.btnDownloadMp4 = new System.Windows.Forms.Button();
            this.btnDownloadMp3 = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.chkPlaylist = new System.Windows.Forms.CheckBox();
            this.grpAutenticacao = new System.Windows.Forms.GroupBox();
            this.radSemCookies = new System.Windows.Forms.RadioButton();
            this.radCookiesNavegador = new System.Windows.Forms.RadioButton();
            this.cmbNavegador = new System.Windows.Forms.ComboBox();
            this.radCookiesArquivo = new System.Windows.Forms.RadioButton();
            this.txtCookiesArquivo = new System.Windows.Forms.TextBox();
            this.btnSelecionarCookies = new System.Windows.Forms.Button();
            this.lblStatusTitle = new System.Windows.Forms.Label();
            this.rtbStatus = new System.Windows.Forms.RichTextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblProgressPercent = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblUrl
            //
            this.lblUrl.AutoSize = true;
            this.lblUrl.Location = new System.Drawing.Point(14, 15);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new System.Drawing.Size(150, 15);
            this.lblUrl.TabIndex = 0;
            this.lblUrl.Text = "URL do vídeo do YouTube:";
            //
            // btnHistorico
            //
            this.btnHistorico.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHistorico.Location = new System.Drawing.Point(482, 10);
            this.btnHistorico.Name = "btnHistorico";
            this.btnHistorico.Size = new System.Drawing.Size(90, 25);
            this.btnHistorico.TabIndex = 13;
            this.btnHistorico.Text = "Histórico";
            this.btnHistorico.UseVisualStyleBackColor = true;
            this.btnHistorico.Click += new System.EventHandler(this.btnHistorico_Click);
            //
            // btnAtualizarYtDlp
            //
            this.btnAtualizarYtDlp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAtualizarYtDlp.Location = new System.Drawing.Point(578, 10);
            this.btnAtualizarYtDlp.Name = "btnAtualizarYtDlp";
            this.btnAtualizarYtDlp.Size = new System.Drawing.Size(96, 25);
            this.btnAtualizarYtDlp.TabIndex = 14;
            this.btnAtualizarYtDlp.Text = "Atualizar yt-dlp";
            this.btnAtualizarYtDlp.UseVisualStyleBackColor = true;
            this.btnAtualizarYtDlp.Click += new System.EventHandler(this.btnAtualizarYtDlp_Click);
            //
            // txtUrl
            //
            this.txtUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUrl.Location = new System.Drawing.Point(14, 33);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.PlaceholderText = "Cole aqui o link do vídeo (ex: https://www.youtube.com/watch?v=...)";
            this.txtUrl.Size = new System.Drawing.Size(660, 23);
            this.txtUrl.TabIndex = 1;
            //
            // lblSavePath
            //
            this.lblSavePath.AutoSize = true;
            this.lblSavePath.Location = new System.Drawing.Point(14, 68);
            this.lblSavePath.Name = "lblSavePath";
            this.lblSavePath.Size = new System.Drawing.Size(110, 15);
            this.lblSavePath.TabIndex = 2;
            this.lblSavePath.Text = "Local de salvamento:";
            //
            // txtSavePath
            //
            this.txtSavePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSavePath.Location = new System.Drawing.Point(14, 86);
            this.txtSavePath.Name = "txtSavePath";
            this.txtSavePath.ReadOnly = true;
            this.txtSavePath.Size = new System.Drawing.Size(558, 23);
            this.txtSavePath.TabIndex = 3;
            //
            // btnSelectFolder
            //
            this.btnSelectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectFolder.Location = new System.Drawing.Point(578, 85);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(96, 25);
            this.btnSelectFolder.TabIndex = 4;
            this.btnSelectFolder.Text = "Selecionar Pasta";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            //
            // btnDownloadMp4
            //
            this.btnDownloadMp4.Location = new System.Drawing.Point(14, 122);
            this.btnDownloadMp4.Name = "btnDownloadMp4";
            this.btnDownloadMp4.Size = new System.Drawing.Size(210, 38);
            this.btnDownloadMp4.TabIndex = 5;
            this.btnDownloadMp4.Text = "Baixar MP4 (Vídeo)";
            this.btnDownloadMp4.UseVisualStyleBackColor = true;
            this.btnDownloadMp4.Click += new System.EventHandler(this.btnDownloadMp4_Click);
            //
            // btnDownloadMp3
            //
            this.btnDownloadMp3.Location = new System.Drawing.Point(240, 122);
            this.btnDownloadMp3.Name = "btnDownloadMp3";
            this.btnDownloadMp3.Size = new System.Drawing.Size(210, 38);
            this.btnDownloadMp3.TabIndex = 6;
            this.btnDownloadMp3.Text = "Baixar MP3 (Áudio)";
            this.btnDownloadMp3.UseVisualStyleBackColor = true;
            this.btnDownloadMp3.Click += new System.EventHandler(this.btnDownloadMp3_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.Enabled = false;
            this.btnCancelar.Location = new System.Drawing.Point(466, 122);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(208, 38);
            this.btnCancelar.TabIndex = 7;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // chkPlaylist
            //
            this.chkPlaylist.AutoSize = true;
            this.chkPlaylist.Location = new System.Drawing.Point(14, 166);
            this.chkPlaylist.Name = "chkPlaylist";
            this.chkPlaylist.Size = new System.Drawing.Size(300, 19);
            this.chkPlaylist.TabIndex = 8;
            this.chkPlaylist.Text = "Baixar playlist inteira (se o link for de uma playlist)";
            this.chkPlaylist.UseVisualStyleBackColor = true;
            //
            // grpAutenticacao
            //
            this.grpAutenticacao.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.grpAutenticacao.Controls.Add(this.radSemCookies);
            this.grpAutenticacao.Controls.Add(this.radCookiesNavegador);
            this.grpAutenticacao.Controls.Add(this.cmbNavegador);
            this.grpAutenticacao.Controls.Add(this.radCookiesArquivo);
            this.grpAutenticacao.Controls.Add(this.txtCookiesArquivo);
            this.grpAutenticacao.Controls.Add(this.btnSelecionarCookies);
            this.grpAutenticacao.Location = new System.Drawing.Point(14, 192);
            this.grpAutenticacao.Name = "grpAutenticacao";
            this.grpAutenticacao.Size = new System.Drawing.Size(660, 112);
            this.grpAutenticacao.TabIndex = 15;
            this.grpAutenticacao.TabStop = false;
            this.grpAutenticacao.Text = "Autenticação (use se o YouTube pedir \"Sign in to confirm you\'re not a bot\")";
            //
            // radSemCookies
            //
            this.radSemCookies.AutoSize = true;
            this.radSemCookies.Checked = true;
            this.radSemCookies.Location = new System.Drawing.Point(10, 22);
            this.radSemCookies.Name = "radSemCookies";
            this.radSemCookies.Size = new System.Drawing.Size(196, 19);
            this.radSemCookies.TabIndex = 0;
            this.radSemCookies.TabStop = true;
            this.radSemCookies.Text = "Não usar cookies (padrão)";
            this.radSemCookies.UseVisualStyleBackColor = true;
            this.radSemCookies.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);
            //
            // radCookiesNavegador
            //
            this.radCookiesNavegador.AutoSize = true;
            this.radCookiesNavegador.Location = new System.Drawing.Point(10, 50);
            this.radCookiesNavegador.Name = "radCookiesNavegador";
            this.radCookiesNavegador.Size = new System.Drawing.Size(160, 19);
            this.radCookiesNavegador.TabIndex = 1;
            this.radCookiesNavegador.Text = "Usar cookies do navegador:";
            this.radCookiesNavegador.UseVisualStyleBackColor = true;
            this.radCookiesNavegador.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);
            //
            // cmbNavegador
            //
            this.cmbNavegador.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNavegador.Enabled = false;
            this.cmbNavegador.Items.AddRange(new object[] {
            "chrome",
            "edge",
            "firefox",
            "brave",
            "opera",
            "vivaldi",
            "chromium"});
            this.cmbNavegador.Location = new System.Drawing.Point(210, 48);
            this.cmbNavegador.Name = "cmbNavegador";
            this.cmbNavegador.Size = new System.Drawing.Size(120, 23);
            this.cmbNavegador.TabIndex = 2;
            //
            // radCookiesArquivo
            //
            this.radCookiesArquivo.AutoSize = true;
            this.radCookiesArquivo.Location = new System.Drawing.Point(10, 80);
            this.radCookiesArquivo.Name = "radCookiesArquivo";
            this.radCookiesArquivo.Size = new System.Drawing.Size(150, 19);
            this.radCookiesArquivo.TabIndex = 3;
            this.radCookiesArquivo.Text = "Usar arquivo cookies.txt:";
            this.radCookiesArquivo.UseVisualStyleBackColor = true;
            this.radCookiesArquivo.CheckedChanged += new System.EventHandler(this.OpcaoDeAutenticacao_CheckedChanged);
            //
            // txtCookiesArquivo
            //
            this.txtCookiesArquivo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCookiesArquivo.Enabled = false;
            this.txtCookiesArquivo.Location = new System.Drawing.Point(210, 78);
            this.txtCookiesArquivo.Name = "txtCookiesArquivo";
            this.txtCookiesArquivo.ReadOnly = true;
            this.txtCookiesArquivo.Size = new System.Drawing.Size(330, 23);
            this.txtCookiesArquivo.TabIndex = 4;
            //
            // btnSelecionarCookies
            //
            this.btnSelecionarCookies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelecionarCookies.Enabled = false;
            this.btnSelecionarCookies.Location = new System.Drawing.Point(550, 77);
            this.btnSelecionarCookies.Name = "btnSelecionarCookies";
            this.btnSelecionarCookies.Size = new System.Drawing.Size(100, 25);
            this.btnSelecionarCookies.TabIndex = 5;
            this.btnSelecionarCookies.Text = "Selecionar...";
            this.btnSelecionarCookies.UseVisualStyleBackColor = true;
            this.btnSelecionarCookies.Click += new System.EventHandler(this.btnSelecionarCookies_Click);
            //
            // progressBar1
            //
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(14, 312);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(590, 20);
            this.progressBar1.TabIndex = 16;
            //
            // lblProgressPercent
            //
            this.lblProgressPercent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgressPercent.AutoSize = true;
            this.lblProgressPercent.Location = new System.Drawing.Point(612, 316);
            this.lblProgressPercent.Name = "lblProgressPercent";
            this.lblProgressPercent.Size = new System.Drawing.Size(30, 15);
            this.lblProgressPercent.TabIndex = 17;
            this.lblProgressPercent.Text = "0%";
            //
            // lblStatusTitle
            //
            this.lblStatusTitle.AutoSize = true;
            this.lblStatusTitle.Location = new System.Drawing.Point(14, 342);
            this.lblStatusTitle.Name = "lblStatusTitle";
            this.lblStatusTitle.Size = new System.Drawing.Size(101, 15);
            this.lblStatusTitle.TabIndex = 18;
            this.lblStatusTitle.Text = "Estágio do download:";
            //
            // rtbStatus
            //
            this.rtbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbStatus.BackColor = System.Drawing.Color.Black;
            this.rtbStatus.ForeColor = System.Drawing.Color.LightGreen;
            this.rtbStatus.Font = new System.Drawing.Font("Consolas", 9F);
            this.rtbStatus.Location = new System.Drawing.Point(14, 360);
            this.rtbStatus.Name = "rtbStatus";
            this.rtbStatus.ReadOnly = true;
            this.rtbStatus.Size = new System.Drawing.Size(660, 226);
            this.rtbStatus.TabIndex = 19;
            this.rtbStatus.Text = "";
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 597);
            this.MinimumSize = new System.Drawing.Size(600, 520);
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
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}

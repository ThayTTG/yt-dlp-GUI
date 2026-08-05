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
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Label lblSavePath;
        private System.Windows.Forms.TextBox txtSavePath;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Button btnDownloadMp4;
        private System.Windows.Forms.Button btnDownloadMp3;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.CheckBox chkPlaylist;
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
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.lblSavePath = new System.Windows.Forms.Label();
            this.txtSavePath = new System.Windows.Forms.TextBox();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.btnDownloadMp4 = new System.Windows.Forms.Button();
            this.btnDownloadMp3 = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.chkPlaylist = new System.Windows.Forms.CheckBox();
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
            // progressBar1
            //
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(14, 200);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(590, 20);
            this.progressBar1.TabIndex = 9;
            //
            // lblProgressPercent
            //
            this.lblProgressPercent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgressPercent.AutoSize = true;
            this.lblProgressPercent.Location = new System.Drawing.Point(612, 204);
            this.lblProgressPercent.Name = "lblProgressPercent";
            this.lblProgressPercent.Size = new System.Drawing.Size(30, 15);
            this.lblProgressPercent.TabIndex = 10;
            this.lblProgressPercent.Text = "0%";
            //
            // lblStatusTitle
            //
            this.lblStatusTitle.AutoSize = true;
            this.lblStatusTitle.Location = new System.Drawing.Point(14, 230);
            this.lblStatusTitle.Name = "lblStatusTitle";
            this.lblStatusTitle.Size = new System.Drawing.Size(101, 15);
            this.lblStatusTitle.TabIndex = 11;
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
            this.rtbStatus.Location = new System.Drawing.Point(14, 248);
            this.rtbStatus.Name = "rtbStatus";
            this.rtbStatus.ReadOnly = true;
            this.rtbStatus.Size = new System.Drawing.Size(660, 226);
            this.rtbStatus.TabIndex = 12;
            this.rtbStatus.Text = "";
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 485);
            this.MinimumSize = new System.Drawing.Size(600, 420);
            this.Controls.Add(this.rtbStatus);
            this.Controls.Add(this.lblStatusTitle);
            this.Controls.Add(this.lblProgressPercent);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.chkPlaylist);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnDownloadMp3);
            this.Controls.Add(this.btnDownloadMp4);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.txtSavePath);
            this.Controls.Add(this.lblSavePath);
            this.Controls.Add(this.txtUrl);
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

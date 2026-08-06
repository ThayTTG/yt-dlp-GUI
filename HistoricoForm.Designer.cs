namespace YtDlpGui
{
    partial class HistoricoForm
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

        private System.Windows.Forms.ListView listViewHistorico;
        private System.Windows.Forms.ColumnHeader colData;
        private System.Windows.Forms.ColumnHeader colTipo;
        private System.Windows.Forms.ColumnHeader colTitulo;
        private System.Windows.Forms.ColumnHeader colPasta;
        private System.Windows.Forms.Button btnAbrirPasta;
        private System.Windows.Forms.Button btnLimparHistorico;
        private System.Windows.Forms.Button btnFechar;
        private System.Windows.Forms.Label lblVazio;

        private void InitializeComponent()
        {
            this.listViewHistorico = new System.Windows.Forms.ListView();
            this.colData = new System.Windows.Forms.ColumnHeader();
            this.colTipo = new System.Windows.Forms.ColumnHeader();
            this.colTitulo = new System.Windows.Forms.ColumnHeader();
            this.colPasta = new System.Windows.Forms.ColumnHeader();
            this.btnAbrirPasta = new System.Windows.Forms.Button();
            this.btnLimparHistorico = new System.Windows.Forms.Button();
            this.btnFechar = new System.Windows.Forms.Button();
            this.lblVazio = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // listViewHistorico
            //
            this.listViewHistorico.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewHistorico.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colData,
            this.colTipo,
            this.colTitulo,
            this.colPasta});
            this.listViewHistorico.FullRowSelect = true;
            this.listViewHistorico.GridLines = true;
            this.listViewHistorico.HideSelection = false;
            this.listViewHistorico.Location = new System.Drawing.Point(12, 12);
            this.listViewHistorico.MultiSelect = false;
            this.listViewHistorico.Name = "listViewHistorico";
            this.listViewHistorico.Size = new System.Drawing.Size(660, 360);
            this.listViewHistorico.TabIndex = 0;
            this.listViewHistorico.UseCompatibleStateImageBehavior = false;
            this.listViewHistorico.View = System.Windows.Forms.View.Details;
            //
            // colData
            //
            this.colData.Text = "Data/Hora";
            this.colData.Width = 120;
            //
            // colTipo
            //
            this.colTipo.Text = "Tipo";
            this.colTipo.Width = 60;
            //
            // colTitulo
            //
            this.colTitulo.Text = "Arquivo";
            this.colTitulo.Width = 260;
            //
            // colPasta
            //
            this.colPasta.Text = "Pasta";
            this.colPasta.Width = 210;
            //
            // lblVazio
            //
            this.lblVazio.AutoSize = true;
            this.lblVazio.ForeColor = System.Drawing.Color.Gray;
            this.lblVazio.Location = new System.Drawing.Point(17, 40);
            this.lblVazio.Name = "lblVazio";
            this.lblVazio.Size = new System.Drawing.Size(280, 15);
            this.lblVazio.TabIndex = 4;
            this.lblVazio.Text = "Nenhum download foi registrado ainda.";
            this.lblVazio.Visible = false;
            //
            // btnAbrirPasta
            //
            this.btnAbrirPasta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAbrirPasta.Location = new System.Drawing.Point(12, 384);
            this.btnAbrirPasta.Name = "btnAbrirPasta";
            this.btnAbrirPasta.Size = new System.Drawing.Size(150, 30);
            this.btnAbrirPasta.TabIndex = 1;
            this.btnAbrirPasta.Text = "Abrir Pasta do Item";
            this.btnAbrirPasta.UseVisualStyleBackColor = true;
            this.btnAbrirPasta.Click += new System.EventHandler(this.btnAbrirPasta_Click);
            //
            // btnLimparHistorico
            //
            this.btnLimparHistorico.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLimparHistorico.Location = new System.Drawing.Point(168, 384);
            this.btnLimparHistorico.Name = "btnLimparHistorico";
            this.btnLimparHistorico.Size = new System.Drawing.Size(150, 30);
            this.btnLimparHistorico.TabIndex = 2;
            this.btnLimparHistorico.Text = "Limpar Histórico";
            this.btnLimparHistorico.UseVisualStyleBackColor = true;
            this.btnLimparHistorico.Click += new System.EventHandler(this.btnLimparHistorico_Click);
            //
            // btnFechar
            //
            this.btnFechar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFechar.Location = new System.Drawing.Point(596, 384);
            this.btnFechar.Name = "btnFechar";
            this.btnFechar.Size = new System.Drawing.Size(76, 30);
            this.btnFechar.TabIndex = 3;
            this.btnFechar.Text = "Fechar";
            this.btnFechar.UseVisualStyleBackColor = true;
            this.btnFechar.Click += new System.EventHandler(this.btnFechar_Click);
            //
            // HistoricoForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 426);
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Controls.Add(this.lblVazio);
            this.Controls.Add(this.btnFechar);
            this.Controls.Add(this.btnLimparHistorico);
            this.Controls.Add(this.btnAbrirPasta);
            this.Controls.Add(this.listViewHistorico);
            this.Name = "HistoricoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Histórico de Downloads";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}

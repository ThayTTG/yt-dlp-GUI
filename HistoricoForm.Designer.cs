using Guna.UI2.WinForms;

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

        // Mesma paleta usada na MainForm, para manter consistência visual.
        private static readonly System.Drawing.Color CorFundo = System.Drawing.Color.FromArgb(247, 248, 250);
        private static readonly System.Drawing.Color CorCartao = System.Drawing.Color.White;
        private static readonly System.Drawing.Color CorBorda = System.Drawing.Color.FromArgb(225, 228, 232);
        private static readonly System.Drawing.Color CorTexto = System.Drawing.Color.FromArgb(31, 41, 55);
        private static readonly System.Drawing.Color CorAcento = System.Drawing.Color.FromArgb(47, 111, 237);
        private static readonly System.Drawing.Color CorPerigo = System.Drawing.Color.FromArgb(239, 68, 68);
        private static readonly System.Drawing.Color CorPerigoHover = System.Drawing.Color.FromArgb(220, 38, 38);
        private static readonly System.Drawing.Color CorBotaoSecundario = System.Drawing.Color.FromArgb(243, 244, 246);

        private System.Windows.Forms.ListView listViewHistorico;
        private System.Windows.Forms.ColumnHeader colData;
        private System.Windows.Forms.ColumnHeader colTipo;
        private System.Windows.Forms.ColumnHeader colTitulo;
        private System.Windows.Forms.ColumnHeader colPasta;
        private Guna2Button btnAbrirPasta;
        private Guna2Button btnLimparHistorico;
        private Guna2Button btnFechar;
        private System.Windows.Forms.Label lblVazio;

        private void InitializeComponent()
        {
            this.listViewHistorico = new System.Windows.Forms.ListView();
            this.colData = new System.Windows.Forms.ColumnHeader();
            this.colTipo = new System.Windows.Forms.ColumnHeader();
            this.colTitulo = new System.Windows.Forms.ColumnHeader();
            this.colPasta = new System.Windows.Forms.ColumnHeader();
            this.btnAbrirPasta = new Guna2Button();
            this.btnLimparHistorico = new Guna2Button();
            this.btnFechar = new Guna2Button();
            this.lblVazio = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // listViewHistorico
            //
            this.listViewHistorico.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewHistorico.BackColor = CorCartao;
            this.listViewHistorico.ForeColor = CorTexto;
            this.listViewHistorico.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.listViewHistorico.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewHistorico.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colData,
            this.colTipo,
            this.colTitulo,
            this.colPasta});
            this.listViewHistorico.FullRowSelect = true;
            this.listViewHistorico.GridLines = true;
            this.listViewHistorico.HideSelection = false;
            this.listViewHistorico.Location = new System.Drawing.Point(16, 16);
            this.listViewHistorico.MultiSelect = false;
            this.listViewHistorico.Name = "listViewHistorico";
            this.listViewHistorico.Size = new System.Drawing.Size(652, 360);
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
            this.colPasta.Width = 200;
            //
            // lblVazio
            //
            this.lblVazio.AutoSize = true;
            this.lblVazio.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblVazio.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblVazio.Location = new System.Drawing.Point(21, 44);
            this.lblVazio.Name = "lblVazio";
            this.lblVazio.Size = new System.Drawing.Size(280, 15);
            this.lblVazio.TabIndex = 4;
            this.lblVazio.Text = "Nenhum download foi registrado ainda.";
            this.lblVazio.Visible = false;
            //
            // btnAbrirPasta
            //
            this.btnAbrirPasta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAbrirPasta.BorderRadius = 8;
            this.btnAbrirPasta.FillColor = CorBotaoSecundario;
            this.btnAbrirPasta.ForeColor = CorTexto;
            this.btnAbrirPasta.BorderColor = CorBorda;
            this.btnAbrirPasta.BorderThickness = 1;
            this.btnAbrirPasta.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnAbrirPasta.Location = new System.Drawing.Point(16, 388);
            this.btnAbrirPasta.Name = "btnAbrirPasta";
            this.btnAbrirPasta.Size = new System.Drawing.Size(160, 34);
            this.btnAbrirPasta.TabIndex = 1;
            this.btnAbrirPasta.Text = "Abrir Pasta do Item";
            this.btnAbrirPasta.Click += new System.EventHandler(this.btnAbrirPasta_Click);
            //
            // btnLimparHistorico
            //
            this.btnLimparHistorico.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLimparHistorico.BorderRadius = 8;
            this.btnLimparHistorico.FillColor = CorPerigo;
            this.btnLimparHistorico.ForeColor = System.Drawing.Color.White;
            this.btnLimparHistorico.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnLimparHistorico.HoverState.FillColor = CorPerigoHover;
            this.btnLimparHistorico.Location = new System.Drawing.Point(184, 388);
            this.btnLimparHistorico.Name = "btnLimparHistorico";
            this.btnLimparHistorico.Size = new System.Drawing.Size(150, 34);
            this.btnLimparHistorico.TabIndex = 2;
            this.btnLimparHistorico.Text = "Limpar Histórico";
            this.btnLimparHistorico.Click += new System.EventHandler(this.btnLimparHistorico_Click);
            //
            // btnFechar
            //
            this.btnFechar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFechar.BorderRadius = 8;
            this.btnFechar.FillColor = CorAcento;
            this.btnFechar.ForeColor = System.Drawing.Color.White;
            this.btnFechar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnFechar.Location = new System.Drawing.Point(588, 388);
            this.btnFechar.Name = "btnFechar";
            this.btnFechar.Size = new System.Drawing.Size(80, 34);
            this.btnFechar.TabIndex = 3;
            this.btnFechar.Text = "Fechar";
            this.btnFechar.Click += new System.EventHandler(this.btnFechar_Click);
            //
            // HistoricoForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = CorFundo;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ClientSize = new System.Drawing.Size(684, 438);
            this.MinimumSize = new System.Drawing.Size(500, 320);
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

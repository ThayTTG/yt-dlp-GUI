using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace YtDlpGui
{
    /// <summary>
    /// Janela auxiliar que exibe o histórico de downloads já realizados,
    /// permitindo abrir a pasta de destino de um item ou limpar todo o histórico.
    /// </summary>
    public partial class HistoricoForm : Form
    {
        public HistoricoForm()
        {
            InitializeComponent();
            CarregarLista();
        }

        /// <summary>
        /// Lê o histórico salvo em disco e preenche a lista da tela.
        /// </summary>
        private void CarregarLista()
        {
            listViewHistorico.Items.Clear();

            var itens = GerenciadorHistorico.Carregar();

            foreach (var item in itens)
            {
                var linha = new ListViewItem(item.DataHora.ToString("dd/MM/yyyy HH:mm"));
                linha.SubItems.Add(item.Tipo);
                linha.SubItems.Add(item.Titulo);
                linha.SubItems.Add(item.Pasta);
                linha.Tag = item;
                listViewHistorico.Items.Add(linha);
            }

            lblVazio.Visible = itens.Count == 0;
        }

        /// <summary>
        /// Abre no Explorer a pasta onde o item selecionado foi salvo.
        /// </summary>
        private void btnAbrirPasta_Click(object sender, EventArgs e)
        {
            if (listViewHistorico.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "Selecione um item da lista primeiro.",
                    "Nenhum item selecionado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (listViewHistorico.SelectedItems[0].Tag is HistoricoItem item)
            {
                if (!Directory.Exists(item.Pasta))
                {
                    MessageBox.Show(this, "A pasta deste item não existe mais no local original.",
                        "Pasta não encontrada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Abre o Explorer diretamente na pasta salva.
                Process.Start(new ProcessStartInfo
                {
                    FileName = item.Pasta,
                    UseShellExecute = true
                });
            }
        }

        /// <summary>
        /// Apaga todo o histórico salvo, após confirmação do usuário.
        /// </summary>
        private void btnLimparHistorico_Click(object sender, EventArgs e)
        {
            if (listViewHistorico.Items.Count == 0)
                return;

            var resultado = MessageBox.Show(this,
                "Tem certeza que deseja apagar todo o histórico de downloads?\nEsta ação não pode ser desfeita.",
                "Confirmar limpeza", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                GerenciadorHistorico.Limpar();
                CarregarLista();
            }
        }

        private void btnFechar_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

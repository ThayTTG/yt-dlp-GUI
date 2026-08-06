using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace YtDlpGui
{
    /// <summary>
    /// Representa um registro de download concluído, salvo no histórico.
    /// </summary>
    public class HistoricoItem
    {
        public DateTime DataHora { get; set; }
        public string Tipo { get; set; } = "";     // "MP4" ou "MP3"
        public string Titulo { get; set; } = "";   // nome do arquivo final, quando identificado
        public string Pasta { get; set; } = "";    // pasta onde o arquivo foi salvo
        public string Url { get; set; } = "";      // URL original informada pelo usuário
    }

    /// <summary>
    /// Responsável por carregar, adicionar e limpar o histórico de downloads,
    /// persistido em um arquivo JSON dentro da pasta de dados do usuário
    /// (%AppData%\YtDlpGui\historico.json). Assim, o histórico não se perde
    /// entre execuções do programa e não exige nenhum banco de dados externo.
    /// </summary>
    public static class GerenciadorHistorico
    {
        private const int MaximoDeRegistros = 200;

        /// <summary>
        /// Retorna (e cria, se necessário) o caminho do arquivo de histórico.
        /// </summary>
        private static string ObterCaminhoArquivo()
        {
            string pastaDados = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "YtDlpGui");

            Directory.CreateDirectory(pastaDados);
            return Path.Combine(pastaDados, "historico.json");
        }

        /// <summary>
        /// Carrega todos os itens salvos. Retorna lista vazia se o arquivo
        /// não existir ou estiver corrompido (nunca lança exceção).
        /// </summary>
        public static List<HistoricoItem> Carregar()
        {
            try
            {
                string caminho = ObterCaminhoArquivo();
                if (!File.Exists(caminho))
                    return new List<HistoricoItem>();

                string json = File.ReadAllText(caminho);
                return JsonSerializer.Deserialize<List<HistoricoItem>>(json) ?? new List<HistoricoItem>();
            }
            catch
            {
                // Um arquivo corrompido não deve travar o aplicativo:
                // simplesmente tratamos como histórico vazio.
                return new List<HistoricoItem>();
            }
        }

        /// <summary>
        /// Adiciona um novo item ao topo do histórico e persiste no disco,
        /// mantendo no máximo <see cref="MaximoDeRegistros"/> registros.
        /// </summary>
        public static void Adicionar(HistoricoItem item)
        {
            try
            {
                var lista = Carregar();
                lista.Insert(0, item);

                if (lista.Count > MaximoDeRegistros)
                {
                    lista = lista.Take(MaximoDeRegistros).ToList();
                }

                string json = JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ObterCaminhoArquivo(), json);
            }
            catch
            {
                // Falha ao salvar histórico não deve interromper o fluxo de download.
            }
        }

        /// <summary>
        /// Apaga todo o histórico salvo.
        /// </summary>
        public static void Limpar()
        {
            try
            {
                string caminho = ObterCaminhoArquivo();
                if (File.Exists(caminho))
                {
                    File.Delete(caminho);
                }
            }
            catch
            {
                // Ignora falhas ao apagar (ex.: arquivo em uso).
            }
        }
    }
}

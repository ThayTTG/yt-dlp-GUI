using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    ///
    /// Proteções de integridade:
    ///   - Gravação atômica: o JSON novo é escrito num arquivo temporário e só
    ///     então troca o antigo. Se o programa cair ou faltar energia no meio da
    ///     gravação, o histórico antigo continua intacto (antes, o arquivo podia
    ///     ficar pela metade, ou seja, corrompido).
    ///   - Quarentena: se o arquivo estiver corrompido, ele NÃO é mais descartado
    ///     em silêncio e sobrescrito; é renomeado para "historico.corrompido-*.json"
    ///     (guardamos os 5 mais recentes) e o histórico recomeça vazio.
    ///   - Trava (lock) contra duas gravações simultâneas dentro do mesmo programa.
    ///   - Todo problema vai para o log de erros.
    /// </summary>
    public static class GerenciadorHistorico
    {
        private const int MaximoDeRegistros = 200;
        private const int MaximoDeArquivosEmQuarentena = 5;

        private static readonly object Trava = new();
        private static readonly JsonSerializerOptions OpcoesJson = new() { WriteIndented = true };

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
            lock (Trava)
            {
                return CarregarInterno(out _);
            }
        }

        // Deve ser chamado dentro do lock.
        private static List<HistoricoItem> CarregarInterno(out string? reparoFeito)
        {
            reparoFeito = null;

            try
            {
                string caminho = ObterCaminhoArquivo();
                if (!File.Exists(caminho))
                    return new List<HistoricoItem>();

                string json = File.ReadAllText(caminho);
                if (string.IsNullOrWhiteSpace(json))
                    return new List<HistoricoItem>();

                try
                {
                    return JsonSerializer.Deserialize<List<HistoricoItem>>(json) ?? new List<HistoricoItem>();
                }
                catch (JsonException ex)
                {
                    // Arquivo corrompido: preserva uma cópia para análise e recomeça vazio.
                    GerenciadorLog.Erro("O arquivo de histórico está corrompido", ex);
                    reparoFeito = ColocarEmQuarentena(caminho);
                    return new List<HistoricoItem>();
                }
            }
            catch (Exception ex)
            {
                // Um problema de leitura não deve travar o aplicativo.
                GerenciadorLog.Erro("Não foi possível ler o histórico", ex);
                return new List<HistoricoItem>();
            }
        }

        // Deve ser chamado dentro do lock.
        private static string? ColocarEmQuarentena(string caminho)
        {
            try
            {
                string pasta = Path.GetDirectoryName(caminho) ?? "";
                string destino = Path.Combine(pasta,
                    "historico.corrompido-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".json");

                File.Move(caminho, destino, overwrite: true);

                // Mantém só os mais recentes, para a quarentena não encher o disco.
                var antigos = new DirectoryInfo(pasta)
                    .GetFiles("historico.corrompido-*.json")
                    .OrderByDescending(f => f.Name)
                    .Skip(MaximoDeArquivosEmQuarentena);
                foreach (var arquivo in antigos)
                {
                    try { arquivo.Delete(); } catch { }
                }

                string mensagem = "Histórico corrompido guardado em '" + destino + "'; um histórico novo foi iniciado.";
                GerenciadorLog.Reparo(mensagem);
                return mensagem;
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Não foi possível colocar o histórico corrompido em quarentena", ex);
                return null;
            }
        }

        /// <summary>
        /// Verificação de integridade usada pelo autodiagnóstico. Se o histórico
        /// estiver corrompido, faz o reparo e devolve uma descrição; senão, null.
        /// </summary>
        public static string? VerificarEReparar()
        {
            lock (Trava)
            {
                CarregarInterno(out string? reparo);
                return reparo;
            }
        }

        /// <summary>
        /// Adiciona um novo item ao topo do histórico e persiste no disco,
        /// mantendo no máximo <see cref="MaximoDeRegistros"/> registros.
        /// </summary>
        public static void Adicionar(HistoricoItem item)
        {
            AdicionarVarios(new[] { item });
        }

        /// <summary>
        /// Adiciona vários itens de uma vez (uma única leitura e uma única gravação
        /// no disco — antes, uma playlist com 50 vídeos reescrevia o arquivo 50 vezes).
        /// Cada item é inserido no topo, como no método <see cref="Adicionar"/>.
        /// </summary>
        public static void AdicionarVarios(IEnumerable<HistoricoItem> itens)
        {
            try
            {
                lock (Trava)
                {
                    var lista = CarregarInterno(out _);

                    foreach (var item in itens)
                        lista.Insert(0, item);

                    if (lista.Count > MaximoDeRegistros)
                        lista = lista.Take(MaximoDeRegistros).ToList();

                    SalvarComSeguranca(ObterCaminhoArquivo(), lista);
                }
            }
            catch (Exception ex)
            {
                // Falha ao salvar histórico não deve interromper o fluxo de download.
                GerenciadorLog.Erro("Não foi possível salvar o histórico", ex);
            }
        }

        // Gravação atômica: escreve num temporário e troca pelo arquivo definitivo.
        private static void SalvarComSeguranca(string caminho, List<HistoricoItem> lista)
        {
            string json = JsonSerializer.Serialize(lista, OpcoesJson);
            string temporario = caminho + ".tmp";

            File.WriteAllText(temporario, json, new UTF8Encoding(false));

            if (File.Exists(caminho))
                File.Replace(temporario, caminho, null);
            else
                File.Move(temporario, caminho);
        }

        /// <summary>
        /// Apaga todo o histórico salvo.
        /// </summary>
        public static void Limpar()
        {
            try
            {
                lock (Trava)
                {
                    string caminho = ObterCaminhoArquivo();
                    if (File.Exists(caminho))
                    {
                        File.Delete(caminho);
                    }
                }
            }
            catch (Exception ex)
            {
                // Ex.: arquivo em uso. Não interrompe o app, mas fica registrado.
                GerenciadorLog.Erro("Não foi possível apagar o histórico", ex);
            }
        }
    }
}

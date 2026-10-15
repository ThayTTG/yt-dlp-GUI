using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YtDlpGui
{
    public enum SituacaoVerificacao
    {
        Ok,
        Corrigido,
        Aviso,
        Falha
    }

    public sealed record ResultadoVerificacao(string Item, SituacaoVerificacao Situacao, string Detalhe);

    /// <summary>
    /// Autodiagnóstico e autorreparo do programa ("debug interno").
    ///
    /// O que ele faz SOZINHO e com segurança:
    ///   - Detecta se a sessão anterior terminou de forma inesperada (travamento,
    ///     queda de energia, fechamento forçado) e registra isso no log.
    ///   - Garante que o log tenha onde gravar (plano B se a pasta for protegida).
    ///   - Conserta um histórico corrompido (guarda uma cópia e recomeça).
    ///   - Verifica yt-dlp.exe, ffmpeg.exe, pasta Assets e o filtro de conteúdo.
    ///   - Encerra processos do yt-dlp que ficaram "órfãos".
    ///
    /// O que ele só faz COM a permissão de quem usa (pergunta antes):
    ///   - Baixar um yt-dlp.exe novo quando o atual sumiu ou está danificado,
    ///     direto do GitHub oficial do yt-dlp, conferindo o hash SHA-256 publicado
    ///     na própria release antes de colocar o arquivo em uso.
    ///
    /// O que ele faz automaticamente uma vez por sessão, de forma visível na tela:
    ///   - Se um download falhar por um erro típico de yt-dlp desatualizado, atualiza
    ///     o yt-dlp e tenta o mesmo download de novo (uma única vez, sem laços).
    /// </summary>
    public static class AutoReparo
    {
        private const long TamanhoMinimoYtDlp = 1_000_000;   // o real tem dezenas de MB
        private const long TamanhoMinimoFfmpeg = 1_000_000;  // o real tem dezenas de MB

        private static readonly string[] ImagensEsperadas =
        {
            "logo_app.png", "ilustracao_pasta.png",
            "ic_home_muted.png", "ic_home_branco.png", "ic_home_roxo.png",
            "ic_historico_muted.png", "ic_historico_branco.png", "ic_historico_roxo.png",
            "ic_pasta_muted.png", "ic_pasta_branco.png", "ic_pasta_roxo.png",
            "ic_config_muted.png", "ic_config_branco.png", "ic_config_roxo.png",
            "ic_link_muted.png", "ic_video_branco.png", "ic_musica_roxo.png",
            "ic_lixeira_rosa.png", "ic_abrir_branco.png", "ic_atualizar_branco.png", "ic_cookie_roxo.png"
        };

        private static string? _caminhoTrava;

        // ---------------------------------------------------------------
        // Controle de sessão (detecção de encerramento inesperado)
        // ---------------------------------------------------------------

        /// <summary>
        /// Deve ser chamado o mais cedo possível em Main(). Cria um arquivo
        /// "sessao-PC-PID.lock" que só é apagado quando o programa fecha
        /// normalmente. Se na próxima abertura sobrar um desses arquivos sem
        /// um programa vivo por trás, a sessão anterior caiu — e isso vai ao log.
        /// </summary>
        public static void IniciarSessao()
        {
            try
            {
                // Esta primeira gravação também decide qual pasta de logs vai valer (com plano B).
                GerenciadorLog.Atividade("Programa iniciado.");

                string pasta = GerenciadorLog.PastaAtual;
                string prefixo = "sessao-" + Environment.MachineName + "-";
                int sessoesQueCairam = 0;

                if (Directory.Exists(pasta))
                {
                    foreach (string trava in Directory.GetFiles(pasta, prefixo + "*.lock"))
                    {
                        string nome = Path.GetFileNameWithoutExtension(trava);
                        string sufixo = nome.Substring(prefixo.Length);

                        if (!int.TryParse(sufixo, out int pid))
                            continue;
                        if (pid == Environment.ProcessId)
                            continue;
                        if (ProcessoDoProgramaEstaVivo(pid))
                            continue; // outra janela do programa ainda aberta

                        sessoesQueCairam++;
                        try { File.Delete(trava); } catch { }
                    }
                }

                _caminhoTrava = Path.Combine(pasta, prefixo + Environment.ProcessId + ".lock");
                File.WriteAllText(_caminhoTrava, DateTime.Now.ToString("o"));

                if (sessoesQueCairam > 0)
                {
                    GerenciadorLog.Aviso(
                        "A sessão anterior terminou de forma inesperada (travamento, queda de energia ou " +
                        "fechamento forçado). Veja se há erros registrados logo antes disso neste arquivo.");
                }
            }
            catch (Exception ex)
            {
                GerenciadorLog.Aviso("Não foi possível registrar o controle de sessão: " + ex.Message);
            }
        }

        /// <summary>Chamado quando o programa fecha normalmente.</summary>
        public static void MarcarEncerramentoLimpo()
        {
            try
            {
                if (_caminhoTrava is not null && File.Exists(_caminhoTrava))
                    File.Delete(_caminhoTrava);

                _caminhoTrava = null;
                GerenciadorLog.Atividade("Programa encerrado normalmente.");
            }
            catch
            {
                // Sem importância: no pior caso a próxima abertura registra um aviso à toa.
            }
        }

        private static bool ProcessoDoProgramaEstaVivo(int pid)
        {
            try
            {
                using var processo = Process.GetProcessById(pid);
                if (processo.HasExited)
                    return false;

                using var atual = Process.GetCurrentProcess();
                return string.Equals(processo.ProcessName, atual.ProcessName, StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException)
            {
                return false; // não existe processo com esse número
            }
            catch
            {
                return true; // sem permissão para inspecionar: por segurança, não mexe
            }
        }

        // ---------------------------------------------------------------
        // Verificação inicial
        // ---------------------------------------------------------------

        public static List<ResultadoVerificacao> VerificarInicio(string caminhoYtDlp, string caminhoFfmpeg, string pastaAssets)
        {
            var resultados = new List<ResultadoVerificacao>
            {
                Tentar("Pasta de logs", VerificarPastaDeLogs),
                Tentar("yt-dlp.exe", () => VerificarExecutavel("yt-dlp.exe", caminhoYtDlp, TamanhoMinimoYtDlp, obrigatorio: true)),
                Tentar("ffmpeg.exe", () => VerificarExecutavel("ffmpeg.exe", caminhoFfmpeg, TamanhoMinimoFfmpeg, obrigatorio: false)),
                Tentar("Imagens (Assets)", () => VerificarAssets(pastaAssets)),
                Tentar("Histórico", VerificarHistorico),
                Tentar("Filtro de conteúdo", VerificarFiltro)
            };

            foreach (var r in resultados)
            {
                string texto = r.Item + ": " + r.Detalhe;
                switch (r.Situacao)
                {
                    case SituacaoVerificacao.Ok:
                        GerenciadorLog.Atividade("Verificação OK - " + texto);
                        break;
                    case SituacaoVerificacao.Corrigido:
                        // O reparo em si já foi registrado por quem o executou.
                        GerenciadorLog.Atividade("Verificação (corrigido) - " + texto);
                        break;
                    case SituacaoVerificacao.Aviso:
                        GerenciadorLog.Aviso("Verificação - " + texto);
                        break;
                    default:
                        GerenciadorLog.Erro("Verificação - " + texto);
                        break;
                }
            }

            return resultados;
        }

        /// <summary>Resumo legível dos resultados, para exibir ao usuário.</summary>
        public static string GerarResumo(IEnumerable<ResultadoVerificacao> resultados)
        {
            var sb = new StringBuilder();
            foreach (var r in resultados)
            {
                string marca = r.Situacao switch
                {
                    SituacaoVerificacao.Ok => "[OK]",
                    SituacaoVerificacao.Corrigido => "[CORRIGIDO]",
                    SituacaoVerificacao.Aviso => "[AVISO]",
                    _ => "[PROBLEMA]"
                };
                sb.AppendLine(marca + " " + r.Item + " - " + r.Detalhe);
            }
            return sb.ToString();
        }

        private static ResultadoVerificacao Tentar(string item, Func<ResultadoVerificacao> verificacao)
        {
            try
            {
                return verificacao();
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("A verificação '" + item + "' falhou", ex);
                return new ResultadoVerificacao(item, SituacaoVerificacao.Falha, "não foi possível verificar (" + ex.Message + ")");
            }
        }

        private static ResultadoVerificacao VerificarPastaDeLogs()
        {
            string atual = GerenciadorLog.PastaAtual;
            string origem = GerenciadorLog.PastaDeOrigem;

            if (string.Equals(Path.GetFullPath(atual), Path.GetFullPath(origem), StringComparison.OrdinalIgnoreCase))
                return new ResultadoVerificacao("Pasta de logs", SituacaoVerificacao.Ok, "gravando na pasta do programa (" + atual + ")");

            return new ResultadoVerificacao("Pasta de logs", SituacaoVerificacao.Corrigido,
                "a pasta do programa não permite gravação; os logs foram para '" + atual + "'");
        }

        private static ResultadoVerificacao VerificarExecutavel(string nome, string caminho, long tamanhoMinimo, bool obrigatorio)
        {
            if (!File.Exists(caminho))
            {
                return new ResultadoVerificacao(nome,
                    obrigatorio ? SituacaoVerificacao.Falha : SituacaoVerificacao.Aviso,
                    obrigatorio
                        ? "não encontrado na pasta do programa"
                        : "não encontrado (MP3 não funciona e alguns MP4 saem em qualidade menor)");
            }

            long tamanho = new FileInfo(caminho).Length;
            if (tamanho < tamanhoMinimo)
            {
                return new ResultadoVerificacao(nome, SituacaoVerificacao.Aviso,
                    "o arquivo existe mas está pequeno demais (" + tamanho + " bytes); pode estar corrompido ou incompleto");
            }

            // Todo executável do Windows começa com as letras "MZ".
            using (var fs = new FileStream(caminho, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int a = fs.ReadByte();
                int b = fs.ReadByte();
                if (a != 'M' || b != 'Z')
                {
                    return new ResultadoVerificacao(nome, SituacaoVerificacao.Aviso,
                        "o arquivo não parece um executável válido do Windows (pode estar corrompido)");
                }
            }

            return new ResultadoVerificacao(nome, SituacaoVerificacao.Ok, "encontrado (" + (tamanho / 1024 / 1024) + " MB)");
        }

        private static ResultadoVerificacao VerificarAssets(string pastaAssets)
        {
            if (!Directory.Exists(pastaAssets))
            {
                return new ResultadoVerificacao("Imagens (Assets)", SituacaoVerificacao.Aviso,
                    "a pasta Assets não foi encontrada; o programa funciona, mas sem os ícones das telas");
            }

            var faltando = new List<string>();
            foreach (string imagem in ImagensEsperadas)
            {
                if (!File.Exists(Path.Combine(pastaAssets, imagem)))
                    faltando.Add(imagem);
            }

            if (faltando.Count > 0)
            {
                return new ResultadoVerificacao("Imagens (Assets)", SituacaoVerificacao.Aviso,
                    "faltam " + faltando.Count + " imagem(ns): " + string.Join(", ", faltando));
            }

            return new ResultadoVerificacao("Imagens (Assets)", SituacaoVerificacao.Ok, "todas presentes");
        }

        private static ResultadoVerificacao VerificarHistorico()
        {
            string? reparo = GerenciadorHistorico.VerificarEReparar();
            return reparo is null
                ? new ResultadoVerificacao("Histórico", SituacaoVerificacao.Ok, "íntegro")
                : new ResultadoVerificacao("Histórico", SituacaoVerificacao.Corrigido, reparo);
        }

        private static ResultadoVerificacao VerificarFiltro()
        {
            string? problema = FiltroConteudo.ValidarArquivoDeConfiguracao();
            if (problema is not null)
                return new ResultadoVerificacao("Filtro de conteúdo", SituacaoVerificacao.Aviso, problema);

            bool existe = File.Exists(Path.Combine(AppContext.BaseDirectory, "filtro-conteudo.json"));
            return existe
                ? new ResultadoVerificacao("Filtro de conteúdo", SituacaoVerificacao.Ok,
                    "filtro-conteudo.json encontrado e válido (confira se está com \"habilitado\": true)")
                : new ResultadoVerificacao("Filtro de conteúdo", SituacaoVerificacao.Ok,
                    "DESLIGADO: não existe filtro-conteudo.json na pasta do programa, então nada é bloqueado");
        }

        // ---------------------------------------------------------------
        // Reparo: baixar o yt-dlp.exe oficial, com conferência de integridade
        // ---------------------------------------------------------------

        /// <summary>
        /// Baixa o yt-dlp.exe mais recente do GitHub oficial do yt-dlp e só o coloca
        /// em uso se o hash SHA-256 bater com o publicado na mesma release.
        /// Se já existir um arquivo no destino (danificado), ele é guardado como
        /// "yt-dlp.exe.corrompido" em vez de ser apagado.
        /// </summary>
        public static async Task<(bool Sucesso, string Mensagem)> BaixarYtDlpAsync(string destino, CancellationToken cancelamento = default)
        {
            string temporario = destino + ".baixando";

            try
            {
                const string baseUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/";

                using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };
                http.DefaultRequestHeaders.UserAgent.ParseAdd("YtDlpGui-AutoReparo");

                GerenciadorLog.Atividade("Autorreparo: baixando lista de hashes oficiais do yt-dlp...");
                string sums = await http.GetStringAsync(baseUrl + "SHA2-256SUMS", cancelamento);
                string? hashEsperado = ExtrairHash(sums, "yt-dlp.exe");
                if (hashEsperado is null)
                    return (false, "Não consegui obter o hash oficial do yt-dlp.exe para conferir o download.");

                GerenciadorLog.Atividade("Autorreparo: baixando yt-dlp.exe...");
                using (var resposta = await http.GetAsync(baseUrl + "yt-dlp.exe", HttpCompletionOption.ResponseHeadersRead, cancelamento))
                {
                    resposta.EnsureSuccessStatusCode();
                    using var origem = await resposta.Content.ReadAsStreamAsync(cancelamento);
                    using var arquivo = new FileStream(temporario, FileMode.Create, FileAccess.Write, FileShare.None);
                    await origem.CopyToAsync(arquivo, cancelamento);
                }

                string hashReal;
                using (var fs = new FileStream(temporario, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    hashReal = Convert.ToHexString(SHA256.HashData(fs));
                }

                if (!string.Equals(hashReal, hashEsperado, StringComparison.OrdinalIgnoreCase))
                {
                    TentarApagar(temporario);
                    GerenciadorLog.Erro("Autorreparo: o hash do yt-dlp.exe baixado NÃO confere (esperado " + hashEsperado + ", obtido " + hashReal + "). Arquivo descartado.");
                    return (false, "O arquivo baixado não passou na verificação de integridade e foi descartado.");
                }

                if (File.Exists(destino))
                    File.Move(destino, destino + ".corrompido", overwrite: true);

                File.Move(temporario, destino);

                GerenciadorLog.Reparo("yt-dlp.exe baixado do GitHub oficial e verificado (SHA-256) em: " + destino);
                return (true, "yt-dlp.exe instalado com sucesso.");
            }
            catch (OperationCanceledException)
            {
                TentarApagar(temporario);
                return (false, "O download do yt-dlp.exe foi interrompido ou demorou demais.");
            }
            catch (Exception ex)
            {
                TentarApagar(temporario);
                GerenciadorLog.Erro("Autorreparo: falha ao baixar o yt-dlp.exe", ex);
                return (false, "Não foi possível baixar o yt-dlp.exe agora (" + ex.Message + ").");
            }
        }

        private static string? ExtrairHash(string conteudoSums, string nomeArquivo)
        {
            foreach (string linha in conteudoSums.Replace("\r", "").Split('\n'))
            {
                string[] partes = linha.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length < 2)
                    continue;

                string nome = partes[partes.Length - 1].TrimStart('*');
                if (string.Equals(nome, nomeArquivo, StringComparison.Ordinal) && partes[0].Length == 64)
                    return partes[0];
            }

            return null;
        }

        private static void TentarApagar(string caminho)
        {
            try { if (File.Exists(caminho)) File.Delete(caminho); } catch { }
        }

        // ---------------------------------------------------------------
        // Classificação de erros (decide se vale tentar um reparo automático)
        // ---------------------------------------------------------------

        /// <summary>
        /// Erros típicos de um yt-dlp desatualizado: o YouTube muda o site com
        /// frequência e a correção costuma ser só atualizar o yt-dlp.
        /// </summary>
        public static bool PodeSerCorrigidoComAtualizacao(string? erro)
        {
            if (string.IsNullOrWhiteSpace(erro))
                return false;

            string[] marcas =
            {
                "make sure you are using the latest version",
                "Unable to extract",
                "nsig extraction failed",
                "Requested format is not available",
                "HTTP Error 403"
            };

            foreach (string marca in marcas)
            {
                if (erro.Contains(marca, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        // ---------------------------------------------------------------
        // Utilitários
        // ---------------------------------------------------------------

        /// <summary>Espaço livre (em bytes) do disco que contém a pasta, ou null se não for possível saber.</summary>
        public static long? EspacoLivreEmBytes(string pasta)
        {
            try
            {
                string? raiz = Path.GetPathRoot(Path.GetFullPath(pasta));
                if (string.IsNullOrEmpty(raiz))
                    return null;

                return new DriveInfo(raiz).AvailableFreeSpace;
            }
            catch
            {
                return null; // caminhos de rede (UNC), por exemplo
            }
        }

        /// <summary>
        /// Pasta onde é seguro gravar arquivos do programa (a pasta do programa, ou o plano B dos logs
        /// quando ela não permite gravação).
        /// </summary>
        public static string PastaGravavel()
        {
            return GerenciadorLog.PastaAtual;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace YtDlpGui
{
    /// <summary>
    /// Registro de erros e de atividade em arquivos .txt, gravados na PASTA DE
    /// ORIGEM do programa (a mesma onde está o YtDlpGui.exe).
    ///
    /// Arquivos gerados:
    ///   - log-erros.txt      -> só problemas: erros (com stack trace completo),
    ///                           avisos e reparos feitos automaticamente.
    ///   - log-atividade.txt  -> linha do tempo: o que o programa fez, a saída
    ///                           do yt-dlp (sem poluir com cada % de progresso)
    ///                           e uma cópia dos erros, para dar contexto.
    ///
    /// Características de segurança/robustez:
    ///   - NUNCA lança exceção: um log com problema jamais pode derrubar o app.
    ///   - Se a pasta do programa não permitir gravação (Arquivos de Programas,
    ///     pendrive protegido, pasta de rede somente-leitura...), muda sozinho
    ///     para %AppData%\YtDlpGui e, em último caso, para a pasta temporária.
    ///   - Rotação: passou do limite de tamanho, o arquivo vira "*.antigo.txt"
    ///     (um só, sobrescrito) e começa um novo. Nunca cresce sem controle.
    ///   - Anti-enxurrada: a mesma mensagem repetida em menos de 2 segundos é
    ///     contada em vez de gravada de novo (evita encher o disco num loop).
    ///   - Vários processos/PCs podem gravar no mesmo arquivo ao mesmo tempo
    ///     (útil se o programa rodar de uma pasta de rede); cada linha traz o
    ///     nome do computador.
    ///
    /// Esta classe é autocontida e pode ser copiada para outras ferramentas.
    /// </summary>
    public static class GerenciadorLog
    {
        private const string NomeArquivoErros = "log-erros.txt";
        private const string NomeArquivoAtividade = "log-atividade.txt";
        private const long LimiteErros = 1_000_000;      // ~1 MB
        private const long LimiteAtividade = 2_000_000;  // ~2 MB

        private static readonly object Trava = new();
        private static readonly string[] PastasCandidatas = MontarPastasCandidatas();
        private static readonly HashSet<string> CabecalhosEscritos = new();

        private static int _indicePasta;
        private static string? _notaMudancaDePasta;

        private static string _ultimaChave = "";
        private static DateTime _ultimaHora = DateTime.MinValue;
        private static int _suprimidas;

        // ---------------------------------------------------------------
        // Informações públicas
        // ---------------------------------------------------------------

        /// <summary>Pasta onde os logs estão sendo gravados neste momento.</summary>
        public static string PastaAtual
        {
            get
            {
                lock (Trava)
                {
                    return PastaSemTrava();
                }
            }
        }

        public static string CaminhoErros => Path.Combine(PastaAtual, NomeArquivoErros);

        public static string CaminhoAtividade => Path.Combine(PastaAtual, NomeArquivoAtividade);

        /// <summary>
        /// Pasta de origem do programa, onde o log TENTA ficar primeiro.
        /// </summary>
        public static string PastaDeOrigem => PastasCandidatas.Length > 0 ? PastasCandidatas[0] : "";

        // ---------------------------------------------------------------
        // API de registro
        // ---------------------------------------------------------------

        /// <summary>Registra um erro sem exceção associada (ex.: erro reportado pelo yt-dlp).</summary>
        public static void Erro(string mensagem) => Gravar("ERRO", mensagem, null);

        /// <summary>Registra um erro com a exceção completa (tipo, mensagem, stack trace, exceções internas).</summary>
        public static void Erro(string contexto, Exception ex) => Gravar("ERRO", contexto, ex);

        /// <summary>Registra algo suspeito que não impediu o funcionamento.</summary>
        public static void Aviso(string mensagem) => Gravar("AVISO", mensagem, null);

        /// <summary>Registra uma correção feita automaticamente pelo autodiagnóstico.</summary>
        public static void Reparo(string mensagem) => Gravar("REPARO", mensagem, null);

        /// <summary>Registra uma linha na linha do tempo (log-atividade.txt).</summary>
        public static void Atividade(string linha)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(linha))
                    return;

                Escrever(NomeArquivoAtividade, LimiteAtividade, Linha("INFO", linha));
            }
            catch
            {
                // Log nunca pode derrubar o programa.
            }
        }

        // ---------------------------------------------------------------
        // Implementação
        // ---------------------------------------------------------------

        private static void Gravar(string nivel, string mensagem, Exception? ex)
        {
            try
            {
                string chave = nivel + "|" + mensagem + "|" + (ex?.GetType().FullName ?? "");
                string prefixo = "";

                lock (Trava)
                {
                    DateTime agora = DateTime.UtcNow;
                    if (chave == _ultimaChave && (agora - _ultimaHora).TotalSeconds < 2)
                    {
                        _suprimidas++;
                        _ultimaHora = agora;
                        return;
                    }

                    if (_suprimidas > 0)
                    {
                        prefixo = Linha("AVISO", "(a mensagem anterior se repetiu mais " + _suprimidas + " vez(es) seguidas)");
                        _suprimidas = 0;
                    }

                    _ultimaChave = chave;
                    _ultimaHora = agora;
                }

                var sb = new StringBuilder(prefixo);
                sb.Append(Linha(nivel, mensagem));
                if (ex is not null)
                    sb.Append(Indentar(ex.ToString()));

                Escrever(NomeArquivoErros, LimiteErros, sb.ToString());

                // Cópia resumida na linha do tempo, para dar contexto ao que veio antes/depois.
                Escrever(NomeArquivoAtividade, LimiteAtividade,
                    Linha(nivel, ex is null ? mensagem : mensagem + " -> " + ex.GetType().Name + ": " + ex.Message));
            }
            catch
            {
                // Log nunca pode derrubar o programa.
            }
        }

        private static string[] MontarPastasCandidatas()
        {
            var lista = new List<string>();

            try { lista.Add(AppContext.BaseDirectory); } catch { }

            try
            {
                lista.Add(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YtDlpGui"));
            }
            catch { }

            try { lista.Add(Path.Combine(Path.GetTempPath(), "YtDlpGui")); } catch { }

            return lista.ToArray();
        }

        // Deve ser chamado já dentro do lock (ou em contexto onde a leitura é segura).
        private static string PastaSemTrava()
        {
            if (PastasCandidatas.Length == 0)
                return "";

            return PastasCandidatas[Math.Min(_indicePasta, PastasCandidatas.Length - 1)];
        }

        private static void Escrever(string nomeArquivo, long limite, string texto)
        {
            try
            {
                lock (Trava)
                {
                    int tentativasNestaPasta = 0;

                    while (true)
                    {
                        string pasta = PastaSemTrava();

                        try
                        {
                            Directory.CreateDirectory(pasta);
                            string caminho = Path.Combine(pasta, nomeArquivo);
                            RotacionarSeNecessario(caminho, limite);

                            string conteudo = texto;
                            bool primeiraVez = !CabecalhosEscritos.Contains(nomeArquivo);
                            if (primeiraVez)
                                conteudo = MontarCabecalho(pasta) + conteudo;
                            if (_notaMudancaDePasta is not null)
                                conteudo = _notaMudancaDePasta + conteudo;

                            // FileShare.ReadWrite: permite abrir o log no Bloco de Notas enquanto o
                            // programa roda, e permite que mais de um processo grave ao mesmo tempo.
                            using (var fs = new FileStream(caminho, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                            using (var sw = new StreamWriter(fs, new UTF8Encoding(true)))
                            {
                                sw.Write(conteudo);
                            }

                            if (primeiraVez)
                                CabecalhosEscritos.Add(nomeArquivo);
                            _notaMudancaDePasta = null;
                            return;
                        }
                        catch (Exception)
                        {
                            tentativasNestaPasta++;

                            // Pode ser um bloqueio momentâneo (antivírus, outro processo): tenta de novo.
                            if (tentativasNestaPasta < 3)
                            {
                                Thread.Sleep(20);
                                continue;
                            }

                            // Esta pasta não serve: tenta a próxima candidata (autorreparo).
                            if (_indicePasta >= PastasCandidatas.Length - 1)
                                return; // sem mais opções; desiste em silêncio

                            string pastaQueFalhou = pasta;
                            _indicePasta++;
                            tentativasNestaPasta = 0;
                            _notaMudancaDePasta = Linha("REPARO",
                                "Não foi possível gravar logs em '" + pastaQueFalhou +
                                "'. Usando '" + PastaSemTrava() + "' a partir de agora.");
                        }
                    }
                }
            }
            catch
            {
                // Log nunca pode derrubar o programa.
            }
        }

        private static void RotacionarSeNecessario(string caminho, long limite)
        {
            try
            {
                var info = new FileInfo(caminho);
                if (!info.Exists || info.Length <= limite)
                    return;

                string pasta = Path.GetDirectoryName(caminho) ?? "";
                string antigo = Path.Combine(pasta, Path.GetFileNameWithoutExtension(caminho) + ".antigo.txt");
                File.Move(caminho, antigo, overwrite: true);
            }
            catch
            {
                // Se não der para rotacionar, continua anexando no mesmo arquivo.
            }
        }

        private static string MontarCabecalho(string pasta)
        {
            string versao = "?";
            try { versao = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?"; } catch { }

            string estrutura = "?";
            string sistema = "?";
            try { estrutura = RuntimeInformation.FrameworkDescription; } catch { }
            try { sistema = RuntimeInformation.OSDescription; } catch { }

            var sb = new StringBuilder();
            sb.AppendLine(new string('=', 72));
            sb.AppendLine("Sessão iniciada em " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("YtDlpGui v" + versao + " | " + estrutura + " | " + sistema + " | PC: " + Environment.MachineName);
            sb.AppendLine("Pasta de logs: " + pasta);
            sb.AppendLine(new string('=', 72));
            return sb.ToString();
        }

        private static string Linha(string nivel, string mensagem)
        {
            return "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] [" + Environment.MachineName + "] [" +
                   nivel + "] " + mensagem + Environment.NewLine;
        }

        private static string Indentar(string texto)
        {
            var linhas = texto.Replace("\r\n", "\n").Split('\n');
            var sb = new StringBuilder();
            foreach (string l in linhas)
                sb.Append("    ").Append(l).Append(Environment.NewLine);
            return sb.ToString();
        }
    }
}

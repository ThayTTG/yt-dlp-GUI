using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace YtDlpGui
{
    /// <summary>Um programa auxiliar que o yt-dlp usa por baixo dos panos.</summary>
    public sealed record Componente(
        string Arquivo,        // nome do .exe (ex.: "ffmpeg.exe")
        string Nome,           // nome amigável (ex.: "FFmpeg")
        string Para,           // para que serve, em uma frase
        string UrlZip,         // pacote oficial
        string UrlHash,        // arquivo com o SHA-256 publicado pelo próprio projeto
        string CaminhoNoZip);  // final do caminho do .exe dentro do zip

    /// <summary>
    /// Garante que os programas auxiliares do yt-dlp estejam na pasta do app:
    ///   - FFmpeg: junta vídeo e áudio (MP4 em alta qualidade) e converte para MP3.
    ///   - Deno: resolve os desafios em JavaScript do YouTube. Sem ele, desde o fim de 2025
    ///     o YouTube entrega poucos formatos (ou nenhum) e aparece o erro
    ///     "Requested format is not available".
    /// Os pacotes vêm dos sites oficiais e só são usados depois de conferir o SHA-256
    /// publicado pelo próprio projeto. O yt-dlp.exe continua sendo cuidado pelo AutoReparo.
    /// </summary>
    public static class Componentes
    {
        public static readonly Componente Ffmpeg = new(
            "ffmpeg.exe", "FFmpeg", "junta vídeo e áudio e converte para MP3",
            "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip",
            "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip.sha256",
            "/bin/ffmpeg.exe");

        public static readonly Componente Deno = new(
            "deno.exe", "Deno", "resolve os desafios do YouTube",
            "https://github.com/denoland/deno/releases/latest/download/deno-x86_64-pc-windows-msvc.zip",
            "https://github.com/denoland/deno/releases/latest/download/deno-x86_64-pc-windows-msvc.zip.sha256sum",
            "deno.exe");

        public static readonly IReadOnlyList<Componente> Todos = new[] { Ffmpeg, Deno };

        private static readonly Regex HashHex = new(@"\b[0-9a-fA-F]{64}\b", RegexOptions.Compiled);

        private const long TamanhoMinimo = 1_000_000; // os reais têm dezenas de MB

        /// <summary>Pasta do programa (onde os componentes ficam, se der para gravar nela).</summary>
        public static string PastaDoApp => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>Plano B quando a pasta do programa é protegida (ex.: Arquivos de Programas).</summary>
        public static string PastaAlternativa => Path.Combine(AutoReparo.PastaGravavel(), "componentes");

        /// <summary>Caminho do componente instalado, ou null se ele não estiver em nenhuma das duas pastas.</summary>
        public static string? Localizar(Componente c)
        {
            foreach (string pasta in new[] { PastaDoApp, PastaAlternativa })
            {
                string caminho = Path.Combine(pasta, c.Arquivo);
                try
                {
                    if (File.Exists(caminho) && new FileInfo(caminho).Length >= TamanhoMinimo)
                        return caminho;
                }
                catch { }
            }
            return null;
        }

        public static bool Instalado(Componente c) => Localizar(c) is not null;

        /// <summary>
        /// PATH para os processos do yt-dlp: as pastas dos componentes vêm primeiro, assim o
        /// yt-dlp encontra o deno.exe (ele procura o Deno pelo PATH).
        /// </summary>
        public static string MontarPath(IEnumerable<string> pastasPrimeiro, string? pathAtual)
        {
            var partes = new List<string>();
            foreach (string p in pastasPrimeiro)
            {
                string limpa = p.TrimEnd('\\', '/');
                if (limpa.Length > 0 && !partes.Contains(limpa, StringComparer.OrdinalIgnoreCase))
                    partes.Add(limpa);
            }
            if (!string.IsNullOrEmpty(pathAtual))
                partes.Add(pathAtual);
            return string.Join(Path.PathSeparator, partes);
        }

        /// <summary>Prepara o ambiente de qualquer processo do yt-dlp (UTF-8 e PATH com os componentes).</summary>
        public static void PrepararAmbiente(ProcessStartInfo psi)
        {
            psi.Environment["PYTHONIOENCODING"] = "utf-8";

            var pastas = new List<string> { PastaDoApp };
            if (Directory.Exists(PastaAlternativa))
                pastas.Add(PastaAlternativa);

            string? pathAtual = psi.Environment.TryGetValue("PATH", out var p) ? p : Environment.GetEnvironmentVariable("PATH");
            psi.Environment["PATH"] = MontarPath(pastas, pathAtual);
        }

        /// <summary>Primeiro hash SHA-256 (64 dígitos hexadecimais) de um arquivo de conferência.</summary>
        public static string? ExtrairHash(string conteudo)
        {
            var m = HashHex.Match(conteudo ?? "");
            return m.Success ? m.Value.ToUpperInvariant() : null;
        }

        /// <summary>Pasta onde dá para instalar: a do programa, ou a alternativa se ela for protegida.</summary>
        public static string PastaDeInstalacao()
        {
            try
            {
                string teste = Path.Combine(PastaDoApp, ".teste-gravacao-" + Environment.ProcessId);
                File.WriteAllText(teste, "");
                File.Delete(teste);
                return PastaDoApp;
            }
            catch
            {
                Directory.CreateDirectory(PastaAlternativa);
                return PastaAlternativa;
            }
        }

        /// <summary>
        /// Baixa o pacote oficial, confere o SHA-256 e extrai só o .exe para a pasta de instalação.
        /// progresso recebe de 0 a 100 (ou -1 quando o tamanho é desconhecido).
        /// </summary>
        public static async Task<(bool Sucesso, string Mensagem)> InstalarAsync(
            Componente c, IProgress<double>? progresso = null, CancellationToken cancelamento = default)
        {
            string pasta = PastaDeInstalacao();
            string destino = Path.Combine(pasta, c.Arquivo);
            string zipTemp = Path.Combine(Path.GetTempPath(), $"ytdlpgui-{c.Nome.ToLowerInvariant()}-{Guid.NewGuid():N}.zip");
            string exeTemp = destino + ".baixando";

            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
                http.DefaultRequestHeaders.UserAgent.ParseAdd("YtDlpGui-Componentes");

                GerenciadorLog.Atividade($"Componentes: baixando o hash oficial do {c.Nome}...");
                string? hashEsperado = ExtrairHash(await http.GetStringAsync(c.UrlHash, cancelamento));
                if (hashEsperado is null)
                    return (false, $"Não consegui obter o hash oficial do {c.Nome} para conferir o download.");

                GerenciadorLog.Atividade($"Componentes: baixando {c.UrlZip}");
                using (var resposta = await http.GetAsync(c.UrlZip, HttpCompletionOption.ResponseHeadersRead, cancelamento))
                {
                    resposta.EnsureSuccessStatusCode();
                    long? total = resposta.Content.Headers.ContentLength;
                    await using var origem = await resposta.Content.ReadAsStreamAsync(cancelamento);
                    await using var arquivo = new FileStream(zipTemp, FileMode.Create, FileAccess.Write, FileShare.None);

                    var buffer = new byte[81920];
                    long lidos = 0;
                    int ultimoInformado = -1;
                    int n;
                    while ((n = await origem.ReadAsync(buffer, cancelamento)) > 0)
                    {
                        await arquivo.WriteAsync(buffer.AsMemory(0, n), cancelamento);
                        lidos += n;
                        if (total is > 0)
                        {
                            int pct = (int)(lidos * 100 / total.Value);
                            if (pct != ultimoInformado)
                            {
                                ultimoInformado = pct;
                                progresso?.Report(pct);
                            }
                        }
                        else if (ultimoInformado != -1)
                        {
                            ultimoInformado = -1;
                            progresso?.Report(-1);
                        }
                    }
                }

                string hashReal;
                await using (var fs = new FileStream(zipTemp, FileMode.Open, FileAccess.Read, FileShare.Read))
                    hashReal = Convert.ToHexString(await SHA256.HashDataAsync(fs, cancelamento));

                if (!string.Equals(hashReal, hashEsperado, StringComparison.OrdinalIgnoreCase))
                {
                    GerenciadorLog.Erro($"Componentes: o hash do {c.Nome} NÃO confere (esperado {hashEsperado}, obtido {hashReal}). Pacote descartado.");
                    return (false, $"O pacote do {c.Nome} não passou na verificação de integridade e foi descartado.");
                }

                using (var zip = ZipFile.OpenRead(zipTemp))
                {
                    ZipArchiveEntry? entrada = null;
                    foreach (var e in zip.Entries)
                    {
                        string nome = e.FullName.Replace('\\', '/');
                        if (nome.EndsWith(c.CaminhoNoZip, StringComparison.OrdinalIgnoreCase) &&
                            (entrada is null || nome.Length < entrada.FullName.Length))
                            entrada = e;
                    }

                    if (entrada is null)
                        return (false, $"Não encontrei o {c.Arquivo} dentro do pacote baixado.");

                    entrada.ExtractToFile(exeTemp, overwrite: true);
                }

                File.Move(exeTemp, destino, overwrite: true);
                GerenciadorLog.Reparo($"{c.Nome} instalado e verificado (SHA-256) em: {destino}");
                return (true, $"{c.Nome} instalado.");
            }
            catch (OperationCanceledException)
            {
                return (false, $"O download do {c.Nome} foi interrompido ou demorou demais.");
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro($"Componentes: falha ao instalar o {c.Nome}", ex);
                return (false, $"Não foi possível baixar o {c.Nome} agora ({ex.Message}).");
            }
            finally
            {
                TentarApagar(zipTemp);
                TentarApagar(exeTemp);
            }
        }

        /// <summary>Primeira linha de "programa --version" (ou "-version"), para a tela Sobre.</summary>
        public static async Task<string?> LerVersaoAsync(string caminhoExe, string argumento)
        {
            try
            {
                var psi = new ProcessStartInfo { FileName = caminhoExe };
                psi.ArgumentList.Add(argumento);
                var r = await ProcessosFilhos.ExecutarCapturandoAsync(psi, TimeSpan.FromSeconds(10), CancellationToken.None);
                if (!r.Iniciou || r.CodigoSaida != 0 || string.IsNullOrWhiteSpace(r.Saida))
                    return null;
                return ResumirVersao(r.Saida);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>"ffmpeg version 7.1-essentials_build-www.gyan.dev Copyright..." → "7.1-essentials_build-www.gyan.dev".</summary>
        public static string ResumirVersao(string saida)
        {
            string linha = saida.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            var m = Regex.Match(linha, @"^(?:ffmpeg version|deno)\s+(\S+)", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : linha;
        }

        private static void TentarApagar(string caminho)
        {
            try { if (File.Exists(caminho)) File.Delete(caminho); } catch { }
        }
    }
}

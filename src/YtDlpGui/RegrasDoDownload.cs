using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace YtDlpGui
{
    /// <summary>Opções escolhidas na tela que mudam o comando do yt-dlp.</summary>
    public sealed record OpcoesDownload(
        string Url,
        bool Mp3,
        string PastaDestino,
        bool BaixarPlaylist,
        bool OrganizarPlaylist,
        ModoCookies Cookies,
        string? Navegador,
        string? ArquivoCookies,
        string? PastaFfmpeg);

    public enum ModoCookies { Nenhum, Navegador, Arquivo }

    /// <summary>
    /// As regras do download, sem nada de tela (vieram do antigo MainForm.cs, sem mudar o
    /// comportamento): montar os argumentos do yt-dlp, traduzir os erros, detectar o navegador
    /// padrão etc. Por não depender da interface, dá para testar.
    /// </summary>
    public static class RegrasDoDownload
    {
        public static readonly Regex RegexProgresso =
            new(@"\[download\]\s+(\d{1,3}(?:\.\d+)?)%", RegexOptions.Compiled);
        public static readonly Regex RegexErro =
            new(@"^ERROR:\s*(.+)$", RegexOptions.Compiled);
        public static readonly Regex RegexPlaylistNome =
            new(@"Downloading playlist:\s*(.+)$", RegexOptions.Compiled);
        public static readonly Regex RegexPlaylistItem =
            new(@"Downloading item (\d+) of (\d+)", RegexOptions.Compiled);

        /// <summary>Página oficial do yt-dlp que explica como exportar o cookies.txt.</summary>
        public const string UrlAjudaCookies = "https://github.com/yt-dlp/yt-dlp/wiki/Extractors#exporting-youtube-cookies";

        /// <summary>Navegadores da lista de Configurações (no formato que o yt-dlp entende).</summary>
        public static readonly string[] Navegadores = { "chrome", "edge", "firefox", "brave", "opera", "vivaldi", "chromium" };

        /// <summary>
        /// Só aceita endereços http/https. Antes, qualquer texto que o .NET considerasse uma
        /// "URL absoluta" passava (inclusive caminhos como C:\Windows ou ftp://...), disparando
        /// o yt-dlp à toa e gerando erros confusos.
        /// </summary>
        public static bool UrlValida(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        public static List<string> ConstruirArgumentos(OpcoesDownload o)
        {
            var argumentos = new List<string> { o.Url };

            if (o.Mp3)
            {
                argumentos.Add("-x");
                argumentos.Add("--audio-format");
                argumentos.Add("mp3");
            }
            else
            {
                argumentos.Add("-f");
                argumentos.Add("bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/bestvideo+bestaudio/best");
                argumentos.Add("--merge-output-format");
                argumentos.Add("mp4");
            }

            argumentos.Add("--newline");
            argumentos.Add("-P");
            argumentos.Add(o.PastaDestino);
            argumentos.Add("-o");
            argumentos.Add(ObterTemplateDeSaida(o.BaixarPlaylist, o.OrganizarPlaylist));

            argumentos.Add(o.BaixarPlaylist ? "--yes-playlist" : "--no-playlist");

            if (o.Cookies == ModoCookies.Navegador)
            {
                if (!string.IsNullOrWhiteSpace(o.Navegador))
                {
                    argumentos.Add("--cookies-from-browser");
                    argumentos.Add(o.Navegador);
                }
            }
            else if (o.Cookies == ModoCookies.Arquivo)
            {
                if (!string.IsNullOrWhiteSpace(o.ArquivoCookies) && File.Exists(o.ArquivoCookies))
                {
                    argumentos.Add("--cookies");
                    argumentos.Add(o.ArquivoCookies);
                }
            }

            if (o.PastaFfmpeg is not null)
            {
                argumentos.Add("--ffmpeg-location");
                argumentos.Add(o.PastaFfmpeg);
            }

            return argumentos;
        }

        /// <summary>
        /// Define onde/como os arquivos são nomeados e salvos:
        /// - Vídeo único: sempre solto direto na pasta escolhida.
        /// - Playlist + "organizar em subpastas" marcado: cria uma subpasta
        ///   com o nome da playlist dentro da pasta escolhida, e cada vídeo
        ///   vai lá dentro.
        /// - Playlist sem a opção marcada: todos os vídeos soltos direto na
        ///   pasta escolhida (comportamento antigo).
        /// </summary>
        public static string ObterTemplateDeSaida(bool baixarPlaylist, bool organizarPlaylist)
        {
            if (baixarPlaylist && organizarPlaylist)
                return "%(playlist_title)s/%(title)s.%(ext)s";

            return "%(title)s.%(ext)s";
        }

        /// <summary>Argumentos da prévia (yt-dlp --dump-json, sem baixar).</summary>
        public static List<string> ArgumentosDaPrevia(string url) =>
            new() { "--dump-json", "--no-warnings", "--skip-download", "--no-playlist", url };

        /// <summary>
        /// Argumentos da extração de cookies. O vídeo é estável (o primeiro já publicado no
        /// YouTube) e serve só de "gatilho" para o yt-dlp gravar os cookies: nada é baixado.
        /// </summary>
        public static List<string> ArgumentosDaExtracaoDeCookies(string navegador, string caminhoCookies) => new()
        {
            "--cookies-from-browser", navegador,
            "--cookies", caminhoCookies,
            "--skip-download", "--simulate", "--no-warnings",
            "https://www.youtube.com/watch?v=jNQXAC9IVRw"
        };

        public static string ObterPastaDownloadsPadrao()
        {
            try
            {
                string perfil = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string downloads = Path.Combine(perfil, "Downloads");
                return Directory.Exists(downloads)
                    ? downloads
                    : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
            catch
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
        }

        /// <summary>Pasta real onde o arquivo foi salvo, ou o valor padrão se não for possível saber.</summary>
        public static string PastaDoArquivo(string? arquivo, string padrao)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(arquivo))
                {
                    string? pasta = Path.GetDirectoryName(arquivo);
                    if (!string.IsNullOrEmpty(pasta) && Directory.Exists(pasta))
                        return pasta;
                }
            }
            catch
            {
                // Caminho em formato estranho: usa o padrão.
            }

            return padrao;
        }

        /// <summary>Título da prévia: no máximo 60 caracteres.</summary>
        public static string EncurtarTitulo(string titulo) =>
            titulo.Length > 60 ? titulo[..57] + "..." : titulo;

        /// <summary>Linha de baixo da prévia: "Canal • 12:34".</summary>
        public static string InfoDaPrevia(string canal, string duracao)
        {
            string info = canal;
            if (!string.IsNullOrEmpty(duracao))
                info += (info.Length > 0 ? " • " : "") + duracao;
            return info;
        }

        public static string TraduzirErro(string? erroBruto)
        {
            if (string.IsNullOrWhiteSpace(erroBruto))
                return "Ocorreu um erro durante o download. Tente novamente.";

            if (erroBruto.Contains("Sign in to confirm") && erroBruto.Contains("not a bot"))
            {
                return "O YouTube pediu confirmação de que você não é um robô. " +
                       "Vá em \"Configurações\" e ative os cookies do navegador ou um arquivo cookies.txt.";
            }

            if (erroBruto.Contains("confirm your age", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("age-restricted", StringComparison.OrdinalIgnoreCase))
            {
                return "Este vídeo tem restrição de idade e exige login. " +
                       "Ative os cookies do navegador ou um arquivo cookies.txt em \"Configurações\".";
            }

            if (erroBruto.Contains("ffmpeg"))
            {
                return "Houve um problema relacionado ao ffmpeg. Verifique se o ffmpeg.exe " +
                       "está na mesma pasta do programa.";
            }

            if (erroBruto.Contains("Video unavailable") || erroBruto.Contains("This video is unavailable"))
            {
                return "Este vídeo não está mais disponível ou foi removido.";
            }

            if (erroBruto.Contains("Private video"))
            {
                return "Este vídeo é privado e não pode ser baixado.";
            }

            if (erroBruto.Contains("No space left on device", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("Errno 28"))
            {
                return "O disco ficou sem espaço livre. Libere espaço ou escolha outra pasta na tela \"Pasta\".";
            }

            if (erroBruto.Contains("Permission denied", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("Errno 13"))
            {
                return "Sem permissão para gravar na pasta de destino. Escolha outra pasta na tela \"Pasta\".";
            }

            if (erroBruto.Contains("getaddrinfo failed", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("name resolution", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("Network is unreachable", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("timed out", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("Connection reset", StringComparison.OrdinalIgnoreCase) ||
                erroBruto.Contains("Connection aborted", StringComparison.OrdinalIgnoreCase))
            {
                return "Não foi possível conectar à internet. Verifique a conexão e tente novamente.";
            }

            if (erroBruto.Contains("HTTP Error 429", StringComparison.OrdinalIgnoreCase))
            {
                return "O site recebeu pedidos demais e pediu para esperar. Aguarde alguns minutos e tente de novo.";
            }

            if (erroBruto.Contains("Unsupported URL", StringComparison.OrdinalIgnoreCase))
            {
                return "Este endereço não é suportado pelo programa. Confira se copiou o link do vídeo.";
            }

            return "Ocorreu um erro: " + erroBruto;
        }

        /// <summary>Erros que normalmente se resolvem usando cookies de uma conta logada.</summary>
        public static bool ErroPedeCookies(string? erro)
        {
            if (string.IsNullOrWhiteSpace(erro))
                return false;

            return erro.Contains("Sign in to confirm", StringComparison.OrdinalIgnoreCase) ||
                   erro.Contains("confirm your age", StringComparison.OrdinalIgnoreCase) ||
                   erro.Contains("age-restricted", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Traduz os erros mais comuns da extração de cookies do navegador.</summary>
        public static string TraduzirErroExtracaoCookies(string erroBruto, string navegador)
        {
            if (string.IsNullOrWhiteSpace(erroBruto))
            {
                return $"Não foi possível extrair os cookies do {navegador}. Verifique se ele está " +
                       "instalado e se você já fez login no YouTube nele.";
            }

            if (erroBruto.Contains("could not find") && erroBruto.Contains("cookies database"))
            {
                return $"Não foi encontrado um perfil do {navegador} com cookies salvos. " +
                       "Abra esse navegador, faça login no YouTube, feche-o completamente e tente novamente.";
            }

            if (erroBruto.Contains("Permission denied") || erroBruto.Contains("being used by another process"))
            {
                return $"O {navegador} está bloqueando o acesso aos cookies. Feche completamente o " +
                       "navegador (inclusive na bandeja do sistema, perto do relógio) e tente novamente.";
            }

            if (erroBruto.Contains("decrypt"))
            {
                return $"Não foi possível decriptar os cookies do {navegador} nesta conta do Windows. " +
                       "Tente fazer login no YouTube novamente nesse navegador antes de extrair.";
            }

            return $"Ocorreu um erro ao extrair os cookies do {navegador}: {erroBruto}";
        }

        /// <summary>O cookies.txt gerado tem cookies do YouTube/Google? (é o que vale, não o código de saída)</summary>
        public static bool CookiesTxtTemCookiesDoYoutube(string caminho)
        {
            try
            {
                if (!File.Exists(caminho) || new FileInfo(caminho).Length == 0)
                    return false;
                foreach (var linha in File.ReadLines(caminho))
                {
                    if (linha.StartsWith('#')) continue;
                    if (linha.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
                        linha.Contains("google.com", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Não foi possível conferir o cookies.txt gerado", ex);
            }
            return false;
        }

        /// <summary>
        /// Descobre o navegador padrão do Windows para links https (Chrome, Edge, Firefox...),
        /// no formato que o yt-dlp entende. Devolve null se não conseguir identificar.
        /// </summary>
        public static string? DetectarNavegadorPadrao()
        {
            if (!OperatingSystem.IsWindows())
                return null;

            try
            {
                using var chave = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice");
                return NavegadorPeloProgId(chave?.GetValue("ProgId") as string);
            }
            catch (Exception ex)
            {
                GerenciadorLog.Aviso("Não foi possível identificar o navegador padrão: " + ex.Message);
            }

            return null;
        }

        /// <summary>Converte o ProgId do registro ("ChromeHTML", "MSEdgeHTM"...) no nome do yt-dlp.</summary>
        public static string? NavegadorPeloProgId(string? progId)
        {
            if (string.IsNullOrEmpty(progId))
                return null;

            string p = progId.ToLowerInvariant();
            if (p.Contains("chrome")) return "chrome";
            if (p.Contains("edge")) return "edge";
            if (p.Contains("firefox")) return "firefox";
            if (p.Contains("brave")) return "brave";
            if (p.Contains("opera")) return "opera";
            if (p.Contains("vivaldi")) return "vivaldi";
            return null;
        }
    }
}

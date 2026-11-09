using YtDlpGui;
using Xunit;

namespace YtDlpGui.Tests;

/// <summary>
/// O comando do yt-dlp e as mensagens têm que continuar EXATAMENTE como na versão 2
/// (a versão 3 só trocou a interface).
/// </summary>
public sealed class RegrasTests
{
    private static OpcoesDownload Opcoes(bool mp3 = false, bool playlist = false, bool organizar = false,
        ModoCookies cookies = ModoCookies.Nenhum, string? navegador = null, string? arquivo = null, string? ffmpeg = "C:/App") =>
        new("https://www.youtube.com/watch?v=x", mp3, "C:/Downloads", playlist, organizar, cookies, navegador, arquivo, ffmpeg);

    [Fact]
    public void Mp4_monta_o_mesmo_comando_da_versao_2()
    {
        var args = RegrasDoDownload.ConstruirArgumentos(Opcoes());

        Assert.Equal(new[]
        {
            "https://www.youtube.com/watch?v=x",
            "-f", "bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/bestvideo+bestaudio/best",
            "--merge-output-format", "mp4",
            "--newline", "-P", "C:/Downloads", "-o", "%(title)s.%(ext)s",
            "--no-playlist",
            "--ffmpeg-location", "C:/App",
        }, args);
    }

    [Fact]
    public void Mp3_extrai_o_audio()
    {
        var args = RegrasDoDownload.ConstruirArgumentos(Opcoes(mp3: true, ffmpeg: null));

        Assert.Equal(new[] { "-x", "--audio-format", "mp3" }, args.Skip(1).Take(3));
        Assert.DoesNotContain("--ffmpeg-location", args);
    }

    [Theory]
    [InlineData(false, false, "%(title)s.%(ext)s", "--no-playlist")]
    [InlineData(true, false, "%(title)s.%(ext)s", "--yes-playlist")]
    [InlineData(true, true, "%(playlist_title)s/%(title)s.%(ext)s", "--yes-playlist")]
    [InlineData(false, true, "%(title)s.%(ext)s", "--no-playlist")] // subpasta só vale com playlist
    public void Playlist_e_subpasta(bool playlist, bool organizar, string template, string flag)
    {
        var args = RegrasDoDownload.ConstruirArgumentos(Opcoes(playlist: playlist, organizar: organizar));

        Assert.Equal(template, args[args.IndexOf("-o") + 1]);
        Assert.Contains(flag, args);
    }

    [Fact]
    public void Cookies_do_navegador()
    {
        var args = RegrasDoDownload.ConstruirArgumentos(Opcoes(cookies: ModoCookies.Navegador, navegador: "firefox"));
        Assert.Equal("firefox", args[args.IndexOf("--cookies-from-browser") + 1]);
    }

    [Fact]
    public void Arquivo_de_cookies_so_entra_se_existir()
    {
        var arquivo = Path.GetTempFileName();
        try
        {
            var com = RegrasDoDownload.ConstruirArgumentos(Opcoes(cookies: ModoCookies.Arquivo, arquivo: arquivo));
            var sem = RegrasDoDownload.ConstruirArgumentos(Opcoes(cookies: ModoCookies.Arquivo, arquivo: arquivo + ".nao-existe"));

            Assert.Equal(arquivo, com[com.IndexOf("--cookies") + 1]);
            Assert.DoesNotContain("--cookies", sem);
        }
        finally { File.Delete(arquivo); }
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=x", true)]
    [InlineData("http://youtu.be/x", true)]
    [InlineData("ftp://site.com/video", false)]
    [InlineData("C:\\Windows", false)]
    [InlineData("não é um link", false)]
    public void Url_valida_so_http_e_https(string url, bool valida) =>
        Assert.Equal(valida, RegrasDoDownload.UrlValida(url));

    [Theory]
    [InlineData("Sign in to confirm you're not a bot", "robô")]
    [InlineData("This video is age-restricted", "restrição de idade")]
    [InlineData("[youtube] x: Private video. Sign in", "privado")]
    [InlineData("Video unavailable", "não está mais disponível")]
    [InlineData("[Errno 28] No space left on device", "sem espaço")]
    [InlineData("HTTP Error 429: Too Many Requests", "pedidos demais")]
    [InlineData("getaddrinfo failed", "internet")]
    [InlineData("Unsupported URL: https://x", "não é suportado")]
    public void Erros_sao_traduzidos(string erro, string trecho) =>
        Assert.Contains(trecho, RegrasDoDownload.TraduzirErro(erro));

    [Fact]
    public void Erro_desconhecido_aparece_como_veio() =>
        Assert.Equal("Ocorreu um erro: algo novo", RegrasDoDownload.TraduzirErro("algo novo"));

    [Theory]
    [InlineData("Sign in to confirm you're not a bot", true)]
    [InlineData("Please confirm your age", true)]
    [InlineData("Video unavailable", false)]
    [InlineData(null, false)]
    public void Erros_que_pedem_cookies(string? erro, bool pede) =>
        Assert.Equal(pede, RegrasDoDownload.ErroPedeCookies(erro));

    [Theory]
    [InlineData("ChromeHTML", "chrome")]
    [InlineData("MSEdgeHTM", "edge")]
    [InlineData("FirefoxURL-308046B0AF4A39CB", "firefox")]
    [InlineData("BraveHTML", "brave")]
    [InlineData("IE.HTTP", null)]
    // Linux (xdg-settings)
    [InlineData("google-chrome.desktop", "chrome")]
    [InlineData("chromium-browser.desktop", "chromium")]
    [InlineData("firefox.desktop", "firefox")]
    [InlineData("microsoft-edge.desktop", "edge")]
    public void Navegador_padrao_pelo_registro(string progId, string? esperado) =>
        Assert.Equal(esperado, RegrasDoDownload.NavegadorPeloProgId(progId));

    [Fact]
    public void Titulo_da_previa_tem_no_maximo_60_caracteres()
    {
        var longo = new string('a', 80);
        Assert.Equal(60, RegrasDoDownload.EncurtarTitulo(longo).Length);
        Assert.EndsWith("...", RegrasDoDownload.EncurtarTitulo(longo));
        Assert.Equal("curto", RegrasDoDownload.EncurtarTitulo("curto"));
    }

    [Fact]
    public void Linha_de_info_da_previa() =>
        Assert.Equal("Canal • 10:42", RegrasDoDownload.InfoDaPrevia("Canal", "10:42"));

    [Fact]
    public void Cookies_txt_so_vale_com_cookies_do_youtube()
    {
        var bom = Path.GetTempFileName();
        var ruim = Path.GetTempFileName();
        try
        {
            File.WriteAllText(bom, "# Netscape HTTP Cookie File\n.youtube.com\tTRUE\t/\tTRUE\t0\tPREF\tx\n");
            File.WriteAllText(ruim, "# Netscape HTTP Cookie File\n.exemplo.com\tTRUE\t/\tTRUE\t0\tA\tb\n");

            Assert.True(RegrasDoDownload.CookiesTxtTemCookiesDoYoutube(bom));
            Assert.False(RegrasDoDownload.CookiesTxtTemCookiesDoYoutube(ruim));
        }
        finally { File.Delete(bom); File.Delete(ruim); }
    }
}

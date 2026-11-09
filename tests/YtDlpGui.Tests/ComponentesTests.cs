using System;
using System.IO;
using Xunit;
using YtDlpGui;

namespace YtDlpGui.Tests;

public class ComponentesTests
{
    private const string Hash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    [Fact]
    public void ExtraiHashDoArquivoSimplesDoGyan() =>
        Assert.Equal(Hash.ToUpperInvariant(), Componentes.ExtrairHash(Hash + "\n"));

    [Fact]
    public void ExtraiHashDoFormatoDoPowerShellUsadoPeloDeno()
    {
        string conteudo =
            "\r\nAlgorithm       Hash                                                                   Path\r\n" +
            "---------       ----                                                                   ----\r\n" +
            "SHA256          " + Hash.ToUpperInvariant() + "       D:\\a\\deno\\deno\\target\\release\\deno-x86_64-pc-windows-msvc.zip\r\n";
        Assert.Equal(Hash.ToUpperInvariant(), Componentes.ExtrairHash(conteudo));
    }

    [Fact]
    public void SemHashDevolveNulo() => Assert.Null(Componentes.ExtrairHash("<html>erro 404</html>"));

    [Fact]
    public void PathColocaAsPastasDosComponentesPrimeiroSemRepetir()
    {
        char sep = Path.PathSeparator;
        string path = Componentes.MontarPath(new[] { "C:\\App\\", "C:\\App", "C:\\Logs\\componentes" }, "C:\\Windows");
        Assert.Equal($"C:\\App{sep}C:\\Logs\\componentes{sep}C:\\Windows", path);
    }

    [Theory]
    [InlineData("ffmpeg version 7.1-essentials_build-www.gyan.dev Copyright (c) 2000-2024\nbuilt with gcc", "7.1-essentials_build-www.gyan.dev")]
    [InlineData("deno 2.5.6 (stable, release, x86_64-pc-windows-msvc)\nv8 14.0", "2.5.6")]
    [InlineData("2026.10.22\n", "2026.10.22")]
    public void ResumeAVersao(string saida, string esperado) =>
        Assert.Equal(esperado, Componentes.ResumirVersao(saida));

    [Fact]
    public void ComponentesUsamOsSitesOficiais()
    {
        Assert.StartsWith(OperatingSystem.IsWindows() ? "https://www.gyan.dev/" : "https://github.com/yt-dlp/FFmpeg-Builds/",
            Componentes.Ffmpeg.UrlZip);
        Assert.StartsWith("https://github.com/denoland/deno/", Componentes.Deno.UrlZip);
        Assert.All(Componentes.Todos, c => Assert.StartsWith("https://", c.UrlHash));
    }

    [Fact]
    public void NomesDosExecutaveisSeguemOSistema()
    {
        string fim = OperatingSystem.IsWindows() ? ".exe" : "";
        Assert.Equal("yt-dlp" + fim, Plataforma.YtDlp);
        Assert.Equal("ffmpeg" + fim, Componentes.Ffmpeg.Arquivo);
        Assert.Equal("deno" + fim, Componentes.Deno.Arquivo);
        Assert.Equal(OperatingSystem.IsWindows() ? "yt-dlp.exe" : "yt-dlp_linux", Plataforma.YtDlpNoGitHub);
    }

    [Fact]
    public void AchaOHashDoPacoteCertoNumaListaComVarios()
    {
        string lista =
            Hash + "  ffmpeg-master-latest-win64-gpl.zip\n" +
            "fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210  ffmpeg-master-latest-linux64-gpl.tar.xz\n";
        Assert.Equal("FEDCBA9876543210FEDCBA9876543210FEDCBA9876543210FEDCBA9876543210",
            Componentes.ExtrairHash(lista, "ffmpeg-master-latest-linux64-gpl.tar.xz"));
        Assert.Null(Componentes.ExtrairHash(lista, "nao-existe.zip"));
    }

    [Fact]
    public void ProcuraProgramasNoPath()
    {
        string pasta = Path.Combine(Path.GetTempPath(), "ytdlpgui-path-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(pasta);
        string nome = "programa-de-teste" + (OperatingSystem.IsWindows() ? ".exe" : "");
        File.WriteAllText(Path.Combine(pasta, nome), "x");
        string? antigo = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", pasta + Path.PathSeparator + antigo);
            Assert.Equal(Path.Combine(pasta, nome), Plataforma.ProcurarNoPath(nome));
            Assert.Null(Plataforma.ProcurarNoPath("nao-existe-mesmo-123"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", antigo);
            Directory.Delete(pasta, recursive: true);
        }
    }

    [Theory]
    [InlineData("ERROR: [youtube] wGPIlagDxEs: Requested format is not available. Use --list-formats for a list of available formats")]
    [InlineData("WARNING: [youtube] No supported JavaScript runtime could be found.")]
    public void ErroDeFormatoIndisponivelExplicaODeno(string erro)
    {
        string msg = RegrasDoDownload.TraduzirErro(erro);
        Assert.Contains("Deno", msg);
        Assert.Contains("Verificar atualização", msg);
    }
}

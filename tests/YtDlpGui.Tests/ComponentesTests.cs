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
        Assert.StartsWith("https://www.gyan.dev/", Componentes.Ffmpeg.UrlZip);
        Assert.StartsWith("https://github.com/denoland/deno/", Componentes.Deno.UrlZip);
        Assert.All(Componentes.Todos, c => Assert.StartsWith("https://", c.UrlHash));
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

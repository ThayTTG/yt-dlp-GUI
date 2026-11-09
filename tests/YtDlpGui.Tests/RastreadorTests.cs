using YtDlpGui;
using Xunit;

namespace YtDlpGui.Tests;

/// <summary>O rastreador acha só os arquivos FINAIS (não os temporários "f140.m4a").</summary>
public sealed class RastreadorTests
{
    [Fact]
    public void Video_com_juncao_fica_so_com_o_arquivo_final()
    {
        var r = new RastreadorDeArquivos();
        r.Reiniciar();
        r.ProcessarLinha("[download] Destination: C:/D/Aula.f137.mp4");
        r.ProcessarLinha("[download] Destination: C:/D/Aula.f140.m4a");
        r.ProcessarLinha("[Merger] Merging formats into \"C:/D/Aula.mp4\"");
        r.Encerrar(true, _ => true);

        Assert.Equal(new[] { "C:/D/Aula.mp4" }, r.Finais);
    }

    [Fact]
    public void Mp3_usa_o_destino_da_extracao()
    {
        var r = new RastreadorDeArquivos();
        r.Reiniciar();
        r.ProcessarLinha("[download] Destination: C:/D/Musica.webm");
        r.ProcessarLinha("[ExtractAudio] Destination: C:/D/Musica.mp3");
        r.Encerrar(true, _ => true);

        Assert.Equal(new[] { "C:/D/Musica.mp3" }, r.Finais);
    }
}

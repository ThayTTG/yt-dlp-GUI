using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace YtDlpGui
{
    /// <summary>
    /// Descobre, a partir das linhas que o yt-dlp imprime, QUAIS são os arquivos
    /// finais de um download.
    ///
    /// Por que isso existe: o yt-dlp imprime várias linhas "Destination:" por vídeo,
    /// incluindo arquivos temporários que depois são apagados. Por exemplo, para
    /// um MP4 que precisa juntar vídeo + áudio:
    ///     [download] Destination: Aula.f137.mp4      (temporário)
    ///     [download] Destination: Aula.f140.m4a      (temporário)
    ///     [Merger] Merging formats into "Aula.mp4"   (ESTE é o arquivo final)
    /// e para MP3:
    ///     [download] Destination: Aula.webm          (temporário)
    ///     [ExtractAudio] Destination: Aula.mp3       (ESTE é o arquivo final)
    /// Antes, o histórico guardava o último "Destination:" visto, então um MP4
    /// aparecia como "Aula.f140.m4a", e em playlists cada vídeo entrava DUAS vezes.
    ///
    /// Esta classe não depende de nada do Windows Forms (pode ser testada isolada).
    /// </summary>
    public sealed class RastreadorDeArquivos
    {
        private static readonly Regex RegexDestinoDownload =
            new Regex(@"^\s*\[download\]\s+Destination:\s*(.+?)\s*$", RegexOptions.Compiled);
        private static readonly Regex RegexDestinoExtracao =
            new Regex(@"^\s*\[ExtractAudio\]\s+Destination:\s*(.+?)\s*$", RegexOptions.Compiled);
        private static readonly Regex RegexJuncao =
            new Regex("^\\s*\\[Merger\\]\\s+Merging formats into\\s+\"(.+)\"\\s*$", RegexOptions.Compiled);
        private static readonly Regex RegexJaBaixado =
            new Regex(@"^\s*\[download\]\s+(.+?)\s+has already been downloaded", RegexOptions.Compiled);
        private static readonly Regex RegexNovoItem =
            new Regex(@"Downloading item \d+ of \d+", RegexOptions.Compiled);

        private readonly List<string> _finais = new();
        private readonly List<string> _pendentes = new();

        /// <summary>Arquivos finais já confirmados, na ordem em que foram concluídos.</summary>
        public IReadOnlyList<string> Finais => _finais;

        public void Reiniciar()
        {
            _finais.Clear();
            _pendentes.Clear();
        }

        /// <summary>Interpreta uma linha de saída do yt-dlp.</summary>
        public void ProcessarLinha(string linha)
        {
            if (string.IsNullOrWhiteSpace(linha))
                return;

            var m = RegexJuncao.Match(linha);
            if (m.Success)
            {
                Finalizar(m.Groups[1].Value);
                return;
            }

            m = RegexDestinoExtracao.Match(linha);
            if (m.Success)
            {
                Finalizar(m.Groups[1].Value);
                return;
            }

            m = RegexJaBaixado.Match(linha);
            if (m.Success)
            {
                Finalizar(m.Groups[1].Value);
                return;
            }

            m = RegexDestinoDownload.Match(linha);
            if (m.Success)
            {
                _pendentes.Add(m.Groups[1].Value);
                return;
            }

            if (RegexNovoItem.IsMatch(linha))
            {
                // Começou o próximo item da playlist: o que sobrou pendente do item anterior
                // era um arquivo "simples" (sem junção nem conversão), portanto já é o final.
                PromoverPendente();
            }
        }

        /// <summary>
        /// Chamado quando o yt-dlp termina. Se deu tudo certo, o último pendente é o arquivo final.
        /// Se terminou com erro, o último pendente só vale se o arquivo realmente existir
        /// (um download interrompido deixa apenas um ".part", nunca o nome final).
        /// </summary>
        public void Encerrar(bool terminouComSucesso, Func<string, bool> arquivoExiste)
        {
            if (_pendentes.Count > 0)
            {
                string ultimo = _pendentes[_pendentes.Count - 1];
                if (terminouComSucesso || arquivoExiste(ultimo))
                    AdicionarFinal(ultimo);
            }

            _pendentes.Clear();
        }

        private void Finalizar(string caminho)
        {
            _pendentes.Clear();
            AdicionarFinal(caminho.Trim());
        }

        private void PromoverPendente()
        {
            if (_pendentes.Count > 0)
                AdicionarFinal(_pendentes[_pendentes.Count - 1]);

            _pendentes.Clear();
        }

        private void AdicionarFinal(string caminho)
        {
            // Evita duplicar o mesmo arquivo quando o yt-dlp repete a linha.
            if (_finais.Count == 0 || _finais[_finais.Count - 1] != caminho)
                _finais.Add(caminho);
        }
    }
}

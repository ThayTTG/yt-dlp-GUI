using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YtDlpGui
{
    // =========================================================================
    // MÓDULO OPCIONAL — FILTRO DE CONTEÚDO PARA USO EM SALA DE AULA
    // =========================================================================
    //
    // Este arquivo é 100% autocontido e não é referenciado em nenhum outro
    // lugar do projeto por padrão. Ele só passa a fazer alguma coisa se:
    //
    //   1. Existir um arquivo "filtro-conteudo.json" na mesma pasta do .exe, E
    //   2. Esse arquivo tiver "habilitado": true
    //
    // Sem esse arquivo (ou com "habilitado": false), FiltroConteudo.CarregarConfiguracao()
    // retorna uma configuração "desligada" e o app se comporta exatamente como
    // a versão pública/sem filtro. Isso permite manter o projeto genérico no
    // GitHub — quem clonar o repo não é afetado — enquanto você, localmente,
    // ativa o filtro só distribuindo esse arquivo de configuração junto do seu
    // .exe. Veja o final deste arquivo (e o README) para o passo a passo.
    //
    // O que ele bloqueia:
    //   - Qualquer domínio fora da lista de permitidos (allowlist). Por
    //     padrão, só youtube.com / youtu.be.
    //   - (Opcional) vídeos do YouTube marcados com restrição de idade
    //     (campo "age_limit" retornado pelo próprio yt-dlp), mesmo estando
    //     dentro de um domínio permitido.
    //
    // Em ambos os casos, um "código mestre" (guardado como hash SHA-256, não
    // em texto puro) pode liberar o download pontualmente.
    // =========================================================================

    /// <summary>Configuração do filtro de conteúdo, carregada de um JSON externo.</summary>
    public class ConfiguracaoFiltroConteudo
    {
        public bool Habilitado { get; set; } = false;
        public List<string> DominiosPermitidos { get; set; } = new() { "youtube.com", "youtu.be" };
        public bool BloquearRestricaoDeIdade { get; set; } = true;
        public string? HashCodigoMestre { get; set; }
    }

    /// <summary>Resultado de uma verificação de URL pelo filtro.</summary>
    public class ResultadoFiltro
    {
        public bool Permitido { get; init; }
        public string? Motivo { get; init; }

        public static ResultadoFiltro Ok() => new() { Permitido = true };
        public static ResultadoFiltro Bloqueado(string motivo) => new() { Permitido = false, Motivo = motivo };
    }

    public static class FiltroConteudo
    {
        private const string NomeArquivoConfig = "filtro-conteudo.json";

        /// <summary>
        /// Carrega a configuração do filtro. Se o arquivo não existir ou estiver
        /// corrompido, retorna uma configuração DESLIGADA (comportamento padrão
        /// e seguro para quem não configurou nada — ex.: outros usuários do GitHub).
        /// </summary>
        public static ConfiguracaoFiltroConteudo CarregarConfiguracao()
        {
            try
            {
                string caminho = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NomeArquivoConfig);
                if (!File.Exists(caminho))
                    return new ConfiguracaoFiltroConteudo { Habilitado = false };

                string json = File.ReadAllText(caminho);
                var config = JsonSerializer.Deserialize<ConfiguracaoFiltroConteudo>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return config ?? new ConfiguracaoFiltroConteudo { Habilitado = false };
            }
            catch (Exception ex)
            {
                // Configuração corrompida não deve travar o app nem bloquear downloads por engano.
                // Mas o problema NÃO pode passar em branco: sem este aviso, o filtro ficaria
                // desligado sem que ninguém soubesse o motivo.
                GerenciadorLog.Aviso("filtro-conteudo.json não pôde ser lido; o filtro de conteúdo está DESLIGADO. Motivo: " + ex.Message);
                return new ConfiguracaoFiltroConteudo { Habilitado = false };
            }
        }

        /// <summary>
        /// Usado pelo autodiagnóstico: se o arquivo de configuração existir, confere se ele
        /// é um JSON válido. Retorna null se estiver tudo bem, ou a descrição do problema.
        /// </summary>
        public static string? ValidarArquivoDeConfiguracao()
        {
            try
            {
                string caminho = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NomeArquivoConfig);
                if (!File.Exists(caminho))
                    return null; // sem arquivo = filtro desligado por design

                string json = File.ReadAllText(caminho);
                var config = JsonSerializer.Deserialize<ConfiguracaoFiltroConteudo>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (config is null)
                    return "o arquivo filtro-conteudo.json está vazio; o filtro está DESLIGADO";

                return null;
            }
            catch (Exception ex)
            {
                return "o arquivo filtro-conteudo.json é inválido (" + ex.Message + "); o filtro está DESLIGADO até você corrigir";
            }
        }

        /// <summary>
        /// Verifica se a URL pode ser baixada, de acordo com a configuração
        /// atual. Faz duas checagens: domínio permitido e (opcionalmente)
        /// restrição de idade do próprio YouTube.
        /// </summary>
        public static async Task<ResultadoFiltro> VerificarAsync(
            string url, string caminhoYtDlp, ConfiguracaoFiltroConteudo config)
        {
            if (!config.Habilitado)
                return ResultadoFiltro.Ok();

            if (!DominioPermitido(url, config.DominiosPermitidos))
            {
                return ResultadoFiltro.Bloqueado(
                    "Este site não está na lista de sites permitidos para uso em sala.");
            }

            if (config.BloquearRestricaoDeIdade)
            {
                int limiteIdade = await ObterAgeLimitAsync(caminhoYtDlp, url);
                if (limiteIdade >= 18)
                {
                    return ResultadoFiltro.Bloqueado(
                        "Este vídeo é marcado com restrição de idade (18+) pelo próprio YouTube.");
                }
            }

            return ResultadoFiltro.Ok();
        }

        /// <summary>
        /// Compara se o domínio da URL está na lista de permitidos. Aceita
        /// subdomínios automaticamente (ex.: "youtube.com" na lista libera
        /// também "www.youtube.com", "m.youtube.com", "music.youtube.com"...).
        /// </summary>
        public static bool DominioPermitido(string url, List<string> dominiosPermitidos)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            string host = uri.Host.ToLowerInvariant();

            foreach (var dominio in dominiosPermitidos)
            {
                string d = dominio.Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(d))
                    continue;

                if (host == d || host.EndsWith("." + d, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Pergunta ao yt-dlp os metadados do vídeo (sem baixar nada) e lê o
        /// campo "age_limit". Adiciona alguns segundos de espera antes do
        /// download começar de fato — é o custo dessa checagem extra.
        /// Retorna 0 (sem restrição) se não conseguir determinar.
        /// </summary>
        private static async Task<int> ObterAgeLimitAsync(string caminhoYtDlp, string url)
        {
            if (!File.Exists(caminhoYtDlp))
                return 0;

            try
            {
                var psi = new ProcessStartInfo { FileName = caminhoYtDlp };
                psi.ArgumentList.Add("--dump-json");
                psi.ArgumentList.Add("--no-warnings");
                psi.ArgumentList.Add("--skip-download");
                psi.ArgumentList.Add("--no-playlist");
                psi.ArgumentList.Add(url);

                // Lê stdout e stderr ao mesmo tempo e com tempo limite de 40s: antes, o stderr nunca
                // era lido (risco de travar para sempre se o yt-dlp escrevesse muito) e não havia
                // limite de tempo (rede lenta = tela presa em "Verificando conteúdo...").
                var resultado = await ProcessosFilhos.ExecutarCapturandoAsync(
                    psi, TimeSpan.FromSeconds(40), CancellationToken.None);

                if (resultado.TempoEsgotado)
                {
                    GerenciadorLog.Aviso("Filtro de conteúdo: a verificação de idade demorou demais e foi interrompida. URL: " + url);
                    return 0;
                }

                // IMPORTANTE: vídeos com restrição de idade geralmente NÃO devolvem metadados sem
                // login; o yt-dlp responde com um erro tipo "Sign in to confirm your age". Antes,
                // esse caso caía no "não consegui determinar" e o vídeo era LIBERADO — justamente o
                // tipo de vídeo que o filtro deveria bloquear. Agora esse erro conta como 18+.
                if (IndicaRestricaoDeIdade(resultado.Erro))
                    return 18;

                if (string.IsNullOrWhiteSpace(resultado.Saida))
                {
                    GerenciadorLog.Aviso("Filtro de conteúdo: não foi possível obter os metadados para checar a idade (liberado por padrão). URL: " + url +
                                         " | Detalhe: " + Resumir(resultado.Erro));
                    return 0;
                }

                using var doc = JsonDocument.Parse(resultado.Saida);
                if (doc.RootElement.TryGetProperty("age_limit", out var prop) &&
                    prop.ValueKind == JsonValueKind.Number)
                {
                    return prop.GetInt32();
                }
            }
            catch (Exception ex)
            {
                // Se não der pra determinar (JSON inválido, vídeo indisponível, etc.),
                // não bloqueia por causa disso — assume sem restrição de idade, mas deixa registrado.
                GerenciadorLog.Aviso("Filtro de conteúdo: falha ao checar restrição de idade (liberado por padrão): " + ex.Message);
            }

            return 0;
        }

        private static bool IndicaRestricaoDeIdade(string? erro)
        {
            if (string.IsNullOrWhiteSpace(erro))
                return false;

            string[] marcas =
            {
                "confirm your age",
                "age-restricted",
                "age restricted",
                "inappropriate for some users"
            };

            foreach (string marca in marcas)
            {
                if (erro.Contains(marca, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string Resumir(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "(sem detalhes)";

            string limpo = texto.Replace("\r", " ").Replace("\n", " ").Trim();
            return limpo.Length > 300 ? limpo.Substring(0, 300) + "..." : limpo;
        }

        // ---------------------------------------------------------------
        // Código mestre
        // ---------------------------------------------------------------

        /// <summary>Calcula o hash SHA-256 (hexadecimal) de um texto.</summary>
        public static string CalcularHash(string texto)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
            var sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>Confere se o código digitado bate com o hash configurado.</summary>
        public static bool CodigoMestreValido(string tentativa, ConfiguracaoFiltroConteudo config)
        {
            if (string.IsNullOrEmpty(config.HashCodigoMestre) || string.IsNullOrWhiteSpace(tentativa))
                return false;

            string hashTentativa = CalcularHash(tentativa.Trim());
            return string.Equals(hashTentativa, config.HashCodigoMestre, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Mostra um pequeno diálogo pedindo o código mestre e retorna true se
        /// o código digitado for válido. Todo construído em código (sem
        /// depender de nenhum arquivo Designer), para manter o módulo num
        /// único arquivo.
        /// </summary>
        public static bool SolicitarCodigoMestre(IWin32Window janelaPai, ConfiguracaoFiltroConteudo config, string motivoBloqueio)
        {
            using var dialogo = new CodigoMestreDialog(motivoBloqueio);
            if (dialogo.ShowDialog(janelaPai) != DialogResult.OK)
                return false;

            bool valido = CodigoMestreValido(dialogo.CodigoDigitado, config);
            if (!valido)
            {
                MessageBox.Show(janelaPai as Form, "Código mestre incorreto.", "Acesso negado",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return valido;
        }

        /// <summary>Diálogo simples de senha, construído inteiramente via código.</summary>
        private class CodigoMestreDialog : Form
        {
            private readonly TextBox _txtCodigo;
            public string CodigoDigitado => _txtCodigo.Text;

            public CodigoMestreDialog(string motivoBloqueio)
            {
                Text = "Conteúdo bloqueado";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MinimizeBox = false;
                MaximizeBox = false;
                ClientSize = new System.Drawing.Size(380, 170);

                var lblMotivo = new Label
                {
                    Text = motivoBloqueio,
                    Location = new System.Drawing.Point(16, 16),
                    Size = new System.Drawing.Size(348, 40),
                };

                var lblCodigo = new Label
                {
                    Text = "Digite o código mestre do professor para continuar:",
                    Location = new System.Drawing.Point(16, 62),
                    Size = new System.Drawing.Size(348, 18),
                };

                _txtCodigo = new TextBox
                {
                    Location = new System.Drawing.Point(16, 84),
                    Size = new System.Drawing.Size(348, 24),
                    UseSystemPasswordChar = true,
                };

                var btnOk = new Button
                {
                    Text = "Liberar",
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(196, 122),
                    Size = new System.Drawing.Size(80, 30),
                };

                var btnCancelar = new Button
                {
                    Text = "Cancelar",
                    DialogResult = DialogResult.Cancel,
                    Location = new System.Drawing.Point(284, 122),
                    Size = new System.Drawing.Size(80, 30),
                };

                Controls.Add(lblMotivo);
                Controls.Add(lblCodigo);
                Controls.Add(_txtCodigo);
                Controls.Add(btnOk);
                Controls.Add(btnCancelar);

                AcceptButton = btnOk;
                CancelButton = btnCancelar;
            }
        }
    }
}

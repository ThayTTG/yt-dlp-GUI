using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace YtDlpGui
{
    /// <summary>
    /// O que muda entre Windows e Linux, num lugar só: nomes dos programas auxiliares
    /// (com ou sem ".exe"), a permissão de execução do Linux e a procura no PATH.
    /// O resto do app (telas, regras do download, histórico) é igual nos dois sistemas.
    /// </summary>
    public static class Plataforma
    {
        public static bool EhWindows => OperatingSystem.IsWindows();

        /// <summary>"yt-dlp" → "yt-dlp.exe" no Windows; igual no Linux.</summary>
        public static string Executavel(string nome) => EhWindows ? nome + ".exe" : nome;

        public static string YtDlp => Executavel("yt-dlp");
        public static string Ffmpeg => Executavel("ffmpeg");
        public static string Deno => Executavel("deno");

        /// <summary>Nome do executável do yt-dlp na página de lançamentos oficial.</summary>
        public static string YtDlpNoGitHub => EhWindows ? "yt-dlp.exe" : "yt-dlp_linux";

        /// <summary>
        /// No Linux, um arquivo só roda com a permissão de execução (o "chmod +x"). Baixar ou
        /// extrair às vezes perde essa permissão, então o app confere antes de usar.
        /// </summary>
        public static void GarantirExecutavel(string caminho)
        {
            if (EhWindows || !File.Exists(caminho))
                return;

            try
            {
                var modo = File.GetUnixFileMode(caminho);
                var comExecucao = modo | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
                if (comExecucao != modo)
                {
                    File.SetUnixFileMode(caminho, comExecucao);
                    GerenciadorLog.Reparo("Permissão de execução ligada em: " + caminho);
                }
            }
            catch (Exception ex)
            {
                GerenciadorLog.Aviso("Não foi possível ligar a permissão de execução de " + caminho + ": " + ex.Message);
            }
        }

        /// <summary>Procura um programa nas pastas do PATH (ex.: ffmpeg instalado pelo apt). Null se não achar.</summary>
        public static string? ProcurarNoPath(string nomeExecutavel)
        {
            string? path = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(path))
                return null;

            foreach (string pasta in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    string candidato = Path.Combine(pasta, nomeExecutavel);
                    if (File.Exists(candidato))
                        return candidato;
                }
                catch { }
            }
            return null;
        }

        /// <summary>
        /// Navegador padrão no Linux, pelo "xdg-settings" (ex.: "firefox.desktop",
        /// "google-chrome.desktop"). Null se não der para descobrir.
        /// </summary>
        public static string? NavegadorPadraoNoLinux()
        {
            if (EhWindows)
                return null;

            try
            {
                var psi = new ProcessStartInfo("xdg-settings")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                psi.ArgumentList.Add("get");
                psi.ArgumentList.Add("default-web-browser");

                using var p = Process.Start(psi);
                if (p is null)
                    return null;
                string saida = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit(3000);
                return string.IsNullOrEmpty(saida) ? null : saida;
            }
            catch
            {
                return null; // sem xdg-settings: o usuário escolhe o navegador na lista
            }
        }

        /// <summary>Notificação do sistema no Linux (notify-send). Melhor esforço: se não houver, não faz nada.</summary>
        public static void NotificarNoLinux(string titulo, string mensagem, bool erro)
        {
            if (EhWindows)
                return;

            try
            {
                var psi = new ProcessStartInfo("notify-send") { UseShellExecute = false, CreateNoWindow = true };
                psi.ArgumentList.Add("--app-name=YT-DLP GUI");
                if (erro)
                    psi.ArgumentList.Add("--urgency=critical");
                psi.ArgumentList.Add(titulo);
                psi.ArgumentList.Add(mensagem);
                using var p = Process.Start(psi);
            }
            catch
            {
                // Sem notify-send (ou sem área de notificações): só fica no log.
            }
        }

        /// <summary>Descrição curta do sistema para os logs (ex.: "Linux X64").</summary>
        public static string Descricao =>
            (EhWindows ? "Windows" : OperatingSystem.IsLinux() ? "Linux" : RuntimeInformation.OSDescription) +
            " " + RuntimeInformation.OSArchitecture;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YtDlpGui
{
    /// <summary>Resultado de uma execução de processo com saída capturada.</summary>
    public sealed record ResultadoProcesso(
        int CodigoSaida,
        string Saida,
        string Erro,
        bool Iniciou,
        bool TempoEsgotado,
        bool Cancelado);

    /// <summary>
    /// Controla os processos filhos (yt-dlp.exe) iniciados pelo programa.
    ///
    /// Por que isso existe: antes, se o programa fechasse de forma inesperada,
    /// ou se uma consulta de prévia fosse "cancelada" só no papel, o yt-dlp.exe
    /// continuava rodando escondido, gastando rede e CPU. Agora cada processo
    /// iniciado é registrado e é encerrado junto com o programa.
    /// </summary>
    public static class ProcessosFilhos
    {
        private static readonly object Trava = new();
        private static readonly HashSet<int> Pids = new();

        public static void Registrar(int pid)
        {
            lock (Trava)
            {
                Pids.Add(pid);
            }
        }

        public static void Remover(int pid)
        {
            lock (Trava)
            {
                Pids.Remove(pid);
            }
        }

        /// <summary>Encerra (com toda a árvore) qualquer processo filho ainda registrado.</summary>
        public static void EncerrarTodos()
        {
            int[] copia;
            lock (Trava)
            {
                copia = new int[Pids.Count];
                Pids.CopyTo(copia);
                Pids.Clear();
            }

            foreach (int pid in copia)
            {
                try
                {
                    using var processo = Process.GetProcessById(pid);
                    if (!processo.HasExited)
                    {
                        processo.Kill(entireProcessTree: true);
                        GerenciadorLog.Reparo("Processo filho (PID " + pid + ") encerrado junto com o programa.");
                    }
                }
                catch
                {
                    // Já terminou, ou não temos permissão: nada a fazer.
                }
            }
        }

        /// <summary>Encerra um processo e sua árvore, sem nunca lançar exceção.</summary>
        public static void Encerrar(Process? processo)
        {
            if (processo is null)
                return;

            try
            {
                if (!processo.HasExited)
                    processo.Kill(entireProcessTree: true);
            }
            catch (Exception ex)
            {
                GerenciadorLog.Aviso("Não foi possível encerrar um processo filho: " + ex.Message);
            }
        }

        /// <summary>
        /// Executa um processo, captura stdout e stderr ao mesmo tempo (evitando o
        /// travamento clássico em que um pipe enche enquanto o outro é esperado),
        /// com tempo limite e cancelamento que REALMENTE encerram o processo.
        /// Nunca lança exceção: problemas vêm descritos no resultado e no log.
        /// </summary>
        public static async Task<ResultadoProcesso> ExecutarCapturandoAsync(
            ProcessStartInfo psi, TimeSpan limite, CancellationToken cancelamento)
        {
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;
            psi.Environment["PYTHONIOENCODING"] = "utf-8";

            Process? processo;
            try
            {
                processo = Process.Start(psi);
            }
            catch (Exception ex)
            {
                GerenciadorLog.Erro("Não foi possível iniciar o processo '" + psi.FileName + "'", ex);
                return new ResultadoProcesso(-1, "", ex.Message, false, false, false);
            }

            if (processo is null)
                return new ResultadoProcesso(-1, "", "O processo não foi iniciado.", false, false, false);

            int pid = processo.Id;
            ProcessosFilhos.Registrar(pid);

            using (processo)
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelamento))
            {
                cts.CancelAfter(limite);

                Task<string>? tarefaSaida = null;
                Task<string>? tarefaErro = null;

                try
                {
                    tarefaSaida = processo.StandardOutput.ReadToEndAsync(cts.Token);
                    tarefaErro = processo.StandardError.ReadToEndAsync(cts.Token);

                    await processo.WaitForExitAsync(cts.Token);

                    string saida = await tarefaSaida;
                    string erro = await tarefaErro;
                    return new ResultadoProcesso(processo.ExitCode, saida, erro, true, false, false);
                }
                catch (OperationCanceledException)
                {
                    ProcessosFilhos.Encerrar(processo);

                    // Dá um instante para as leituras pendentes terminarem e "observa" qualquer falha delas.
                    try
                    {
                        if (tarefaSaida is not null && tarefaErro is not null)
                            await Task.WhenAll(tarefaSaida, tarefaErro).WaitAsync(TimeSpan.FromSeconds(2));
                    }
                    catch
                    {
                        // Esperado: as leituras foram canceladas.
                    }

                    bool canceladoPorQuemChamou = cancelamento.IsCancellationRequested;
                    return new ResultadoProcesso(-1, "", "", true, !canceladoPorQuemChamou, canceladoPorQuemChamou);
                }
                catch (Exception ex)
                {
                    GerenciadorLog.Erro("Falha ao executar '" + psi.FileName + "'", ex);
                    ProcessosFilhos.Encerrar(processo);
                    return new ResultadoProcesso(-1, "", ex.Message, true, false, false);
                }
                finally
                {
                    ProcessosFilhos.Remover(pid);
                }
            }
        }
    }
}

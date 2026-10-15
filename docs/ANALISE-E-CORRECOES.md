# Análise de integridade do YtDlpGui — o que foi encontrado e o que mudou

Data da análise: 13/10/2026 · Versão do programa após as mudanças: 1.1.0

> **Importante — leia antes de usar:** esta revisão foi feita **sem compilar** o
> projeto (o ambiente onde o código foi escrito não tinha acesso ao SDK do .NET
> nem ao NuGet). Foram feitas conferências manuais (balanceamento de chaves,
> referências a nomes que não existem mais, simulação das expressões regulares e
> da lógica de arquivos finais em Python), mas **o primeiro passo é compilar no
> Windows** (`dotnet build`) e rodar o roteiro de testes do final deste arquivo.
> Se aparecer algum erro de compilação, ele deve ser pequeno (um `using`, um tipo);
> mande a mensagem de erro para corrigirmos.

## 1. Resumo

Foram integrados os dois protocolos pedidos e, na análise, apareceram outros
problemas estruturais — alguns com chance real de causar falha durante o uso.

**Protocolos integrados**

1. **Log de erros em .txt na pasta de origem** → `GerenciadorLog.cs`
   (`log-erros.txt` e `log-atividade.txt`, ao lado do `YtDlpGui.exe`).
2. **Sistema de debug interno que tenta corrigir a si mesmo** → `AutoReparo.cs`
   (autodiagnóstico na abertura, reparo do histórico, reparo do `yt-dlp.exe`,
   atualização + nova tentativa automática, detecção de queda da sessão anterior) +
   rede de proteção global em `Program.cs`.

**Arquivos novos:** `GerenciadorLog.cs`, `AutoReparo.cs`, `ProcessosFilhos.cs`,
`RastreadorDeArquivos.cs`, este relatório.
**Arquivos alterados:** `Program.cs`, `MainForm.cs`, `GerenciadorHistorico.cs`,
`FiltroConteudo.cs`, `YtDlpGui.csproj` (só adiciona `<Version>`), `README.md`, `LEIA-ME.txt`.
`MainForm.Designer.cs` e `publicar.ps1` **não foram tocados**.

## 2. Problemas encontrados e corrigidos

### Alta gravidade (podiam derrubar o programa, travá-lo ou burlar o filtro)

| # | Onde | Problema | Correção |
|---|------|----------|----------|
| 1 | `Program.cs` | Nenhum tratamento global de exceções. Qualquer erro dentro de um botão (todos são `async void`) encerrava o programa sem deixar rastro. | Handlers globais (`ThreadException`, `UnhandledException`, `UnobservedTaskException`) que gravam no log e mantêm o programa aberto quando possível. |
| 2 | `MainForm` (downloads) | **Reentrância:** durante "Verificando conteúdo..." os botões continuavam ativos. Um segundo clique (ou MP4 + MP3) iniciava dois yt-dlp ao mesmo tempo, sobrescrevia `_processoAtual` e o botão Cancelar perdia o primeiro processo. | Trava `_ocupado` em todas as operações + controles desabilitados antes do filtro. |
| 3 | `MainForm` + `FiltroConteudo` | **Filtro burlável:** a URL era verificada e depois **relida** da caixa de texto para montar o download. Dava para trocar o endereço durante a verificação. | A URL é "fotografada" uma vez e usada em tudo; campos travados durante a verificação. |
| 4 | `FiltroConteudo.ObterAgeLimitAsync` | **O filtro de idade falhava "aberto" justamente nos vídeos que deveria bloquear:** vídeo 18+ sem login não devolve o JSON, o yt-dlp responde com "Sign in to confirm your age", o JSON vazio gerava exceção e o código assumia "sem restrição". | Esse erro agora conta como 18+ (bloqueia). Outras falhas continuam liberando, mas ficam registradas no log. |
| 5 | `BuscarPreviaAsync` | **Processos órfãos:** o "cancelamento" da prévia só cancelava a espera; o `yt-dlp.exe` continuava rodando escondido. Cada URL colada/editada podia deixar um processo vivo. | `ProcessosFilhos.ExecutarCapturandoAsync` mata o processo de verdade no cancelamento/timeout; todos os filhos são encerrados ao fechar o programa. |
| 6 | `BuscarPreviaAsync` e `ObterAgeLimitAsync` | **Risco de travamento permanente:** `stderr` era redirecionado e nunca lido (se o yt-dlp escrevesse bastante, o pipe enchia e o programa esperava para sempre) e não havia tempo limite (tela presa em "Verificando conteúdo..." sem saída). | Leitura simultânea de stdout/stderr + tempo limite de 40 s. |
| 7 | `GerenciadorHistorico` | Histórico corrompido era tratado como "vazio" **e depois sobrescrito**, destruindo o que dava para recuperar. Gravação direta (`WriteAllText`): queda de energia no meio da gravação corrompia o arquivo. Sem trava contra gravações simultâneas. | Quarentena (`historico.corrompido-*.json`, 5 mais recentes), gravação atômica (temporário + troca), `lock`, tudo no log. |

### Média gravidade (resultado errado ou degradação com o tempo)

| # | Onde | Problema | Correção |
|---|------|----------|----------|
| 8 | `TratarLinha` / histórico | O histórico guardava o **último** `Destination:` visto. Num MP4 mesclado isso era um arquivo temporário (`Aula.f140.m4a`); em playlists cada vídeo entrava **duas vezes** (temporário + final; no MP3, `.webm` + `.mp3`). | `RastreadorDeArquivos` considera só os arquivos finais (`[Merger]`, `[ExtractAudio]`, "already been downloaded"). Lógica simulada com 4 cenários. |
| 9 | `ExecutarYtDlpAsync` | Playlist com um vídeo indisponível → código de saída ≠ 0 → "Falha no download" e **nada** dos outros vídeos ia para o histórico. | "Concluído com avisos": registra o que foi baixado e informa o que falhou. |
| 10 | `BuscarPreviaAsync` | `Image.FromStream` com o `MemoryStream` descartado logo em seguida. O GDI+ exige o stream aberto; pode dar erro na hora de desenhar a miniatura. | Cópia para um `Bitmap` independente. |
| 11 | `MainForm` (imagens) | Vazamento de GDI+: a cada troca de tela eram relidas 3 imagens do disco (sem `Dispose`), `MostrarLogoPadrao` relia o logo a cada limpeza da prévia, miniaturas antigas não eram liberadas, e os `.png` ficavam travados no disco. | Cache de imagens (leitura única, em memória) e `Dispose` da miniatura anterior. |
| 12 | `txtUrl_TextChanged` | Uma busca antiga que terminava **desligava o spinner da busca nova**; apagar a URL durante a busca podia deixar o spinner girando para sempre. | Só desliga o spinner a busca mais recente; `MostrarLogoPadrao` também para o spinner. |
| 13 | `RegistrarLog` (antigo) | Abria e fechava o arquivo **a cada linha** de saída (inclusive cada atualização de %), na thread da tela, sem limite de tamanho nem rotação, e falhas eram engolidas. | `GerenciadorLog` (rotação, anti-enxurrada, plano B de pasta); progresso registrado de 25 em 25%. |
| 14 | `ProcessarLinhaDeSaida` / fechamento | Saída do yt-dlp chegando depois da janela fechada → `ObjectDisposedException`. Fechar a janela no meio de um download cancelava tudo sem aviso. | Guardas de janela fechada; confirmação ao fechar com download em andamento. |
| 15 | `btnExtrairCookies_Click` | Sem tempo limite: navegador travando o arquivo de cookies deixava a tela presa. | Limite de 2 minutos e processo encerrado corretamente. |
| 16 | `ValidarEntradas` | `Uri.TryCreate(..., Absolute)` aceita `C:\Windows`, `file://`, `ftp://`... e disparava o yt-dlp para isso. | Só `http://` e `https://`. |
| 17 | `RegistrarHistoricoDoDownload` | A pasta da playlist era montada "na mão" (`Path.Combine(pasta, nomeDaPlaylist)`), mas o yt-dlp troca caracteres proibidos no nome da pasta → "A pasta deste item não existe mais". Com "Playlist" marcado e URL de vídeo único, criava a pasta `NA` e o item "NA (1 vídeos)". | Usa a pasta real do arquivo final; vídeo único com "Playlist" marcado é tratado como vídeo único. |
| 18 | `menuItemAbrirPasta_Click` | `Process.Start` sem `try/catch`. | Protegido e registrado. |
| 19 | `btnExtrairCookies_Click` | `cookies.txt` gravado na pasta do programa sem checar permissão. | Usa a mesma pasta "gravável" dos logs (a do programa, ou o plano B). |

### Novas proteções (os protocolos pedidos)

- **Log de erros/atividade** em `.txt` na pasta de origem, com plano B, rotação e anti-enxurrada.
- **Autodiagnóstico na abertura** (e sob demanda com **Ctrl+Shift+D**): sessão anterior caída, pasta de logs, histórico, `yt-dlp.exe` (existe? tamanho? começa com "MZ"?), `ffmpeg.exe`, `Assets`, `filtro-conteudo.json`.
- **Reparo com consentimento:** se o `yt-dlp.exe` sumiu ou está danificado, o programa **pergunta** e baixa o oficial do GitHub, conferindo o SHA-256 publicado na própria release antes de usar (o arquivo ruim é guardado como `yt-dlp.exe.corrompido`).
- **Reparo automático (1× por sessão):** erro típico de yt-dlp desatualizado → atualiza e repete o download.
- **Ctrl+Shift+L** abre a pasta de logs.
- Mensagens de erro novas em português: disco cheio, sem permissão, sem internet, limite de acessos (429), URL não suportada, vídeo com restrição de idade.
- Aviso de pouco espaço em disco (< 500 MB) antes de baixar.

## 3. Pontos de atenção que NÃO foram alterados (decisão sua)

1. **O filtro de conteúdo é desligável por quem tem acesso à pasta.** Se `filtro-conteudo.json` for apagado, editado para `"habilitado": false` ou deixado com JSON inválido, o filtro simplesmente liga em "desligado" (de propósito, para quem baixou do GitHub). Para uso em sala, vale proteger a pasta (somente leitura para alunos) ou ler a configuração de um local que só administradores gravem. Agora, ao menos, um arquivo inválido fica registrado no log e o autodiagnóstico avisa.
2. **O código mestre usa SHA-256 sem sal.** Códigos curtos podem ser descobertos por força bruta em segundos por quem copiar o hash. Se isso importar, o ideal é `Rfc2898DeriveBytes.Pbkdf2` com sal e muitas iterações.
3. **`cookies.txt` guarda a sessão logada** (a conta do YouTube/Google de quem extraiu) em texto puro na pasta do programa. Se essa pasta for compartilhada com alunos, qualquer um pode copiar o arquivo. Prefira uma conta dedicada, ou um local por usuário (`%AppData%`), e apague o arquivo depois de usar. (Não mudei o local porque o README e o LEIA-ME descrevem a pasta do programa.)
4. **`ApplicationHighDpiMode` no `.csproj` provavelmente não tem efeito:** o `Program.cs` não chama `ApplicationConfiguration.Initialize()`, que é quem aplica essa propriedade, então em telas com escala acima de 100% a janela fica "esticada"/borrada. Mudar isso pode alterar o layout desenhado à mão, então deixei para você testar.
5. **`publicar.ps1` baixa `yt-dlp` e `ffmpeg` sem conferir hash** e sempre "latest" (build não reproduzível). O mesmo método de conferência usado no autorreparo (`SHA2-256SUMS`) poderia ser aplicado ali.
6. **Sem autorreparo do `ffmpeg.exe`:** é um arquivo grande (~100 MB); hoje só há aviso. Dá para fazer algo parecido com o do yt-dlp, se quiser.
7. As mensagens de erro do yt-dlp são reconhecidas pelo **texto em inglês**; se o yt-dlp mudar a redação, algumas traduções/reparos deixam de casar (o erro bruto continua sendo mostrado e registrado).
8. Os logs contêm **URLs baixadas e o nome do computador**. Em laboratório compartilhado, vale limpar de tempos em tempos.

## 4. Roteiro de testes sugerido (no Windows)

1. `dotnet build` — deve compilar sem erros.
2. Abrir o programa → conferir que `log-erros.txt`/`log-atividade.txt` aparecem ao lado do `.exe` e o cabeçalho traz versão e PC.
3. Colocar o programa em uma pasta sem permissão de gravação (ex.: `C:\Program Files\...`) → os logs devem ir para `%AppData%\YtDlpGui` com um aviso "REPARO" no log.
4. Renomear `yt-dlp.exe` → ao abrir, deve perguntar se pode baixar o oficial; aceitar e conferir o resultado (`REPARO` no log).
5. Estragar o `historico.json` (apagar uma `}`) → abrir o programa: deve aparecer `historico.corrompido-*.json` ao lado e o histórico recomeçar vazio.
6. Colar uma URL, apagar, colar outra rapidamente → no Gerenciador de Tarefas não deve sobrar `yt-dlp.exe` rodando depois.
7. Baixar um MP4 e um MP3 → o histórico deve mostrar `Nome.mp4` / `Nome.mp3` (e não `Nome.f140.m4a`).
8. Baixar uma playlist pequena com **e** sem "organizar em subpastas" → uma entrada por vídeo (ou uma da playlist), "Abrir pasta" funcionando.
9. Com o filtro ligado, testar um vídeo 18+ → deve ser bloqueado.
10. Dar dois cliques rápidos em "Baixar .MP4" → só um download.
11. Fechar o programa no meio de um download → deve perguntar; ao confirmar, nenhum `yt-dlp.exe` fica rodando.
12. Matar o programa pelo Gerenciador de Tarefas e abrir de novo → o log deve registrar "A sessão anterior terminou de forma inesperada".
13. Ctrl+Shift+D (resumo do autodiagnóstico) e Ctrl+Shift+L (abre a pasta de logs).

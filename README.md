# YtDlpGui — Interface gráfica para o yt-dlp

Aplicação desktop em **C# / .NET 8 (WinForms)** que permite baixar vídeos e
áudios do YouTube sem que o aluno precise usar o CMD ou o PowerShell.

Visual baseado em um mockup próprio: barra lateral de navegação, telas
separadas por função, paleta sálvia + menta + azul, sem exibir a saída
bruta do yt-dlp em nenhum momento.

## Estrutura dos arquivos

```
YtDlpGui/
├── YtDlpGui.csproj            # projeto (.NET 8, WinForms, pacote Guna.UI2.WinForms)
├── Program.cs                  # ponto de entrada da aplicação
├── MainForm.cs                  # toda a lógica: navegação, download, notificações
├── MainForm.Designer.cs         # layout visual (barra lateral + 4 páginas + status bar)
├── GerenciadorHistorico.cs      # persistência do histórico em JSON (%AppData%)
├── FiltroConteudo.cs             # MÓDULO OPCIONAL: bloqueio de sites (ver seção 9)
├── filtro-conteudo.exemplo.json  # exemplo de configuração do filtro (inerte por si só)
├── icon.ico                     # ícone do app (janela, taskbar, .exe)
├── Assets/                      # ícones e ilustrações (logo, navegação, botões)
├── publicar.ps1                 # script que gera o pacote pronto para distribuir
├── LEIA-ME.txt                  # instruções para o usuário final (vai dentro do pacote)
└── README.md                    # este arquivo (documentação técnica)
```

## 1. Onde colocar o `yt-dlp.exe` e o `ffmpeg.exe`

O aplicativo espera encontrar os dois executáveis na **mesma pasta** do
`.exe` da aplicação (a pasta de saída da compilação, ex.:
`bin\Debug\net8.0-windows\`, ou a pasta onde o programa for distribuído):

```csharp
string pastaApp = AppDomain.CurrentDomain.BaseDirectory;
_ytDlpPath = Path.Combine(pastaApp, "yt-dlp.exe");
_ffmpegPath = Path.Combine(pastaApp, "ffmpeg.exe");
```

- **yt-dlp.exe**: `https://github.com/yt-dlp/yt-dlp/releases` (arquivo `yt-dlp.exe`)
- **ffmpeg.exe**: `https://www.gyan.dev/ffmpeg/builds/` (build "essentials", pasta `bin`)

## 2. Arquitetura da interface (barra lateral + páginas)

Diferente da versão anterior (tudo em uma única tela com log rolável), o
app agora é dividido em uma **casca de navegação** (`MainForm`) com quatro
"páginas" — painéis que ocupam a mesma área e alternam visibilidade:

| Painel               | Conteúdo                                             |
|-----------------------|-------------------------------------------------------|
| `pnlHome`              | Campo de URL + botões "Baixar .MP4" / "Baixar .MP3" + toggle de playlist |
| `pnlHistorico`         | Lista (Tipo/Nome/Data) dos downloads já concluídos    |
| `pnlPasta`             | Local de salvamento + botão "Alterar Local"           |
| `pnlConfiguracoes`     | Autenticação por cookies + botão "Atualizar yt-dlp"   |

A troca de página é feita por `MostrarPagina(string pagina)` em
`MainForm.cs`, chamada pelos cliques nos ícones da barra lateral
(`picLogo`, `picNavHistorico`, `picNavPasta`, `picNavConfig`). Os ícones
trocam de cor (branco → azul) conforme a página ativa, via
`AtualizarIconesDeNavegacao`.

Uma **barra de status fixa** (`pnlStatusBar`) fica sempre visível na parte
inferior, independente da página aberta, mostrando o estágio atual do
download e a barra de progresso — e o botão vermelho circular "Cancelar"
fica sempre acessível no canto inferior direito da área de conteúdo.

## 3. Sem terminal / log bruto

A versão anterior tinha uma `RichTextBox` mostrando linha por linha a
saída do yt-dlp — útil para desenvolvedores, mas confusa para um usuário
comum. Isso foi removido da interface. Em vez disso:

- `TratarLinha()` interpreta cada linha da saída do yt-dlp e converte em
  uma frase curta e amigável, exibida em `lblStatus` (ex.: "Baixando...
  45%", "Convertendo para MP3...", "Finalizando arquivo...").
- Toda a saída bruta (stdout + stderr) continua sendo gravada, mas
  **silenciosamente**, em um arquivo de log para fins de suporte/depuração:
  `%AppData%\YtDlpGui\log.txt` (função `RegistrarLog`). Isso preserva a
  possibilidade de diagnosticar problemas sem expor o usuário a jargão
  técnico.
- Em caso de falha, `TraduzirErro()` mapeia as mensagens de erro mais
  comuns do yt-dlp (bloqueio anti-bot, ffmpeg ausente, vídeo indisponível,
  vídeo privado) para frases em português, exibidas em um `MessageBox`
  simples — sem o texto original do yt-dlp na tela.

## 4. Notificações do Windows

Ao final de um download (sucesso ou falha), o app dispara uma notificação
usando `NotifyIcon.ShowBalloonTip` (método `MostrarNotificacao` em
`MainForm.cs`):

- **Sucesso**: título "Download concluído", mensagem com o nome do
  arquivo salvo.
- **Falha**: título "Falha no download", mensagem com o erro traduzido
  (mesma lógica de `TraduzirErro`).
- **Atualização do yt-dlp**: notificação própria ("yt-dlp atualizado"),
  sem gerar entrada no histórico.

Isso permite que o aluno minimize a janela ou troque de tela enquanto o
download roda em segundo plano, e ainda saiba quando terminou.

> Notificações balão (`ShowBalloonTip`) são a API nativa mais simples do
> WinForms para esse fim. Elas usam o ícone da bandeja do sistema
> (`notifyIcon1`, criado oculto/visível conforme necessário). Se quiser
> "toasts" no estilo Windows 11 mais recente, seria necessário migrar para
> a API `Microsoft.Toolkit.Uwp.Notifications` — não incluída aqui para
> manter o projeto sem dependências extras além do Guna.UI2.

## 5. Histórico de downloads

Persistido em JSON por `GerenciadorHistorico`
(`%AppData%\YtDlpGui\historico.json`), sem depender de banco de dados.
Cada item guarda tipo (MP4/MP3), nome do arquivo (capturado da saída do
yt-dlp via `RegexDestino`), pasta e data/hora. A tela de Histórico permite
abrir a pasta de um item (duplo clique ou menu de contexto) e limpar tudo.

Ações sem tipo associado (como "Atualizar yt-dlp") não entram na lista
exibida — o filtro está em `CarregarHistorico()`.

**Comportamento com playlists** (corrigido — antes só o último vídeo da
playlist entrava no histórico): o método `RegistrarHistoricoDoDownload()`
decide como registrar com base no checkbox "Organizar cada playlist em uma
subpasta própria" (tela Configurações):

- **Desmarcado** (padrão): um item de histórico **para cada vídeo**
  baixado da playlist, capturados durante o download em
  `_arquivosBaixadosNestaSessao` (uma lista, não mais uma única variável).
- **Marcado**: um único item representando a playlist inteira (nome +
  quantidade de vídeos), apontando para a subpasta criada.

## 6. Autenticação por cookies

Página "Configurações" com três opções (agora **corretamente exclusivas**
entre si — veja o bug abaixo):

- **Não usar cookies** (padrão)
- **Cookies do navegador**: `--cookies-from-browser <navegador>`
- **Arquivo cookies.txt**: `--cookies <caminho>`, selecionado via
  `OpenFileDialog`. Um botão "?" ao lado do título abre a página oficial do
  yt-dlp com instruções de como exportar esse arquivo do navegador
  (`btnAjudaCookies_Click`), já que testers relataram confusão sobre como
  "fabricar" esse arquivo sozinho. O campo também ganhou um
  `PlaceholderText` explicativo.

Resolve o erro "Sign in to confirm you're not a bot" do YouTube.

### Bug corrigido: os dois radios ficavam marcados ao mesmo tempo

`Guna2RadioButton`, diferente do `RadioButton` nativo do WinForms, **não**
desmarca automaticamente os "irmãos" no mesmo container quando um é
marcado — isso é comportamento do Win32 `BM_CLICK`/grupo nativo, que o
Guna2RadioButton não replica sozinho. Resultado: era possível marcar
"cookies do navegador" e, ao trocar de tela e voltar, ver "cookies.txt"
marcado — na real os dois estavam marcados ao mesmo tempo por baixo dos
panos, e a exibição só "escolhia" um pra mostrar.

Corrigido em `OpcaoDeAutenticacao_CheckedChanged`: agora a exclusividade é
forçada manualmente (desmarca os outros dois sempre que um é marcado),
com uma flag `_atualizandoOpcaoDeAutenticacao` para evitar reentrância
infinita do evento `CheckedChanged`.

## 7. Cancelar download, playlist e organização em subpastas

- Botão circular vermelho "X" (`btnCancelar`) mata o processo do yt-dlp
  (e o ffmpeg filho) via `Process.Kill(entireProcessTree: true)`.
- Toggle "Baixar Playlist" na Home controla `--yes-playlist` /
  `--no-playlist` (desmarcado por padrão, mais seguro para uso educacional).
- Novo checkbox **"Organizar cada playlist em uma subpasta própria"**
  (tela Configurações, `chkOrganizarPlaylist`): quando marcado junto com
  "Baixar Playlist", o template de saída passa de `%(title)s.%(ext)s` para
  `%(playlist_title)s/%(title)s.%(ext)s` (método `ObterTemplateDeSaida()`),
  fazendo o próprio yt-dlp criar a subpasta com o nome da playlist dentro
  do local de salvamento escolhido.
- Durante o download de uma playlist, o status agora mostra "Baixando
  vídeo X de Y..." (capturado via `RegexPlaylistItem`, que interpreta as
  linhas `Downloading item X of Y` do yt-dlp), além da porcentagem do
  vídeo atual.

## 8. Visual (Guna.UI2 + paleta do mockup)

| Uso                            | Cor        |
|----------------------------------|------------|
| Fundo da área de conteúdo          | `#5C7F72`  |
| Fundo da barra de status           | `#4E6E62`  |
| Barra lateral / botões "pill"      | `#8FCEBB`  |
| Campos de texto / cards            | `#FFFFFF`  |
| Texto sobre fundo escuro           | `#FFFFFF`  |
| Texto sobre fundo claro            | `#1F2937`  |
| Acento (ativo/foco/progresso)      | `#2F7DE8`  |
| Perigo (Cancelar/Limpar)           | `#E15C4F`  |

Constantes centralizadas no topo de `MainForm.Designer.cs`. Fonte usada em
toda a interface: **Consolas** (monoespaçada, combinando com a estética do
mockup). Os ícones/ilustrações ficam em `Assets/` (PNG, gerados como
silhuetas simples: logo com linhas de movimento, relógio/histórico, pasta,
engrenagem, seta de download, nota musical).

### Ícone do aplicativo

`icon.ico` (raiz do projeto) é referenciado via
`<ApplicationIcon>` no `.csproj` (ícone do `.exe`) e carregado em tempo de
execução por `CarregarIconeDaJanela()` (ícone da janela/taskbar/bandeja de
notificações). Para trocar, substitua o arquivo mantendo o nome.

### Instalando o Guna.UI2.WinForms

```bash
cd YtDlpGui
dotnet restore
```

Exige internet na primeira vez (baixa o pacote do NuGet, fixado na versão
`2.0.4.6` no `.csproj`). O `publicar.ps1` já roda `dotnet publish`, que
restaura automaticamente.

## 9. Filtro de conteúdo (opcional — para uso em sala de aula)

Módulo pensado para quem usa o app com alunos e quer restringir quais
sites podem ser baixados, evitando que o "generic extractor" do yt-dlp
(que consegue extrair vídeo embutido em praticamente qualquer página da
internet) seja usado para baixar conteúdo impróprio.

### Por que é allowlist, e não blocklist

O yt-dlp já inclui extractors dedicados para sites adultos, e o
"generic extractor" tenta baixar de qualquer site não catalogado. Tentar
listar sites *proibidos* é uma corrida perdida — sempre vai faltar um.
Por isso o módulo funciona ao contrário: você define quais sites são
**permitidos** (por padrão, só `youtube.com`/`youtu.be`) e tudo mais é
bloqueado.

### Por que é um arquivo separado

`FiltroConteudo.cs` **não é referenciado em nenhum outro lugar do projeto
por padrão** — a única forma de ele fazer alguma coisa é existir um
arquivo `filtro-conteudo.json` na mesma pasta do `.exe`, com
`"habilitado": true`. Sem esse arquivo (o caso de qualquer pessoa que
clonar o repositório no GitHub), `FiltroConteudo.CarregarConfiguracao()`
retorna uma configuração desligada e `PassaPeloFiltroDeConteudoAsync()`
(chamado em `MainForm.cs`, nos dois botões de download) retorna
imediatamente `true` — nenhuma verificação, nenhuma chamada extra ao
yt-dlp, nenhum impacto em quem não configurou nada.

Ou seja: **o filtro só existe pra quem explicitamente criar o arquivo de
configuração.** Isso resolve o problema de "alguém clona o projeto pra
baixar do Pinterest e o programa simplesmente bloqueia sem aviso".

### O que ele verifica

1. **Domínio da URL** (`FiltroConteudo.DominioPermitido`): compara o host
   da URL colada contra `dominiosPermitidos`. Aceita subdomínios
   automaticamente (`youtube.com` na lista libera `www.`, `m.`,
   `music.youtube.com`, etc.).
2. **Restrição de idade do próprio YouTube** (opcional, campo
   `bloquearRestricaoDeIdade`): roda `yt-dlp --dump-json --skip-download`
   antes de baixar de verdade, lê o campo `age_limit` da resposta, e
   bloqueia se vier ≥ 18. Isso adiciona alguns segundos de espera antes do
   download começar — é o custo dessa checagem.

Se qualquer uma das duas checagens bloquear, aparece um diálogo pedindo o
**código mestre** (construído inteiramente em código dentro do próprio
`FiltroConteudo.cs`, sem precisar de arquivo Designer). Digitando
corretamente, aquele download específico é liberado.

### Como ativar (só na SUA cópia, sem afetar o GitHub)

**Passo 1 — Gere o hash do seu código mestre.** O código nunca fica em
texto puro no arquivo de configuração, só o hash SHA-256 dele. No
PowerShell:

```powershell
$codigo = "coloque-seu-codigo-aqui"
$bytes  = [System.Text.Encoding]::UTF8.GetBytes($codigo)
$hash   = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
([System.BitConverter]::ToString($hash) -replace '-', '').ToLower()
```

Isso imprime uma sequência de 64 caracteres (ex.: para o código
`professor123`, o hash é
`bc1f12dae4eded5bc6a976a10aff2b93f36b5b26dcdd22da485b2fecd4b3b119`).
Copie o resultado.

**Passo 2 — Crie o arquivo de configuração.** Copie
`filtro-conteudo.exemplo.json` para `filtro-conteudo.json` (mesma pasta,
mesma pasta onde o `.exe` compilado vai parar) e edite:

```json
{
  "habilitado": true,
  "dominiosPermitidos": [ "youtube.com", "youtu.be" ],
  "bloquearRestricaoDeIdade": true,
  "hashCodigoMestre": "bc1f12dae4eded5bc6a976a10aff2b93f36b5b26dcdd22da485b2fecd4b3b119"
}
```

**Passo 3 — Coloque o arquivo junto do `.exe`.** Não precisa recompilar
nada — é só garantir que `filtro-conteudo.json` esteja na mesma pasta do
`YtDlpGui.exe` (a pasta de saída do build, ex.:
`bin\Debug\net8.0-windows\`, ou a pasta final se você usar o
`publicar.ps1`). Ele é lido toda vez que um download é iniciado, então dá
pra editar/ligar/desligar mesmo com o app já publicado, sem rebuild.

**Se você usa o `publicar.ps1`:** ele não copia `filtro-conteudo.json`
automaticamente (só o `.exemplo.json`, como documentação) — depois que o
script gerar a pasta `Distribuicao/`, copie seu `filtro-conteudo.json`
real pra lá manualmente antes de zipar/distribuir pros alunos. Assim o
próprio script continua 100% genérico e reutilizável por qualquer pessoa
que pegar o projeto no GitHub.

### Limitações (importante ter em mente)

- **Não é segurança real, é uma barreira de uso normal.** É um app
  local — um aluno com conhecimento técnico poderia abrir o `.json` e
  trocar `"habilitado": false`, ou editar o `.cs` e recompilar. Serve para
  evitar uso indevido no dia a dia da sala, não para um cenário
  adversarial sério.
- A checagem de `age_limit` só existe pra URLs que passaram no filtro de
  domínio primeiro (ou seja, hoje só se aplica a URLs do YouTube).
- Vídeos sem o campo `age_limit` na resposta do yt-dlp (extractor não
  fornece, ou vídeo já bloqueado antes de gerar metadados) são tratados
  como sem restrição — não é 100% infalível.

## 10. Distribuição para usuários comuns (`publicar.ps1`)

Continua funcionando como antes: compila em modo self-contained + single
file, baixa `yt-dlp.exe` e `ffmpeg.exe` automaticamente, copia o
`LEIA-ME.txt` e empacota tudo em `YtDlpGui-Portatil.zip`. Rode em um
Windows com .NET 8 SDK e internet:

```powershell
cd YtDlpGui
.\publicar.ps1
```

> **Atenção**: o script atual não copia a pasta `Assets/` nem o
> `icon.ico` manualmente — isso já é feito automaticamente pelo
> `dotnet publish`, pois ambos estão marcados como `CopyToOutputDirectory`
> no `.csproj`. Não é necessário alterar o `publicar.ps1` para isso.

## 11. Possíveis melhorias futuras (fora do escopo atual)

- Notificações estilo "toast" do Windows 11 (via
  `Microsoft.Toolkit.Uwp.Notifications`), em vez de balão clássico.
- Exportar o histórico para CSV/Excel.
- Assinatura digital do executável, para eliminar o aviso do SmartScreen.
- Indicador visual de "download em andamento" nos ícones da barra lateral
  mesmo quando o usuário está em outra página.

## 12. Sugestões extras (para avaliação — nada disso foi implementado)

Ideias que surgiram olhando o código atual, deixadas aqui só para você
decidir se vale a pena entrar num próximo ciclo:

- **Fila de downloads**: hoje só dá pra baixar um vídeo/playlist por vez
  (os botões ficam bloqueados durante o download). Uma fila permitiria
  colar várias URLs e deixar baixando em sequência sem precisar ficar
  vigiando.
- **Detectar link de playlist automaticamente**: em vez do usuário
  precisar lembrar de marcar "Baixar Playlist", o app poderia detectar
  `&list=` na URL colada e sugerir/marcar o checkbox sozinho (com opção de
  desmarcar).
- **Botão "Recomeçar do zero" no Histórico**: re-baixar um item antigo
  clicando nele, sem precisar colar a URL de novo (a URL original já é
  salva em `HistoricoItem.Url`, só falta a ação na tela).
- **Validação de espaço em disco** antes de iniciar um download grande
  (playlists podem ser pesadas).
- **Indicador de velocidade/tamanho** (ex.: "12 MB/s — 340 MB de 1.2 GB"),
  já que o yt-dlp imprime isso na mesma linha do progresso — só falta
  capturar com outra regex e mostrar ao lado da porcentagem.
- **Tema claro/escuro alternável** pelo usuário, em vez de só a paleta
  fixa atual.
- **Verificação automática de atualização do yt-dlp** ao abrir o
  app (silenciosa, uma vez por dia), em vez de depender do botão manual.

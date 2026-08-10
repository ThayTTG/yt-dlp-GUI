# YtDlpGui — Interface gráfica para o yt-dlp

Aplicação desktop em **C# / .NET 8 (WinForms)** que permite baixar vídeos e
áudios do YouTube sem que o aluno precise usar o CMD ou o PowerShell.

Visual baseado em um mockup próprio: barra lateral de navegação, telas
separadas por função, paleta sálvia + menta + azul, sem exibir a saída
bruta do yt-dlp em nenhum momento.

## Estrutura dos arquivos

```
YtDlpGui/
├── YtDlpGui.csproj              # projeto (.NET 8, WinForms, pacote Guna.UI2.WinForms)
├── Program.cs                   # ponto de entrada da aplicação
├── MainForm.cs                  # toda a lógica: navegação, download, notificações
├── MainForm.Designer.cs         # layout visual (barra lateral + 4 páginas + status bar)
├── GerenciadorHistorico.cs      # persistência do histórico em JSON (%AppData%)
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

| Painel                 | Conteúdo                                                                 |
|----------------------- |--------------------------------------------------------------------------|
| `pnlHome`              | Campo de URL + botões "Baixar .MP4" / "Baixar .MP3" + toggle de playlist |
| `pnlHistorico`         | Lista (Tipo/Nome/Data) dos downloads já concluídos                       |
| `pnlPasta`             | Local de salvamento + botão "Alterar Local"                              |
| `pnlConfiguracoes`     | Autenticação por cookies + botão "Atualizar yt-dlp"                      |

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

## 6. Autenticação por cookies

Página "Configurações" com três opções (mutuamente exclusivas via
`Guna2RadioButton`):

- **Não usar cookies** (padrão)
- **Cookies do navegador**: `--cookies-from-browser <navegador>`
- **Arquivo cookies.txt**: `--cookies <caminho>`, selecionado via
  `OpenFileDialog`

Resolve o erro "Sign in to confirm you're not a bot" do YouTube.

## 7. Cancelar download e suporte a playlist

- Botão circular vermelho "X" (`btnCancelar`) mata o processo do yt-dlp
  (e o ffmpeg filho) via `Process.Kill(entireProcessTree: true)`.
- Toggle "Baixar Playlist" na Home controla `--yes-playlist` /
  `--no-playlist` (desmarcado por padrão, mais seguro para uso educacional).

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

## 9. Distribuição para usuários comuns (`publicar.ps1`)

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

## 10. Possíveis melhorias futuras (fora do escopo atual)

- Notificações estilo "toast" do Windows 11 (via
  `Microsoft.Toolkit.Uwp.Notifications`), em vez de balão clássico.
- Exportar o histórico para CSV/Excel.
- Assinatura digital do executável, para eliminar o aviso do SmartScreen.
- Indicador visual de "download em andamento" nos ícones da barra lateral
  mesmo quando o usuário está em outra página.

# YtDlpGui — Interface gráfica para o yt-dlp

Aplicação desktop em **C# / .NET 8 (WinForms)** que permite baixar vídeos e
áudios do YouTube sem que o aluno precise usar o CMD ou o PowerShell.

## Estrutura dos arquivos

```
YtDlpGui/
├── YtDlpGui.csproj          # arquivo de projeto (.NET 8, WinForms)
├── Program.cs                # ponto de entrada da aplicação
├── MainForm.cs                # lógica principal (chamada ao yt-dlp, eventos, validações)
├── MainForm.Designer.cs       # definição visual dos controles (gerado como "Designer")
└── README.md                  # este arquivo
```

## 1. Onde colocar o `yt-dlp.exe` e o `ffmpeg.exe`

**O aplicativo espera encontrar os dois executáveis na MESMA PASTA do
`.exe` da aplicação** (a pasta de saída da compilação, por exemplo:
`bin\Debug\net8.0-windows\` durante o desenvolvimento, ou a pasta onde
você distribuir o programa publicado).

O caminho é resolvido em `MainForm.cs` através de:

```csharp
string pastaApp = AppDomain.CurrentDomain.BaseDirectory;
_ytDlpPath = Path.Combine(pastaApp, "yt-dlp.exe");
_ffmpegPath = Path.Combine(pastaApp, "ffmpeg.exe");
```

### Onde baixar cada executável

- **yt-dlp.exe**: baixe a versão Windows mais recente em
  `https://github.com/yt-dlp/yt-dlp/releases` (arquivo `yt-dlp.exe`).
- **ffmpeg.exe**: baixe um build Windows (essentials/full) em
  `https://www.gyan.dev/ffmpeg/builds/` ou `https://ffmpeg.org/download.html`,
  e copie apenas o `ffmpeg.exe` (dentro da pasta `bin` do pacote baixado)
  para a mesma pasta da aplicação.

> O `ffmpeg.exe` é **obrigatório** para a extração/conversão de MP3 (`-x
> --audio-format mp3`) e é altamente recomendado também para o MP4, pois é
> ele quem mescla as faixas de vídeo e áudio baixadas separadamente pelo
> yt-dlp em um único arquivo.

### Automatizando a cópia na compilação (opcional)

Se preferir, crie uma pasta `tools/` ao lado do `.csproj`, coloque os dois
executáveis lá dentro, e descomente o bloco `<ItemGroup>` já preparado no
`YtDlpGui.csproj`. Assim, toda vez que você compilar o projeto, os arquivos
serão copiados automaticamente para a pasta de saída.

## 2. Como compilar e executar

Com o [.NET 8 SDK](https://dotnet.microsoft.com/download) instalado:

```bash
cd YtDlpGui
dotnet build
dotnet run
```

Ou abra a pasta no Visual Studio 2022 (17.8+) e pressione **F5**.

## 3. Como o programa evita abrir a janela do CMD

O `MainForm.cs` configura o `ProcessStartInfo` da seguinte forma:

```csharp
var psi = new ProcessStartInfo
{
    FileName = _ytDlpPath,
    UseShellExecute = false,       // necessário para redirecionar a saída
    CreateNoWindow = true,         // impede que qualquer console apareça
    RedirectStandardOutput = true, // captura o progresso
    RedirectStandardError = true,  // captura mensagens de erro/aviso
};
```

A saída do processo é lida de forma assíncrona via
`OutputDataReceived` / `ErrorDataReceived` e repassada para o
`RichTextBox` de status usando `Invoke`/`BeginInvoke`, já que esses
eventos disparam em uma thread diferente da interface gráfica.

## 4. Validações e tratamento de erros incluídos

- URL vazia ou em formato inválido → aviso ao usuário, sem chamar o processo.
- Pasta de destino inexistente/inválida → aviso ao usuário.
- `yt-dlp.exe` ausente → mensagem de erro explicando onde colocá-lo.
- `ffmpeg.exe` ausente → aviso no log (o download pode falhar dependendo do
  formato escolhido pelo YouTube para aquele vídeo).
- Falha ao iniciar o processo (ex.: antivírus bloqueando) → mensagem de erro
  capturada e exibida no log, sem travar a aplicação.
- Código de saída (`ExitCode`) diferente de 0 → aviso de que algo pode ter
  dado errado, mantendo o log visível para o aluno investigar.
- Fechamento da janela durante um download em andamento → o processo filho
  do yt-dlp é encerrado junto (evita processos "fantasma" em segundo plano).

## 5. Cancelar download e suporte a playlist

- **Botão "Cancelar"**: fica desabilitado enquanto não há download em
  andamento. Ao clicar, chama `_processoAtual.Kill(entireProcessTree: true)`,
  encerrando o `yt-dlp.exe` e qualquer `ffmpeg.exe` filho que ele tenha
  iniciado. O log exibe "Download cancelado pelo usuário." em vez de uma
  mensagem de erro.
- **Checkbox "Baixar playlist inteira"**: desmarcado por padrão (mais seguro
  para uso educacional — evita que o aluno baixe uma playlist inteira sem
  querer ao colar um link que contém `&list=`). Quando desmarcado, o
  programa passa `--no-playlist` ao yt-dlp; quando marcado, passa
  `--yes-playlist`, baixando todos os vídeos da playlist referenciada pelo
  link.
- O checkbox e o botão "Selecionar Pasta"/campo de URL ficam desabilitados
  durante um download em andamento, e o botão "Cancelar" é o único
  controle de ação habilitado nesse momento.

## 6. Possíveis melhorias futuras (fora do escopo inicial)

- Histórico de downloads realizados.
- Verificação/atualização automática do `yt-dlp.exe` (`yt-dlp -U`).

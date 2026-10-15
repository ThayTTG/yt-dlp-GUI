<#
    publicar.ps1
    -------------------------------------------------------------------------
    Gera um pacote PRONTO PARA DISTRIBUIR a qualquer usuário comum, sem que
    ele precise instalar o .NET, o yt-dlp ou o ffmpeg manualmente.

    QUEM DEVE RODAR ESTE SCRIPT: você (o desenvolvedor/professor), apenas UMA
    vez a cada nova versão que quiser distribuir. O usuário final NUNCA
    precisa rodar este script — ele só recebe o .zip gerado no final.

    REQUISITOS PARA RODAR ESTE SCRIPT:
      - Windows 10/11
      - .NET 8 SDK instalado (https://dotnet.microsoft.com/download)
      - PowerShell 5.1+ (já vem com o Windows)
      - Conexão com a internet (para baixar yt-dlp.exe e ffmpeg.exe)

    O QUE ELE FAZ:
      1. Compila a aplicação em modo "self-contained" + "single file": gera
         um único YtDlpGui.exe que já inclui o runtime do .NET embutido.
         Ou seja, o usuário final NÃO precisa instalar o .NET para usar o
         programa.
      2. Baixa automaticamente o yt-dlp.exe mais recente do GitHub oficial.
      3. Baixa automaticamente um build oficial do ffmpeg (gyan.dev) e extrai
         apenas o ffmpeg.exe.
      4. Copia o arquivo LEIA-ME.txt (instruções em português simples).
      5. Junta tudo em uma pasta "Distribuicao" e compacta em
         "YtDlpGui-Portatil.zip", pronto para enviar por e-mail, Google
         Drive, pendrive, etc.

    COMO USAR:
      1. Abra o PowerShell nesta pasta (a mesma do YtDlpGui.csproj).
      2. Rode:  .\publicar.ps1
      3. Ao final, envie o arquivo YtDlpGui-Portatil.zip para os alunos.
-------------------------------------------------------------------------- #>

$ErrorActionPreference = "Stop"

$pastaProjeto = $PSScriptRoot
$pastaPublish = Join-Path $pastaProjeto "bin\publish"
$pastaDist    = Join-Path $pastaProjeto "Distribuicao"
$arquivoZip   = Join-Path $pastaProjeto "YtDlpGui-Portatil.zip"

function Escrever-Etapa($texto) {
    Write-Host ""
    Write-Host "==> $texto" -ForegroundColor Cyan
}

# --- 1. Limpeza de execuções anteriores -----------------------------------
Escrever-Etapa "Limpando pastas de builds anteriores..."
Remove-Item $pastaPublish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $pastaDist    -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $arquivoZip   -Force  -ErrorAction SilentlyContinue

# --- 2. Compilação self-contained / single-file ----------------------------
Escrever-Etapa "Compilando a aplicação (self-contained, single file, win-x64)..."
dotnet publish (Join-Path $pastaProjeto "YtDlpGui.csproj") `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $pastaPublish

if (-not (Test-Path (Join-Path $pastaPublish "YtDlpGui.exe"))) {
    throw "A compilação não gerou o YtDlpGui.exe esperado. Verifique os erros acima."
}

New-Item -ItemType Directory -Path $pastaDist -Force | Out-Null

Escrever-Etapa "Copiando os arquivos publicados (exe, icon.ico, Assets)..."
Get-ChildItem -Path $pastaPublish -Exclude "*.pdb" | Copy-Item -Destination $pastaDist -Recurse -Force

if (-not (Test-Path (Join-Path $pastaDist "YtDlpGui.exe"))) {
    throw "Falha ao copiar o YtDlpGui.exe para a pasta de distribuição."
}
# Garantia extra: o ícone da janela precisa estar ao lado do .exe.
if (-not (Test-Path (Join-Path $pastaDist "icon.ico"))) {
    Copy-Item (Join-Path $pastaProjeto "icon.ico") $pastaDist -Force
}

if (-not (Test-Path (Join-Path $pastaDist "Assets"))) {
    Write-Host "Aviso: a pasta Assets não foi encontrada na publicação — os ícones das telas podem não aparecer." -ForegroundColor Yellow
}

# --- 3. Download do yt-dlp.exe ---------------------------------------------
Escrever-Etapa "Baixando o yt-dlp.exe mais recente..."
try {
    Invoke-WebRequest -Uri "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe" `
        -OutFile (Join-Path $pastaDist "yt-dlp.exe")
}
catch {
    Write-Host "Não foi possível baixar o yt-dlp.exe automaticamente ($($_.Exception.Message))." -ForegroundColor Yellow
    Write-Host "Baixe manualmente em https://github.com/yt-dlp/yt-dlp/releases e coloque em: $pastaDist" -ForegroundColor Yellow
}

# --- 4. Download do ffmpeg.exe ----------------------------------------------
Escrever-Etapa "Baixando o ffmpeg (build essentials, gyan.dev)..."
try {
    $ffmpegZip = Join-Path $env:TEMP "ffmpeg-download-$(Get-Random).zip"
    $ffmpegExtract = Join-Path $env:TEMP "ffmpeg-extract-$(Get-Random)"

    Invoke-WebRequest -Uri "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip" `
        -OutFile $ffmpegZip

    Expand-Archive -Path $ffmpegZip -DestinationPath $ffmpegExtract -Force

    $ffmpegExe = Get-ChildItem -Path $ffmpegExtract -Filter "ffmpeg.exe" -Recurse | Select-Object -First 1
    if ($null -eq $ffmpegExe) {
        throw "ffmpeg.exe não encontrado dentro do pacote baixado."
    }

    Copy-Item $ffmpegExe.FullName (Join-Path $pastaDist "ffmpeg.exe") -Force

    Remove-Item $ffmpegZip -Force -ErrorAction SilentlyContinue
    Remove-Item $ffmpegExtract -Recurse -Force -ErrorAction SilentlyContinue
}
catch {
    Write-Host "Não foi possível baixar/extrair o ffmpeg automaticamente ($($_.Exception.Message))." -ForegroundColor Yellow
    Write-Host "Baixe manualmente em https://www.gyan.dev/ffmpeg/builds/ e coloque o ffmpeg.exe em: $pastaDist" -ForegroundColor Yellow
}

# --- 5. Instruções para o usuário final ------------------------------------
Escrever-Etapa "Copiando instruções para o usuário final (LEIA-ME.txt)..."
$leiaMe = Join-Path $pastaProjeto "LEIA-ME.txt"
if (Test-Path $leiaMe) {
    Copy-Item $leiaMe $pastaDist -Force
}
else {
    Write-Host "Arquivo LEIA-ME.txt não encontrado ao lado do script; pulei essa etapa." -ForegroundColor Yellow
}

# --- 6. Compactação final ---------------------------------------------------
Escrever-Etapa "Compactando pacote final..."
Compress-Archive -Path (Join-Path $pastaDist "*") -DestinationPath $arquivoZip -Force

Write-Host ""
Write-Host "Pronto! Pacote gerado em:" -ForegroundColor Green
Write-Host "  $arquivoZip" -ForegroundColor Green
Write-Host ""
Write-Host "Basta enviar esse .zip para qualquer usuário. Ele só precisa extrair" -ForegroundColor Green
Write-Host "e dar dois cliques em YtDlpGui.exe — nada mais precisa ser instalado." -ForegroundColor Green

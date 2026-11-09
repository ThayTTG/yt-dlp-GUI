# Histórico de versões

Tudo o que muda no YT-DLP GUI, versão por versão. O texto de cada versão vira as notas do lançamento no GitHub e aparece em **Novidades**, no NokyLabs Launcher e no site.

**Como usar:** anote as mudanças em `## [Não lançado]` enquanto trabalha. Na hora de lançar, é só aumentar o `<Version>` no `.csproj` e enviar para o branch `release`: o lançamento usa essa seção e ela vira a seção da versão sozinha.

## [Não lançado]

## [3.0.0] - 2026-11-09
- Visual novo, no padrão de todos os apps da NokyLabs (o mesmo do launcher e do ValReport), agora na cor menta. Tudo continua funcionando do mesmo jeito.
- Botões em pílula, animações suaves ao trocar de tela e janelas de aviso no mesmo visual do app.
- As notificações de download concluído aparecem no formato novo do Windows 10/11.
- No canto da barra lateral: "Idealizado e aprovado por Thayna Dias" e "Desenvolvido com IA (Claude, da Anthropic)".
- O ícone da janela volta a aparecer na versão baixada.
- Corrige o erro "Requested format is not available" no YouTube: o app agora usa o Deno, que resolve os desafios do YouTube.
- O FFmpeg e o Deno são baixados sozinhos (dos sites oficiais, com conferência de integridade) se estiverem faltando, e o botão de atualizar confere os dois junto com o yt-dlp.
- Nova tela **Sobre**, com os créditos de quem fez o app e das ferramentas livres que ele usa (yt-dlp, FFmpeg, Deno e outras).

## [2.0.0] - 2026-10-15
- Visual novo, inspirado no NokyLabs Launcher.
- Reparo automático e logs, para facilitar a solução de problemas.
- Já vem com o yt-dlp e o ffmpeg, sem precisar instalar o .NET.

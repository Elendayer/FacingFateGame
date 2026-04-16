@"
# Claude Code – Project Rules

## Git Rules
- NEVER run ``git push`` under any circumstances.
- NEVER create branches without explicit user approval.
- NEVER run ``git commit`` without showing the diff first and asking for confirmation.
- Only stage/commit when explicitly instructed to do so.
- Always show ``git status`` before any git operation.

## General
- This is a Unity 6 project using URP.
- Primary language: English (code, naming, comments).
- Do not make improvement suggestions unless explicitly asked.
"@ | Set-Content -Path "CLAUDE.md" -Encoding UTF8
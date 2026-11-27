## Context

O ambiente local (DT-34) já provê `docker-compose.yml` com `postgres`, `mailpit`, `seq`, `api` e `web`. Para depurar, a API e a SPA rodam fora do Docker: a API em `http://localhost:5071` (`launchSettings.json`) e o Angular em `http://localhost:4200`, cujo `proxy.conf.json` já encaminha `/api` e `/health` a `localhost:5071`. A API exige `ConnectionStrings__Clinica` (papel `clinica_api`) e nunca aplica migrations (DT-12); o semeado do administrador lê `ADMIN_EMAIL`/`ADMIN_SENHA_INICIAL`.

## Goals / Non-Goals

**Goals:**
- F5 sobre o composto `API + Angular` sobe a pilha e permite depurar a API com breakpoints.
- Nenhum segredo versionado; tudo vem do `.env` local.

**Non-Goals:**
- Depurar o Angular com breakpoints além do que o navegador do VS Code oferece por padrão.
- Subir `mailpit`/`seq` (opcionais em desenvolvimento) ou aplicar migrations automaticamente.

## Decisions

- **Depurar a DLL, não `dotnet run`:** `program` aponta para `bin/Debug/net10.0/Clinica.Api.dll`, precedido da tarefa `api: preparar` (Postgres + build). Alternativa descartada: `dotnet run` como programa, porque o depurador anexaria ao processo da CLI e os breakpoints não seriam atingidos.
- **Porta fixa por `ASPNETCORE_URLS`:** `http://localhost:5071`, igual ao alvo do proxy do Angular; `--no-launch-profile` não é necessário porque a DLL é executada diretamente.
- **Segredos por `envFile`:** `"envFile": "${workspaceFolder}/.env"`. O `envFile` lê `CHAVE=valor` literalmente, então o `ConnectionStrings__Clinica` (com `;`) funciona; carregá-lo por `source` no shell não funciona. `ConnectionStrings__Clinica` é acrescentada ao `.env` (e ao `.env.example`) com host `localhost` e usuário `clinica_api`.
- **Postgres via tarefa:** `docker compose up -d --wait postgres` aguarda o healthcheck antes de a API iniciar; evita subir serviços desnecessários.
- **Angular por tarefa em segundo plano:** `npm start` com `problemMatcher` em segundo plano; o composto abre o navegador (Edge) em `http://localhost:4200` após a compilação. Chrome é alternativa trocando o `type`.
- **Versionar `.vscode/`:** só `launch.json` e `tasks.json` (sem `settings.json` pessoal), para que todos executem da mesma forma.

## Risks / Trade-offs

- [Selecionar só "API (.NET)" no F5 não sobe o Angular] → documentar no README que deve ser escolhido o composto `API + Angular`.
- [`dotnet build` da tarefa e `dotnet watch` concorrentes bloqueiam a DLL] → não usar `watch`; reiniciar pelo depurador.
- [`.env` com `ConnectionStrings__Clinica` divergente do compose] → senha única `CLINICA_APP_SENHA`; `.env.example` documenta o formato.
- [Sem hot reload da API] → aceito; o Angular mantém seu próprio live reload.

# Tasks

> Origem: DT-34 (TDD-001). Sem RF/RN associados.

## 1. Configuração do VS Code

- [x] 1.1 Criar `.vscode/tasks.json` com `banco: subir postgres`, `api: compilar`, `api: preparar` e `frontend: ng serve`; verificar que `docker compose up -d --wait postgres` e `dotnet build` concluem sem erro
- [x] 1.2 Criar `.vscode/launch.json` com `API (.NET)` (DLL, `ASPNETCORE_URLS=http://localhost:5071`, `envFile` `.env`), `Angular (Edge)` e o composto `API + Angular`; verificar que o F5 do composto abre a SPA e o Swagger responde na 5071
- [x] 1.3 Verificar o login do administrador semeado pela SPA e pelo Swagger com a pilha iniciada pelo composto

## 2. Documentação e segredos

- [x] 2.1 Acrescentar `ConnectionStrings__Clinica` (papel `clinica_api`, host `localhost`) ao `.env.example`; verificar que o `.env` real continua ignorado (`git check-ignore .env`)
- [x] 2.2 Documentar no `README.md` raiz e no `backend/README.md` a execução pelo VS Code (escolher o composto `API + Angular`, Docker ligado, `.env` preenchido); verificar a coerência com os nomes das configurações em `launch.json`
- [x] 2.3 Verificar que `.vscode/*.json` não contém senhas, chaves nem strings de conexão com credenciais (`git grep -n -i "password\|senha" .vscode` sem ocorrências)

## Workflow follow-up

- Abrir PR com `.github/pull_request_template.md` a partir de `chore/configuracao-vscode-execucao-local`, após as tarefas marcadas.
- Sincronizar (`openspec-sync-specs`) e arquivar (`openspec-archive-change`) a change após o merge.

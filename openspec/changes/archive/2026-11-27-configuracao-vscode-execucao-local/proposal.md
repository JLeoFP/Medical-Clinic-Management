## Why

O ambiente local (DT-34) hoje exige subir o Postgres, a API e o Angular manualmente em três terminais, e a API só conecta se o `ConnectionStrings__Clinica` estiver exportado no shell. Isso gera erros de primeira execução (API sem string de conexão, Angular sem API) e atrapalha a depuração dentro do VS Code.

## What Changes

- Adicionar `.vscode/tasks.json` com tarefas para subir o Postgres (`docker compose up -d --wait postgres`), compilar a API e servir o Angular (`npm start`).
- Adicionar `.vscode/launch.json` com as configurações `API (.NET)` (depuração do `Clinica.Api.dll` em `http://localhost:5071`) e `Angular (Edge)` (`http://localhost:4200`), e o composto `API + Angular` acionado por F5.
- A API carrega as variáveis do `.env` local via `envFile`; o `.env.example` e o `backend/README.md` passam a documentar `ConnectionStrings__Clinica` para execução fora do Docker.
- Não alterar código de produto, `docker-compose.yml` nem pipelines.

Fora desta change: depuração do Angular em outros navegadores além de Edge/Chrome, perfis de produção e qualquer ajuste em CI.

## Capabilities

### New Capabilities
- `ambiente-desenvolvimento-local`: execução e depuração da pilha local (Postgres, API e SPA) a partir do VS Code com um único comando.

### Modified Capabilities
<!-- Nenhuma: `ambiente-integracao-continua` (change `fundacao-plataforma`) cobre o docker-compose e o CI, não a execução pelo editor. -->

## Impact

- Arquivos novos: `.vscode/tasks.json`, `.vscode/launch.json`.
- Documentação: `.env.example`, `backend/README.md`, `README.md` raiz (seção de execução local).
- Sem impacto em API, banco, dependências ou segurança (o `.env` continua fora do git).
- Origem: DT-34 (Docker e docker-compose para ambiente local), TDD-001. Nenhuma decisão de produto nova (sem RF/RN associados).

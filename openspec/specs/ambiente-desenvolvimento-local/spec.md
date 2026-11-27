# ambiente-desenvolvimento-local Specification

## Purpose
Definir a execução e a depuração da pilha local (Postgres, API e SPA) a partir do VS Code com um único comando, sem versionar segredos.

## Requirements

### Requirement: Execução da pilha local pelo VS Code
O repositório SHALL fornecer configurações de execução do VS Code (`.vscode/launch.json` e `.vscode/tasks.json`) que subam, com um único comando (F5 no composto `API + Angular`), o Postgres local, a API em `http://localhost:5071` e a SPA em `http://localhost:4200`, com o proxy da SPA encaminhando `/api` e `/health` à API. Origem: DT-34.

#### Scenario: Subida pelo composto
- **WHEN** o desenvolvedor, com o Docker em execução e o `.env` local preenchido, executa o composto `API + Angular`
- **THEN** o Postgres fica saudável, a API responde 200 em `/health` e a SPA é aberta no navegador em `http://localhost:4200`

#### Scenario: Login do administrador inicial
- **WHEN** o desenvolvedor entra com o administrador semeado (`ADMIN_EMAIL`/`ADMIN_SENHA_INICIAL` do `.env`) na SPA ou no Swagger UI
- **THEN** a autenticação é concluída com sucesso

### Requirement: Segredos fora do versionamento
As configurações do VS Code SHALL ler a conexão e demais segredos do `.env` local (ignorado pelo git), sem conter senhas, chaves ou strings de conexão com credenciais. Origem: DT-34.

#### Scenario: Verificação de segredos
- **WHEN** `.vscode/launch.json` e `.vscode/tasks.json` são inspecionados
- **THEN** não contêm valores de senha nem de chaves de API; a conexão vem do `envFile` apontando para `.env`

### Requirement: Papel restrito da aplicação
A API executada pelo VS Code SHALL usar o login restrito `clinica_api` (sem `UPDATE`/`DELETE` em `auditoria`), e as migrations SHALL continuar sendo aplicadas manualmente com o papel dono do banco. Origem: DT-34, DT-12.

#### Scenario: API não migra o banco
- **WHEN** a API é iniciada pelo VS Code
- **THEN** nenhuma migration é aplicada automaticamente e a conexão usa o usuário `clinica_api`

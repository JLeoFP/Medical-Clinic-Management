# Backend — API da Clínica

API REST em **.NET 10 / C# 14** (Minimal APIs, Clean Architecture, CQRS com MediatR). Decisões em
[`docs/tdd/TDD-001`](../docs/tdd/TDD-001-plataforma-gestao-clinica.md); a change em
[`openspec/changes/fundacao-plataforma`](../openspec/changes/fundacao-plataforma).

## Estrutura

```text
backend/
├── Clinica.slnx
├── src/
│   ├── Clinica.Domain/          # sem referências a outros projetos
│   ├── Clinica.Application/     # casos de uso, pipeline MediatR (referencia só Domain)
│   ├── Clinica.Infrastructure/  # EF Core, Identity, sessões, auditoria, jobs
│   └── Clinica.Api/             # raiz de composição (Program.cs, endpoints)
└── tests/
    ├── Clinica.Domain.Tests/
    ├── Clinica.Application.Tests/
    └── Clinica.Api.IntegrationTests/   # WebApplicationFactory + Testcontainers (PostgreSQL 17)
```

## Pré-requisitos

- .NET SDK 10 (`dotnet --version` ≥ 10.0.100)
- Docker em execução (somente para os testes de integração e para o `docker-compose.yml`)
- Ferramentas locais: `dotnet tool restore` (instala o `dotnet-ef`)

## Compilar e testar

```bash
cd backend
dotnet build Clinica.slnx                 # warnings são erros
dotnet format Clinica.slnx --verify-no-changes
dotnet test --solution Clinica.slnx       # todos os testes (integração exige Docker)

# apenas os testes unitários (não precisam de Docker)
dotnet test --project tests/Clinica.Domain.Tests
dotnet test --project tests/Clinica.Application.Tests
```

O `global.json` seleciona o runner Microsoft.Testing.Platform (MSTest 4).

## Executar a API

Variáveis de ambiente (nunca versione valores reais; use o `.env` local, ignorado pelo git):

| Variável | Obrigatória | Descrição |
|---|---|---|
| `ConnectionStrings__Clinica` | sim | Conexão PostgreSQL com o **papel restrito** da aplicação (sem `UPDATE`/`DELETE` em `auditoria`). |
| `ADMIN_EMAIL` | não | E-mail do administrador inicial. |
| `ADMIN_SENHA_INICIAL` | não | Senha inicial (mín. 8 caracteres, com letra e número); a troca é obrigatória no primeiro acesso. Sem as duas variáveis, a API só registra um aviso. |
| `MediatR__LicenseKey` | não | Chave da licença do MediatR 14 (DT-20). Sem ela a biblioteca registra um aviso (uso de desenvolvimento/teste). |
| `Seq__ServerUrl` | não | Destino dos logs em `Development` (padrão `http://localhost:5341`). |
| `ASPNETCORE_ENVIRONMENT` | não | `Development` habilita OpenAPI (`/openapi/v1.json`) e Swagger UI (`/swagger`). |

```bash
cd backend
dotnet run --project src/Clinica.Api
```

No VS Code, o composto **API + Angular** (F5) executa a API com essas variáveis lidas do `.env` da raiz (`envFile`);
nesse arquivo o valor de `ConnectionStrings__Clinica` fica sem aspas.

## Migrations

A API **nunca** aplica migrations ao iniciar. Aplique-as com o papel dono do banco:

```bash
cd backend
export ConnectionStrings__Clinica="Host=localhost;Database=clinica;Username=postgres;Password=<senha-do-dono>"
dotnet ef database update --project src/Clinica.Infrastructure
# nova migration
dotnet ef migrations add NomeEmPascalCase --project src/Clinica.Infrastructure --output-dir Persistence/Migrations
```

A migration inicial cria o papel de grupo `clinica_app` (sem `UPDATE`/`DELETE` em `auditoria`). O login de conexão da API
deve pertencer a esse papel — o `docker-compose.yml` local já faz isso.

## Convenções

- API em `/api/v1`; erros em ProblemDetails com `code` e `traceId`; JSON em camelCase.
- Sessão por cookie `__Host-clinica-sessao` (armazenada em `identidade.sessoes`) e anti-CSRF com `X-XSRF-TOKEN`
  em todo `POST`/`PUT`/`PATCH`/`DELETE`.
- Instantes em UTC; o fuso da clínica (`America/Sao_Paulo`) é aplicado por `RelogioClinica`.
- Logs JSON sem corpo de requisição nem dados pessoais.

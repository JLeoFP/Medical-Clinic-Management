## Why

O PRD-001 e o TDD-001 estão aprovados, mas não existe código. A Fase 0 (TDD §22.2, tag `v0.1.0`) entrega o esqueleto executável de ponta a ponta — API, banco, autenticação por sessão, SPA, ambiente local e CI — para que todas as fases seguintes (cadastros, agenda, prontuário, portal, cobrança) sejam construídas sobre qualidade automatizada desde o primeiro commit.

## What Changes

- Criar o monorepo `backend/` + `frontend/` com `Clinica.slnx` (4 projetos + 3 de teste), `Directory.Build.props` e `Directory.Packages.props` (DT-01, DT-05, DT-06).
- API base: pipeline MediatR (`ValidationBehavior`, `LoggingBehavior`, `TransactionBehavior`), ProblemDetails com `code`, Serilog, OpenAPI + Swagger UI em desenvolvimento, `/health` e `/health/ready` (API-46) (DT-02, DT-25, DT-27, DT-28, DT-39, DT-42).
- Persistência: `ClinicaDbContext`, primeira migration (schemas `clinica` e `identidade`; tabelas de Identity, `sessoes`, `chaves_protecao_dados`, `parametros`, `auditoria`), seeds de perfis, parâmetros e administrador inicial (DT-11 a DT-13, TDD §9.3).
- Autenticação: Identity com cookie `__Host-clinica-sessao`, `SessaoTicketStore`, anti-CSRF, API-01 a API-03, política de senha e bloqueio, JOB-02 de limpeza de sessões (DT-18, DT-23, DT-45).
- Auditoria: `AuditoriaInterceptor`, `AuditoriaLeituraBehavior`, registro de login e falha de login (DT-19); `RelogioClinica` sobre `TimeProvider` (DT-26).
- SPA Angular 22 zoneless com CDK, Tailwind CSS 4, Lucide, componentes base de `shared/ui`, validadores de Signal Forms, locale pt-BR, `AuthStore` restaurado por API-02, `sessaoInterceptor`, XSRF do `HttpClient`, layout `/app` e tela `/entrar` (DT-07 a DT-10, DT-46, DT-47).
- Ambiente: `docker-compose.yml` local (postgres, mailpit, seq, api, web) e `ci.yml` com build, lint, testes e gate de cobertura (DT-30 a DT-32, DT-34, DT-36).

Fora desta change: endpoints de domínio (usuários, pacientes etc.), portal do paciente (API-04 a API-06, API-47, API-48 e `SmtpEmailSender`), `deploy.yml`, imagens de produção e qualquer publicação na AWS (EC2, CloudFront, SSM, Budgets, GHCR), que ficam para a Fase 5 (`implantacao-aws`); a Fase 0 roda apenas em ambiente local e CI.

## Capabilities

### New Capabilities
- `autenticacao-sessao`: login interno, sessão no servidor com cookie, logout, restauração de sessão, anti-CSRF, bloqueio por falhas e revogação.
- `plataforma-api`: convenções transversais da API (versionamento, ProblemDetails, validação, health checks, OpenAPI, logs sem dados pessoais).
- `dados-iniciais`: esquema inicial do banco, perfis, parâmetros padrão e administrador inicial idempotente.
- `auditoria`: trilha de auditoria somente inserção (login, falha de login, alterações e leituras marcadas).
- `spa-base`: shell da SPA, restauração de sessão ao recarregar, tela `/entrar`, tratamento de 401 e erros, locale pt-BR.
- `ambiente-integracao-continua`: ambiente local via docker-compose e pipeline de CI com gates de qualidade.

### Modified Capabilities
<!-- Nenhuma: `openspec/specs/` está vazio. -->

## Impact

- Código novo em `backend/`, `frontend/`, `docker-compose.yml`, `.github/workflows/ci.yml`; nenhum código existente é alterado.
- Dependências: .NET 10, MediatR 14 (licença comercial, DT-02), Angular 22, Tailwind 4, PostgreSQL 17, Testcontainers (Docker necessário para os testes de integração).
- Rastreabilidade: RF-36 (parcial), RNF-02, RNF-03 (base), RNF-09; DT-01 a DT-13, DT-18 a DT-20, DT-22 a DT-28, DT-30 a DT-32, DT-34, DT-36, DT-37, DT-39, DT-42, DT-45 a DT-47.

# Tasks

> Origem: TDD-001 §22.2 (Fase 0, `v0.1.0`). Cada grupo inclui seus próprios testes e documentação.

## 1. Estrutura da solução backend

- [x] 1.1 Criar `backend/Clinica.slnx`, `Directory.Build.props` (net10.0, Nullable, TreatWarningsAsErrors) e `Directory.Packages.props`; verificar com `dotnet build backend/Clinica.slnx` sem warnings
- [x] 1.2 Criar `Clinica.Domain`, `Clinica.Application`, `Clinica.Infrastructure`, `Clinica.Api` e os 3 projetos de teste com as referências da regra de dependência (TDD §7.2); verificar que `Domain` não referencia nenhum projeto (teste de arquitetura ou checagem do `.csproj`)
- [x] 1.3 Adicionar `Domain/Shared` (`Entity`, `AggregateRoot`, `IDomainEvent`, `DomainException`) com testes unitários em `Clinica.Domain.Tests`; verificar `dotnet test` verde
- [x] 1.4 Adicionar `.editorconfig`, analyzers e `dotnet format`; verificar `dotnet format --verify-no-changes`

## 2. API base

- [ ] 2.1 Configurar `Program.cs` com `MapGroup("/api/v1")`, JSON camelCase, `TimeProvider` e `RelogioClinica` (`America/Sao_Paulo`); testar conversão UTC → local com `FakeTimeProvider`
- [ ] 2.2 Implementar pipeline MediatR (`LoggingBehavior`, `ValidationBehavior` com FluentValidation, `TransactionBehavior`, `AuditoriaLeituraBehavior`) com testes unitários dos behaviors (spec `plataforma-api`)
- [ ] 2.3 Implementar ProblemDetails com `code`, `errors` por campo e tratamento de exceção não tratada (500 com `traceId`); teste de integração para 400, 404, 422 e 500
- [ ] 2.4 Configurar Serilog (JSON, enriquecimento, sem corpo/dados pessoais) e sink Seq em `Development`; teste que o log de login não contém e-mail nem senha
- [ ] 2.5 Configurar OpenAPI + Swagger UI somente em `Development`; teste de integração por ambiente
- [ ] 2.6 Implementar `/health` e `/health/ready` (API-46); teste com Testcontainers, incluindo banco indisponível → 503
- [ ] 2.7 Documentar com `<summary>` os tipos públicos criados e escrever `backend/README.md` (como compilar, testar e variáveis de ambiente); verificar que os comandos documentados rodam

## 3. Persistência e dados iniciais

- [ ] 3.1 Criar `ClinicaDbContext` (schemas `clinica`/`identidade`, snake_case, UUID v7, `timestamptz`) e a infraestrutura de testes com Testcontainers PostgreSQL 17
- [ ] 3.2 Criar a primeira migration (Identity renomeada, `sessoes`, `chaves_protecao_dados`, `parametros`, `auditoria`, índices do TDD §9.2) e o papel da aplicação sem `UPDATE`/`DELETE` em `auditoria`; teste de integração aplica a migration em banco vazio e confirma que `UPDATE` em `auditoria` é rejeitado
- [ ] 3.3 Semear perfis e parâmetros (`HasData`) e implementar o seeder idempotente do administrador (`ADMIN_EMAIL`, `ADMIN_SENHA_INICIAL`); testes: primeira execução cria, reinício não duplica, sem variáveis só registra aviso
- [ ] 3.4 Garantir que a API não aplica migrations ao iniciar; teste que a API sobe contra o esquema já migrado

## 4. Autenticação e sessão

- [ ] 4.1 Configurar Identity (política de senha e bloqueio 5/15 min, DT-18), `ClinicaClaimsPrincipalFactory` e Data Protection em `chaves_protecao_dados`; testes de senha fraca e de bloqueio
- [ ] 4.2 Implementar `SessaoTicketStore` (criação, leitura, renovação após metade do prazo, limite absoluto de 12 h, revogação) e os eventos 401/403 do cookie `__Host-clinica-sessao`; testes com `FakeTimeProvider` para inatividade e limite absoluto
- [ ] 4.3 Implementar `ValidacaoAntiforgeryFilter` e o cookie `__Host-XSRF-TOKEN`; teste: escrita sem `X-XSRF-TOKEN` → 400 `seguranca.antiforgery_invalido`
- [ ] 4.4 Implementar API-01, API-02 e API-03 (commands/queries, validadores, endpoints); testes de integração: login válido/ inválido/ usuário desativado, sessão 200/204, logout revoga e a requisição seguinte recebe 401
- [ ] 4.5 Revogar sessões ao desativar usuário e ao trocar senha (serviço de domínio de acesso, sem endpoint público ainda); teste de revogação imediata
- [ ] 4.6 Implementar JOB-02 no `BackgroundService` (limpeza de sessões > 30 dias, execução diária e na inicialização); teste de idempotência com `FakeTimeProvider`

## 5. Auditoria

- [ ] 5.1 Implementar `AuditoriaInterceptor` (criação/alteração com colunas alteradas em JSONB) registrado para `parametros` nesta fase; teste de integração com alteração de parâmetro
- [ ] 5.2 Registrar `Login` e `LoginFalho` (usuário, perfil, IP, `correlation_id`) nos handlers de login; teste: login do admin semeado gera linha `Login`
- [ ] 5.3 Cobrir `AuditoriaLeituraBehavior` com uma query de teste marcada com `[AuditarLeitura]` e verificar a linha `Leitura`

## 6. Frontend base

- [ ] 6.1 Criar o app Angular 22 zoneless em `frontend/` com Tailwind 4 (tokens em `@theme`), CDK, `@lucide/angular`, ESLint + Prettier e Vitest; verificar `ng build`, `ng lint` e `ng test`
- [ ] 6.2 Configurar locale pt-BR (`LOCALE_ID`, `DEFAULT_CURRENCY_CODE`), pipes `cpf`, `telefone`, `idade` e validadores de Signal Forms (CPF, telefone, data não futura); testes Vitest incluindo `18/11/2026` e `R$ 1.234,50`
- [ ] 6.3 Implementar `AuthStore`, `provideAppInitializer` chamando API-02, `sessaoInterceptor`, `errorInterceptor`, `correlationInterceptor` e `withXsrfConfiguration`; testes Vitest dos interceptors (401, ProblemDetails, cabeçalhos) e do `AuthStore`
- [ ] 6.4 Implementar `shared/ui` base (botão, campo, avisos com `LiveAnnouncer`, diálogo, tabela, paginação) com testes de acessibilidade básica (rótulos, foco)
- [ ] 6.5 Implementar rotas `/entrar` e `/app` (lazy, `canMatch`, layout, `/app/inicio` vazio) com `returnUrl`; testes Vitest de guard e do formulário de login (erro 401 mantém o e-mail)
- [ ] 6.6 Configurar proxy do `ng serve` para a API (mesma origem) e escrever `frontend/README.md`; verificar login manual contra a API local

## 7. Ambiente local e CI

- [ ] 7.1 Criar Dockerfiles de `api` e `web` (apenas desenvolvimento) e `docker-compose.yml` (postgres:17, mailpit, seq, api, web) com healthchecks e `.env.example`; verificar `docker compose up` com `/health/ready` 200 e confirmar que `.env` está no `.gitignore`
- [ ] 7.2 Criar `.github/workflows/ci.yml` (restore/build, `dotnet format --verify-no-changes`, testes com cobertura, `npm ci`, lint, Vitest, build) e o gate de 80 % em Domain/Application; verificar execução verde no PR e falha simulada com cobertura reduzida
- [ ] 7.3 Escrever um teste de fumaça Playwright (login do admin semeado + recarga mantém a sessão + escrita sem token rejeitada) contra o compose; verificar execução local
- [ ] 7.4 Atualizar o `README.md` raiz (pré-requisitos, subir o ambiente, rodar testes) e a matriz de rastreabilidade do TDD §21 marcando a Fase 0

## 8. Verificação de integração

- [ ] 8.1 Verificar o critério de conclusão do TDD §22.2: admin semeado entra na SPA, a sessão sobrevive à recarga, escrita sem `X-XSRF-TOKEN` é rejeitada, o CI está verde e o login aparece na auditoria

## Workflow follow-up

- Abrir PR com `.github/pull_request_template.md` a partir de `feat/fundacao-plataforma` após build e testes verdes.
- Arquivar a change (`openspec-archive-change`) e criar a tag `v0.1.0` após o merge.

# Design

## Context

Projeto greenfield: o repositório tem apenas `docs/`, `openspec/` e `.github/`. O ambiente de desenvolvimento tem .NET SDK 10.0.103, Node 24 e Docker 29. Todas as decisões vêm do TDD-001 (`DT-xx`) e não são reabertas aqui; esta change só define a ordem e o desenho de montagem da Fase 0. Motivação em `proposal.md`.

## Goals / Non-Goals

**Goals:**
- Esqueleto executável de ponta a ponta: o admin semeado entra na SPA, a sessão sobrevive à recarga, escrita sem `X-XSRF-TOKEN` é rejeitada, o login é auditado e o CI fica verde (critério do TDD §22.2).
- Dependências entre camadas aplicadas desde o início (Domain sem referências; Application só Domain; Infrastructure implementa portas; Api é a raiz de composição).

**Non-Goals:**
- Nenhuma regra de negócio de domínio (pacientes, agenda, controles, cobrança) além de `Shared` (`Entity`, `AggregateRoot`, `DomainException`).
- Portal, e-mail (`SmtpEmailSender`), `deploy.yml`, imagens de produção, Nginx de produção e AWS.
- Cobertura mínima de frontend (DT-31 não define gate).

## Decisions

1. **Estrutura e build (DT-01, DT-05, DT-06):** `backend/Clinica.slnx` com `src/` (4 projetos) e `tests/` (3 projetos), pacotes centralizados em `Directory.Packages.props`, `TreatWarningsAsErrors` e `Nullable` em `Directory.Build.props`. Versões "última estável" são fixadas nesta change. Alternativa descartada: um único projeto — perderia a regra de dependência verificável.
2. **Pipeline MediatR (DT-02, DT-25):** ordem `Logging → Validation → Transaction → handler`. `AuditoriaLeituraBehavior` entra na pipeline mas só atua em queries com `[AuditarLeitura]` (sem uso real até a Fase 1; coberto por teste com query de teste). MediatR 14 exige chave de licença (DT-20): lida de configuração; sem ela, a API loga o aviso e segue (licença de uso não comercial/dev).
3. **Identity + sessão no servidor (DT-45, DT-18):** `AddIdentity` com tabelas renomeadas para o schema `identidade`; `SessaoTicketStore : ITicketStore` grava o ticket protegido em `sessoes` e renova `expira_em` após metade do prazo; Data Protection com `PersistKeysToDbContext` (`chaves_protecao_dados`). Eventos de cookie devolvem 401/403. Alternativa descartada: JWT + refresh (justificada na DT-45).
4. **Anti-CSRF:** `AddAntiforgery` com cabeçalho `X-XSRF-TOKEN` e cookie `__Host-XSRF-TOKEN`; `ValidacaoAntiforgeryFilter` aplicado ao `MapGroup("/api/v1")`, inclusive login. `GET /auth/sessao` e login/logout emitem o token novo.
5. **Auditoria (DT-19):** `AuditoriaInterceptor` (`SaveChangesInterceptor`) lê o `ChangeTracker` e grava `auditoria` na mesma transação. Login/falha de login são gravados por um serviço explícito chamado pelo handler de login. Para a regra "somente inserção", a migration cria o papel de banco da aplicação sem `UPDATE`/`DELETE` em `auditoria`; no ambiente local o compose usa esse papel.
6. **Tempo (DT-26):** `TimeProvider` registrado como singleton; `RelogioClinica` converte para `America/Sao_Paulo` (id IANA, Linux e Windows com ICU). Testes usam `FakeTimeProvider`.
7. **Job (DT-23):** `BackgroundService` único com JOB-02 diário (JOB-01 entra na Fase 1d); execução também na inicialização; falha logada, sem derrubar a API.
8. **Seeds (TDD §9.3):** perfis e parâmetros por `HasData`; administrador por seeder idempotente na inicialização (DT-20). `--seed-demo` fica para a Fase 4.
9. **Frontend (DT-07, DT-09, DT-10, DT-46, DT-47):** Angular 22 standalone/zoneless, Tailwind 4 com tokens em `@theme`, CDK, `@lucide/angular`. `provideAppInitializer` chama API-02; `withXsrfConfiguration` com URLs relativas; interceptors `sessaoInterceptor`, `errorInterceptor`, `correlationInterceptor`. `shared/ui` nesta fase: botão, campo de formulário, aviso (`LiveAnnouncer`), diálogo, tabela e paginação básicos (a DT-46 antecipa esse custo na Fase 0). Em desenvolvimento, `ng serve` usa proxy para a API para manter a mesma origem.
10. **Testes (DT-30, DT-31, DT-32):** MSTest 4 + NSubstitute + Shouldly; integração com `WebApplicationFactory` + Testcontainers PostgreSQL 17; Vitest no front; Playwright com um único teste de fumaça (login + recarga) no compose, executado localmente e documentado; gate de cobertura de 80 % em Domain/Application via coverlet e verificação no CI.
11. **CI (DT-36):** `ci.yml` em runner Ubuntu, com cache de NuGet e npm; Testcontainers usa o Docker do runner.

## Risks / Trade-offs

- [Pouco código em Domain/Application na Fase 0 pode tornar o gate de 80 % trivial ou instável] → medir só os assemblies com código; manter handlers de login/logout/sessão com testes unitários.
- [Licença comercial do MediatR 14] → configuração opcional documentada no README; verificar os termos antes de publicar o repositório (RT do TDD).
- [Angular 22 / Signal Forms / `@lucide/angular` podem ter APIs que mudam] → fixar versões exatas no `package.json` e consultar a documentação (context7) ao implementar.
- [`__Host-` exige HTTPS: não funciona em `http://localhost` em alguns navegadores] → Chromium trata `localhost` como contexto seguro; se falhar, usar o nome do cookie sem prefixo apenas em `Development` via configuração e registrar a exceção no TDD (DT-45) antes de implementar.
- [Papel de banco sem `UPDATE`/`DELETE` em `auditoria` complica migrations] → migrations rodam com o papel dono; a API usa o papel restrito; testar com Testcontainers.
- [Testes de integração exigem Docker no ambiente do desenvolvedor] → documentar no README; usar `Category` para rodar só os unitários.

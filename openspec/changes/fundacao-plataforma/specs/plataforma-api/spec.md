## ADDED Requirements

### Requirement: Versionamento e formatos da API
A API SHALL ser exposta sob `/api/v1`, em JSON camelCase, com datas de calendário `YYYY-MM-DD`, instantes ISO 8601 em UTC e identificadores UUID v7. Origem: DT-03, DT-13, DT-26.

#### Scenario: Rota versionada
- **WHEN** um cliente chama uma rota fora de `/api/v1` (exceto health e documentação)
- **THEN** a API responde 404

### Requirement: Erros em ProblemDetails
A API SHALL responder erros no formato ProblemDetails (RFC 9457) com `type`, `title`, `status`, `detail`, `traceId` e `code` de domínio; erros de validação MUST incluir `errors` por campo. Exceções não tratadas SHALL gerar 500 com `traceId` sem expor detalhes internos. Origem: DT-39, DT-25.

#### Scenario: Validação
- **WHEN** um comando com dados inválidos chega à API
- **THEN** responde 400 com `errors` indexado por campo e nenhuma alteração é persistida

#### Scenario: Erro inesperado
- **WHEN** ocorre exceção não tratada
- **THEN** responde 500 com `traceId` e sem stack trace

### Requirement: Transação por caso de uso
Cada command SHALL executar em uma única transação, confirmada somente se o handler concluir sem erro.

#### Scenario: Falha no handler
- **WHEN** um handler lança exceção após gravar parcialmente
- **THEN** nenhuma alteração do command permanece no banco

### Requirement: Health checks
A API SHALL expor `/health` (liveness, sem dependências) e `/health/ready` (verifica o PostgreSQL), acessíveis sem autenticação (API-46). Origem: DT-42.

#### Scenario: Banco indisponível
- **WHEN** o PostgreSQL não responde
- **THEN** `/health/ready` responde 503 e `/health` continua 200

### Requirement: Documentação OpenAPI em desenvolvimento
O sistema SHALL publicar o documento OpenAPI e a Swagger UI apenas no ambiente `Development`. Origem: DT-27.

#### Scenario: Ambiente de produção
- **WHEN** a API roda fora de `Development`
- **THEN** o documento OpenAPI e a Swagger UI não estão disponíveis

### Requirement: Logs estruturados sem dados pessoais
A API SHALL registrar logs JSON com `CorrelationId`, `UserId`, `Perfil`, `RequestPath` e `Elapsed`, e MUST NOT registrar corpo de requisição nem dados pessoais. Origem: DT-28, RNF-01.

#### Scenario: Requisição com corpo
- **WHEN** uma requisição de login é processada
- **THEN** o log contém o caminho e o tempo, mas não e-mail nem senha

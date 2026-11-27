## ADDED Requirements

### Requirement: Login interno por e-mail e senha
O sistema SHALL autenticar usuários ativos por e-mail e senha (API-01), criar uma sessão no servidor e entregar um cookie de sessão `HttpOnly`, `Secure`, `SameSite=Strict`, sem `Domain`, que leva apenas a chave da sessão. A resposta MUST conter id, nome, perfis, `pacienteId`/`medicoId` quando aplicável e `cadastroPendente`. Origem: RF-36, RNF-02, DT-45.

#### Scenario: Credenciais válidas
- **WHEN** um usuário ativo envia e-mail e senha corretos com token anti-CSRF válido
- **THEN** a API responde 200 com os dados do usuário, define o cookie de sessão e emite um novo token anti-CSRF

#### Scenario: Credenciais inválidas
- **WHEN** o e-mail ou a senha estão incorretos
- **THEN** a API responde 401 com ProblemDetails e nenhuma sessão é criada

#### Scenario: Usuário desativado
- **WHEN** um usuário com `ativo = false` informa credenciais corretas
- **THEN** a API responde 401 e nenhuma sessão é criada

### Requirement: Política de senha e bloqueio
O sistema SHALL exigir senhas com no mínimo 8 caracteres contendo letra e número e SHALL bloquear a conta por 15 minutos após 5 falhas consecutivas. Origem: DT-18.

#### Scenario: Bloqueio após cinco falhas
- **WHEN** ocorrem 5 tentativas de login falhas consecutivas para o mesmo usuário
- **THEN** a sexta tentativa, mesmo com a senha correta, é rejeitada até o fim de 15 minutos

#### Scenario: Senha fraca
- **WHEN** se tenta definir uma senha com menos de 8 caracteres ou sem letra ou sem número
- **THEN** a definição é rejeitada com erro de validação

### Requirement: Restauração da sessão
O sistema SHALL expor API-02, que retorna os dados do usuário da sessão ou 204 sem sessão, e SHALL sempre emitir o cookie legível `__Host-XSRF-TOKEN`. Origem: RF-36, RNF-02, DT-45.

#### Scenario: Sessão válida
- **WHEN** a SPA chama `GET /api/v1/auth/sessao` com cookie de sessão válido
- **THEN** a API responde 200 com os dados do usuário

#### Scenario: Sem sessão
- **WHEN** a chamada chega sem cookie ou com sessão expirada ou revogada
- **THEN** a API responde 204 e define `__Host-XSRF-TOKEN`

### Requirement: Logout e revogação imediata
O sistema SHALL revogar a sessão atual no logout (API-03) e MUST revogar todas as sessões de um usuário ao desativá-lo ou ao trocar ou redefinir sua senha, com efeito na requisição seguinte. Origem: RNF-02, DT-45.

#### Scenario: Logout
- **WHEN** um usuário autenticado chama `POST /api/v1/auth/logout`
- **THEN** a sessão é marcada como revogada, o cookie é removido e uma nova chamada com o cookie antigo recebe 401

### Requirement: Expiração de sessão
O sistema SHALL expirar a sessão após 30 minutos de inatividade (renovação deslizante após metade do prazo) e no máximo 12 horas após a criação. Rotas protegidas SHALL responder 401 (autenticação) ou 403 (autorização), sem redirecionamento HTML. Origem: DT-45.

#### Scenario: Inatividade
- **WHEN** passam 31 minutos sem requisições
- **THEN** a requisição seguinte recebe 401

#### Scenario: Limite absoluto
- **WHEN** a sessão tem 12 horas de vida, mesmo com atividade contínua
- **THEN** a requisição seguinte recebe 401

### Requirement: Proteção anti-CSRF
O sistema SHALL exigir o cabeçalho `X-XSRF-TOKEN` válido em todo `POST`, `PUT`, `PATCH` e `DELETE` sob `/api/v1`, inclusive o login. Origem: RNF-02, DT-45.

#### Scenario: Escrita sem token
- **WHEN** uma requisição de escrita chega sem `X-XSRF-TOKEN` válido
- **THEN** a API responde 400 com `code` `seguranca.antiforgery_invalido` e nada é alterado

### Requirement: Limpeza de sessões antigas
O sistema SHALL remover diariamente as sessões expiradas ou revogadas há mais de 30 dias, de forma idempotente (JOB-02).

#### Scenario: Sessão antiga
- **WHEN** a limpeza roda e existe sessão revogada há 31 dias
- **THEN** a linha é removida e sessões recentes permanecem

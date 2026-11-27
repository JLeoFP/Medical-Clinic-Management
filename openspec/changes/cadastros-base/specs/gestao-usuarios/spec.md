## ADDED Requirements

### Requirement: Listagem de usuários internos
O sistema SHALL listar, de forma paginada, os usuários internos (perfis Administrador, Médico e Recepção) com filtro opcional por perfil e situação, apenas ao Administrador (API-07). Origem: RF-37, DT-39.

#### Scenario: Administrador lista usuários
- **WHEN** o Administrador consulta `GET /api/v1/usuarios`
- **THEN** recebe `{ items, page, pageSize, totalItems }` sem contas do perfil Paciente

#### Scenario: Outro perfil é recusado
- **WHEN** um usuário Recepção ou Médico consulta `GET /api/v1/usuarios`
- **THEN** a resposta é 403

### Requirement: Criação e edição de usuários internos
O sistema SHALL permitir ao Administrador criar um usuário interno com e-mail único, nome, perfil (Administrador, Médico ou Recepção) e senha inicial conforme a política de senha, e editar seu nome, e-mail e perfil (API-08, API-09). O perfil Paciente MUST ser recusado. Origem: RF-36, RF-37, DT-18.

#### Scenario: Criação válida
- **WHEN** o Administrador cria um usuário Recepção com e-mail inédito e senha válida
- **THEN** o usuário é criado ativo e consegue autenticar

#### Scenario: E-mail duplicado
- **WHEN** o Administrador cria ou edita um usuário com e-mail já usado por outra conta
- **THEN** a resposta é 409 com `code` `usuario.email_duplicado`

#### Scenario: Senha fora da política
- **WHEN** a senha inicial não atende à política de senha
- **THEN** a resposta é 400 com erro no campo `senha`

#### Scenario: Perfil Paciente
- **WHEN** o Administrador tenta criar um usuário com perfil Paciente
- **THEN** a resposta é 400 com erro no campo `perfil`

### Requirement: Desativação de usuário
O sistema SHALL permitir ao Administrador desativar um usuário (API-10), impedindo seu login e revogando todas as suas sessões com efeito na requisição seguinte. O Administrador MUST NOT desativar a si mesmo. Origem: RF-37, DT-45.

#### Scenario: Usuário desativado não autentica
- **WHEN** um usuário é desativado e tenta fazer login
- **THEN** o login é recusado com 401

#### Scenario: Sessão aberta é revogada
- **WHEN** um usuário com sessão aberta é desativado
- **THEN** a próxima requisição dele recebe 401

#### Scenario: Autodesativação
- **WHEN** o Administrador tenta desativar a própria conta
- **THEN** a resposta é 422 com `code` `usuario.autodesativacao`

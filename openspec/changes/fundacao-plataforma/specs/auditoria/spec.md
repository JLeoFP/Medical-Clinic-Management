## ADDED Requirements

### Requirement: Registro de login e falha de login
O sistema SHALL gravar na tabela `auditoria` cada login bem-sucedido (`Login`) e cada falha (`LoginFalho`), com instante, usuário (quando conhecido), perfil, IP e `correlation_id`. Origem: RNF-03, DT-19.

#### Scenario: Login do administrador
- **WHEN** o administrador faz login com sucesso
- **THEN** existe uma linha de auditoria `Login` com seu `usuario_id`

#### Scenario: Senha incorreta
- **WHEN** um login falha
- **THEN** existe uma linha `LoginFalho` sem a senha informada

### Requirement: Auditoria automática de alterações
O sistema SHALL gravar `Criacao`/`Alteracao`/`Estorno` com `dados_antes` e `dados_depois` (apenas colunas alteradas) para as entidades auditadas, sem exigir código nos casos de uso. Origem: DT-19.

#### Scenario: Alteração de parâmetro
- **WHEN** uma entidade auditada é alterada e salva
- **THEN** a auditoria contém o valor anterior e o novo apenas das colunas alteradas

### Requirement: Auditoria de leitura declarativa
Queries marcadas como auditáveis SHALL gerar um registro `Leitura` com o usuário e, quando aplicável, o `paciente_id`.

#### Scenario: Query marcada
- **WHEN** uma query marcada como auditável é executada
- **THEN** uma linha `Leitura` é gravada

### Requirement: Trilha somente inserção
A tabela `auditoria` SHALL ser somente inserção: o usuário de banco da aplicação MUST NOT ter `UPDATE` nem `DELETE` sobre ela. Origem: RNF-03.

#### Scenario: Tentativa de alteração
- **WHEN** a aplicação tenta `UPDATE` ou `DELETE` em `auditoria`
- **THEN** o banco rejeita a operação

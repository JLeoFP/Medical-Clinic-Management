## ADDED Requirements

### Requirement: Cadastro de médicos
O sistema SHALL permitir ao Administrador cadastrar um médico vinculado a um usuário de perfil Médico, com nome, especialidade e registro profissional únicos (API-24), editá-lo ou inativá-lo (API-25), e listar os médicos ativos a qualquer usuário autenticado (API-23). Origem: RF-16, RF-37.

#### Scenario: Cadastro válido
- **WHEN** o Administrador cadastra um médico vinculado a um usuário Médico ainda sem médico
- **THEN** o médico é criado ativo e aparece em `GET /api/v1/medicos`

#### Scenario: Usuário sem perfil Médico
- **WHEN** o Administrador vincula o médico a um usuário que não tem perfil Médico
- **THEN** a resposta é 422 com `code` `medico.usuario_invalido`

#### Scenario: Registro profissional duplicado
- **WHEN** o Administrador informa um registro profissional já cadastrado
- **THEN** a resposta é 409 com `code` `medico.registro_duplicado`

#### Scenario: Médico inativo fora da listagem
- **WHEN** um médico é inativado
- **THEN** ele deixa de aparecer em `GET /api/v1/medicos`

#### Scenario: Edição concorrente
- **WHEN** duas edições do mesmo médico usam a mesma versão
- **THEN** a segunda recebe 409 de concorrência

### Requirement: Agenda semanal do médico
O sistema SHALL permitir ao Administrador substituir por inteiro a agenda semanal de um médico (API-27) com faixas de dia da semana (0–6), hora de início, hora de fim e duração do turno de 5 a 240 minutos, e consultar a agenda a Administrador e Recepção (API-26). Faixas sobrepostas no mesmo dia MUST ser rejeitadas. Origem: RF-16.

#### Scenario: Agenda válida
- **WHEN** o Administrador envia faixas de segunda e quarta, sem sobreposição
- **THEN** a agenda anterior é substituída e `GET /medicos/{id}/agenda` devolve apenas as novas faixas

#### Scenario: Faixas sobrepostas
- **WHEN** duas faixas do mesmo dia se sobrepõem (08:00–12:00 e 11:00–14:00)
- **THEN** a resposta é 422 com `code` `agenda.faixas_sobrepostas` e a agenda anterior é mantida

#### Scenario: Fim antes do início
- **WHEN** uma faixa tem `horaFim` menor ou igual a `horaInicio`
- **THEN** a resposta é 400 com erro no campo da faixa

#### Scenario: Duração fora do limite
- **WHEN** a duração do turno é menor que 5 ou maior que 240 minutos
- **THEN** a resposta é 400 com erro no campo `duracaoTurnoMin`

#### Scenario: Perfil sem permissão
- **WHEN** um usuário Médico ou Recepção tenta substituir a agenda
- **THEN** a resposta é 403

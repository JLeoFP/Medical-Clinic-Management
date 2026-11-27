## ADDED Requirements

### Requirement: Manutenção de tipos de atendimento
O sistema SHALL permitir ao Administrador criar, editar e inativar tipos de atendimento com nome único e tarifa vigente maior ou igual a zero (API-37 POST/PUT), e a Administrador, Médico e Recepção listá-los (API-37 GET). Origem: RF-20, RF-25.

#### Scenario: Criação válida
- **WHEN** o Administrador cria o tipo "Consulta" com tarifa 200,00
- **THEN** o tipo é criado ativo e aparece na listagem

#### Scenario: Nome duplicado
- **WHEN** o Administrador cria um tipo com nome já existente
- **THEN** a resposta é 409 com `code` `tipo_atendimento.nome_duplicado`

#### Scenario: Tarifa negativa
- **WHEN** a tarifa informada é negativa
- **THEN** a resposta é 400 com erro no campo `tarifaVigente`

#### Scenario: Alteração de tarifa
- **WHEN** o Administrador altera a tarifa de um tipo
- **THEN** a nova tarifa passa a ser a vigente e a alteração é registrada na auditoria com valor anterior e novo

#### Scenario: Escrita por perfil sem permissão
- **WHEN** um usuário Recepção ou Médico tenta criar ou editar um tipo
- **THEN** a resposta é 403, mas a listagem continua permitida

#### Scenario: Tipo inativo
- **WHEN** um tipo é inativado
- **THEN** ele deixa de ser oferecido para novos turnos, mantendo os registros existentes

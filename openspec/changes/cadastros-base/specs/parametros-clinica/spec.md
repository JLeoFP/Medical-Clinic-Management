## ADDED Requirements

### Requirement: Consulta de parâmetros
O sistema SHALL expor os parâmetros `janela_agenda_meses`, `desconto_dinheiro_percentual` e `antecedencia_cancelamento_horas` com seus valores padrão (2, 10 e 24) a qualquer usuário interno (API-45 GET). Origem: RF-17, RF-23, RF-27.

#### Scenario: Valores padrão
- **WHEN** um usuário interno consulta `GET /api/v1/parametros` em um banco recém-migrado
- **THEN** recebe janela 2, desconto 10 e antecedência 24

#### Scenario: Paciente não consulta
- **WHEN** um usuário Paciente consulta os parâmetros
- **THEN** a resposta é 403

### Requirement: Alteração de parâmetros
O sistema SHALL permitir ao Administrador alterar os parâmetros dentro das faixas válidas — janela de 1 a 12 meses, desconto de 0 a 100 % e antecedência de 0 a 168 horas — invalidando o cache e auditando a alteração (API-45 PUT). Origem: RF-17, RF-23, RF-27, DT-16, DT-19.

#### Scenario: Alteração válida
- **WHEN** o Administrador altera a janela para 3 meses
- **THEN** a consulta seguinte devolve 3 e o cache não serve o valor antigo

#### Scenario: Valor fora da faixa
- **WHEN** o Administrador informa janela 13, desconto 101 ou antecedência 169
- **THEN** a resposta é 400 com erro no campo correspondente e nada é alterado

#### Scenario: Auditoria da alteração
- **WHEN** um parâmetro é alterado
- **THEN** existe um registro `Alteracao` com valor anterior e novo e o usuário

#### Scenario: Perfil sem permissão
- **WHEN** um usuário Recepção ou Médico tenta alterar um parâmetro
- **THEN** a resposta é 403

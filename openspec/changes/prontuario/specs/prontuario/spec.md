## ADDED Requirements

### Requirement: Registro de evolução clínica
O sistema SHALL permitir ao Médico registrar uma evolução para um paciente com motivo (obrigatório), observações, medicamentos e indicações (API-16), gravando o médico da sessão e a data do atendimento. Quando a evolução é vinculada a um turno `Agendado` do mesmo paciente e médico, o turno MUST passar a `Atendido` na mesma transação. Origem: RF-04, CMP-03.

#### Scenario: Evolução simples
- **WHEN** o Médico registra uma evolução com motivo e sem próximo controle
- **THEN** a resposta é 201 e a evolução fica na história clínica do paciente

#### Scenario: Evolução vinculada a turno
- **WHEN** o Médico registra a evolução informando o `turnoId` de um turno `Agendado` do paciente
- **THEN** o turno passa a `Atendido` e uma segunda evolução para o mesmo turno é rejeitada

#### Scenario: Turno de outro paciente
- **WHEN** o `turnoId` pertence a outro paciente ou a outro médico
- **THEN** a resposta é 422 com `code` `evolucao.turno_invalido` e nada é gravado

#### Scenario: Motivo obrigatório
- **WHEN** o motivo está vazio
- **THEN** a resposta é 400 com erro no campo `motivo`

#### Scenario: Usuário Médico sem cadastro de médico
- **WHEN** o usuário tem perfil Médico mas nenhum médico vinculado
- **THEN** a resposta é 422 com `code` `medico.nao_cadastrado`

### Requirement: Leitura da história clínica
O sistema SHALL permitir a qualquer Médico da instituição ler a história clínica completa de um paciente em ordem cronológica (API-15), independentemente do autor das evoluções, e MUST registrar cada leitura na auditoria com usuário e `paciente_id`. Origem: RF-05, RN-07, RNF-03, HU-04.

#### Scenario: Outro médico lê a história
- **WHEN** o Dr. B abre a história de uma paciente atendida pelo Dr. A
- **THEN** vê todas as evoluções em ordem cronológica, com o autor de cada uma

#### Scenario: Leitura auditada
- **WHEN** um Médico consulta o prontuário
- **THEN** existe um registro `Leitura` com o usuário e o `paciente_id`

#### Scenario: Paciente sem evoluções
- **WHEN** o paciente não tem evoluções
- **THEN** a resposta é 200 com lista vazia

### Requirement: Acesso exclusivo do Médico ao conteúdo clínico
O sistema MUST restringir leitura, registro e correção de evoluções ao perfil Médico; Administrador, Recepção e Paciente MUST receber 403, e nenhuma resposta de outro módulo MUST conter campos clínicos. Origem: RF-06, RN-07, RNF-01, HU-04.

#### Scenario: Recepção não acessa o prontuário
- **WHEN** a Recepção chama `GET /pacientes/{id}/evolucoes`
- **THEN** a resposta é 403

#### Scenario: Administrador não acessa o prontuário
- **WHEN** o Administrador tenta ler, registrar ou corrigir uma evolução
- **THEN** a resposta é 403

#### Scenario: Ficha sem conteúdo clínico
- **WHEN** a Recepção abre a ficha da paciente
- **THEN** vê dados pessoais e a data do próximo controle, mas nenhum campo de evolução

### Requirement: Correção de evolução
O sistema SHALL permitir ao Médico corrigir uma evolução (API-17), sem excluí-la, registrando na auditoria os valores anteriores e novos. Origem: RF-04, RF-06, DT-15, DT-19.

#### Scenario: Correção auditada
- **WHEN** o Médico altera as observações de uma evolução
- **THEN** a alteração é gravada e a auditoria contém `dados_antes` e `dados_depois` apenas das colunas alteradas

#### Scenario: Exclusão não existe
- **WHEN** se procura uma rota de exclusão de evolução
- **THEN** ela não existe

### Requirement: Início e prontuário na SPA do Médico
A SPA SHALL abrir `/app/inicio` do Médico com seus turnos do dia e oferecer `/app/pacientes/:id/prontuario` somente ao perfil Médico. Origem: RNF-04, TDD §12.1.

#### Scenario: Médico abre o início
- **WHEN** um usuário Médico entra em `/app/inicio`
- **THEN** vê seus turnos do dia

#### Scenario: Recepção navega ao prontuário
- **WHEN** a Recepção navega para `/app/pacientes/:id/prontuario`
- **THEN** é redirecionada e nenhum dado clínico é solicitado à API

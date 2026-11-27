## ADDED Requirements

### Requirement: Criação do controle na evolução
O sistema SHALL permitir ao Médico, ao registrar a evolução, indicar o próximo controle como prazo (quantidade de 1 a 60 em semanas ou meses) ou data aproximada, calculando a data prevista a partir da data local do atendimento e criando o controle vinculado à evolução. Origem: RF-07, RF-09, HU-01.

#### Scenario: Controle fora da janela
- **WHEN** o Médico registra a consulta de 2026-11-18 com "próximo controle em 6 meses" e a janela é de 2 meses
- **THEN** o controle é criado com data prevista 2027-05-18 e status `Pendente`, visível na história clínica do paciente

#### Scenario: Controle em semanas
- **WHEN** o prazo é "em 3 semanas" em 2026-11-18
- **THEN** a data prevista é 2026-12-09

#### Scenario: Data aproximada
- **WHEN** o Médico informa uma data aproximada futura
- **THEN** o controle usa essa data como data prevista

#### Scenario: Prazo e data juntos ou nenhum válido
- **WHEN** o Médico informa prazo e data aproximada ao mesmo tempo, quantidade fora de 1–60 ou data no passado
- **THEN** a resposta é 400 com erro no campo `proximoControle`

### Requirement: Sugestão de agendamento para controle na janela
O sistema SHALL, quando o controle nasce dentro da janela de agenda, devolver na resposta da evolução até 5 horários livres do médico próximos da data prevista para marcação imediata. Origem: RF-08, HU-01.

#### Scenario: Controle dentro da janela
- **WHEN** o Médico registra a consulta de 2026-11-18 com "próximo controle em 1 mês"
- **THEN** o controle nasce `NaJanela` e a resposta traz sugestões de horários próximos de 2026-12-18

#### Scenario: Sem horários disponíveis
- **WHEN** o médico não tem horários livres na janela
- **THEN** o controle é criado `NaJanela` e `sugestaoAgendamento` vem vazia

### Requirement: Máquina de estados do controle
O sistema SHALL manter o status do controle conforme o fluxo `Pendente → NaJanela → Contatado → Agendado → Realizado`, com `Agendado → NaJanela` quando o turno é cancelado e `Cancelado` a partir de `Pendente`, `NaJanela` ou `Contatado`; qualquer outra transição MUST ser rejeitada com 422 `controle.transicao_invalida`. Origem: RF-09, PRD §9.1.

#### Scenario: Transição inválida
- **WHEN** se tenta cancelar um controle `Realizado` ou marcar contato em um controle `Pendente`
- **THEN** a resposta é 422 com `code` `controle.transicao_invalida` e o status não muda

#### Scenario: Primeira tentativa de contato
- **WHEN** a primeira tentativa de contato é registrada em um controle `NaJanela`
- **THEN** o controle passa a `Contatado`

### Requirement: Vínculo entre turno e controle
O sistema SHALL vincular o controle ao turno marcado pela Recepção: com `controleId` informado na marcação (campo opcional de API-30) ou, na ausência dele, o controle `NaJanela`/`Contatado` mais antigo do mesmo paciente e médico; o controle passa a `Agendado` e sai da lista de trabalho. Ao cancelar o turno o controle volta a `NaJanela`; ao atender o turno vai a `Realizado`, tudo na mesma transação do turno. Origem: RF-13, HU-03.

#### Scenario: Turno marcado para um controle
- **WHEN** a Recepção marca um turno para a Ana com o médico responsável, existindo um controle `Contatado` dela
- **THEN** o controle passa a `Agendado` e deixa a lista de trabalho

#### Scenario: Atendimento registrado
- **WHEN** o Médico registra a evolução do turno vinculado
- **THEN** o controle passa a `Realizado`

#### Scenario: Turno cancelado
- **WHEN** o turno vinculado é cancelado
- **THEN** o controle volta a `NaJanela` sem turno e reaparece na lista de trabalho

#### Scenario: `controleId` inválido
- **WHEN** o `controleId` é de outro paciente ou não está `NaJanela`/`Contatado`
- **THEN** a resposta é 422 com `code` `controle.nao_vinculavel` e nenhum turno é criado

#### Scenario: Marcação sem controle elegível
- **WHEN** o paciente não tem controle `NaJanela`/`Contatado` com o médico
- **THEN** o turno é marcado normalmente e nenhum controle é alterado

### Requirement: Cancelamento manual do controle
O sistema SHALL permitir a Administrador, Médico e Recepção cancelar um controle `Pendente`, `NaJanela` ou `Contatado` informando o motivo obrigatório (API-21). Origem: RF-09.

#### Scenario: Cancelamento com motivo
- **WHEN** a Recepção cancela um controle `Contatado` informando o motivo
- **THEN** o controle passa a `Cancelado`, guarda o motivo e sai da lista de trabalho

#### Scenario: Cancelamento sem motivo
- **WHEN** o motivo está vazio
- **THEN** a resposta é 400 com erro no campo `motivo`

### Requirement: Consulta geral de controles
O sistema SHALL permitir a Administrador, Médico e Recepção consultar todos os controles, inclusive `Pendente`, com filtros por médico, status e intervalo de datas previstas, paginados (API-19), sem expor conteúdo clínico. Origem: RF-14, RNF-01.

#### Scenario: Filtro por status e médico
- **WHEN** a consulta usa `status=Pendente` e um `medicoId`
- **THEN** só controles pendentes desse médico são devolvidos, ordenados por data prevista

#### Scenario: Intervalo de datas
- **WHEN** a consulta usa `de` e `ate`
- **THEN** só controles com data prevista no intervalo (inclusive) são devolvidos

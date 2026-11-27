## ADDED Requirements

### Requirement: Horários livres dentro da janela
O sistema SHALL devolver a Administrador e Recepção os horários livres de um médico ativo em uma data (API-28), gerados pela agenda semanal do dia da semana, descontando turnos não cancelados e limitados ao período de hoje até hoje mais a janela de agenda (padrão 2 meses). Origem: RF-17, RN-01.

#### Scenario: Slots de uma agenda
- **WHEN** a agenda de segunda tem 08:00–10:00 com turnos de 30 minutos e há um turno às 08:30
- **THEN** os horários livres dessa segunda são 08:00, 09:00 e 09:30

#### Scenario: Cancelamento libera o horário
- **WHEN** o turno das 08:30 é cancelado
- **THEN** 08:30 volta a aparecer entre os horários livres

#### Scenario: Data além da janela
- **WHEN** a data consultada é posterior a hoje mais a janela configurada
- **THEN** a resposta é 422 com `code` `turno.fora_da_janela`

#### Scenario: Dia sem agenda
- **WHEN** o médico não tem faixa no dia da semana consultado
- **THEN** a lista de horários livres é vazia

### Requirement: Marcação de turno
O sistema SHALL permitir a Administrador e Recepção marcar um turno informando paciente, médico, tipo de atendimento e horário de início (API-30). O horário MUST coincidir com um slot da agenda do médico, estar dentro da janela e não estar no passado; médico e tipo MUST estar ativos. Origem: RF-17, RF-18, RF-20, RN-01.

#### Scenario: Marcação válida
- **WHEN** a Recepção marca um slot livre dentro da janela
- **THEN** a resposta é 201, o turno fica `Agendado` com `fim` igual ao início mais a duração da faixa e a origem `Recepcao`

#### Scenario: Fora da janela
- **WHEN** o início local é posterior a hoje mais a janela (ex.: janela de 2 meses, início em 3 meses)
- **THEN** a resposta é 422 com `code` `turno.fora_da_janela`

#### Scenario: Horário fora da grade
- **WHEN** o início não coincide com um slot da agenda do médico (ex.: 08:10 em turnos de 30 minutos)
- **THEN** a resposta é 422 com `code` `turno.horario_invalido`

#### Scenario: Horário no passado
- **WHEN** o início é anterior ao instante atual
- **THEN** a resposta é 422 com `code` `turno.no_passado`

#### Scenario: Médico ou tipo inativo
- **WHEN** o médico ou o tipo de atendimento está inativo
- **THEN** a resposta é 422 com `code` `turno.medico_inativo` ou `turno.tipo_inativo` e nenhum turno é criado

#### Scenario: Médico não marca turno
- **WHEN** um usuário Médico tenta marcar um turno
- **THEN** a resposta é 403

### Requirement: Unicidade de horário por médico
O sistema MUST impedir dois turnos não cancelados no mesmo horário para o mesmo médico, inclusive sob requisições concorrentes, respondendo 409 `turno.horario_ocupado` à perdedora. Origem: RF-19, DT-14.

#### Scenario: Horário já ocupado
- **WHEN** a Recepção marca um horário em que o médico já tem turno `Agendado`
- **THEN** a resposta é 409 com `code` `turno.horario_ocupado`

#### Scenario: Corrida concorrente
- **WHEN** duas marcações simultâneas disputam o mesmo médico e horário
- **THEN** uma retorna 201, a outra 409, e existe exatamente um turno não cancelado nesse horário

#### Scenario: Horário de turno cancelado
- **WHEN** existe apenas um turno `Cancelado` no horário
- **THEN** uma nova marcação nesse horário é aceita

### Requirement: Cancelamento de turno
O sistema SHALL permitir a Administrador e Recepção cancelar um turno `Agendado`, com motivo opcional, sem apagar o registro e sem exigir antecedência mínima (API-32). Origem: RF-18, DT-15.

#### Scenario: Cancelamento válido
- **WHEN** a Recepção cancela um turno `Agendado`
- **THEN** o turno passa a `Cancelado` com `cancelado_em` e motivo, e continua consultável

#### Scenario: Turno já cancelado ou atendido
- **WHEN** a Recepção tenta cancelar um turno `Cancelado` ou `Atendido`
- **THEN** a resposta é 422 com `code` `turno.status_invalido`

### Requirement: Reprogramação de turno
O sistema SHALL permitir a Administrador e Recepção reprogramar um turno `Agendado` para outro horário livre do mesmo médico, mantendo o mesmo turno e aplicando as mesmas regras de janela, grade e unicidade da marcação (API-31). Origem: RF-18, RF-19.

#### Scenario: Reprogramação válida
- **WHEN** a Recepção move o turno para outro horário livre
- **THEN** o mesmo turno (mesmo id) tem novo `inicio` e `fim`, e o horário antigo fica livre

#### Scenario: Novo horário ocupado
- **WHEN** o novo horário já tem turno do médico
- **THEN** a resposta é 409 com `code` `turno.horario_ocupado` e o turno original permanece inalterado

#### Scenario: Reprogramação fora da janela
- **WHEN** o novo horário está além da janela
- **THEN** a resposta é 422 com `code` `turno.fora_da_janela`

### Requirement: Consulta de turnos
O sistema SHALL listar turnos por data ou intervalo e por médico, ordenados por início, a Administrador, Médico e Recepção (API-29), incluindo paciente, médico, tipo e status. Origem: RF-18, TDD §13.2.

#### Scenario: Turnos do dia
- **WHEN** a Recepção consulta os turnos de uma data para um médico
- **THEN** recebe os turnos do dia ordenados por início, inclusive cancelados identificados pelo status

#### Scenario: Médico lê a agenda
- **WHEN** um usuário Médico consulta turnos
- **THEN** a resposta é 200, mas qualquer escrita retorna 403

### Requirement: Eventos de turno
O sistema SHALL emitir os eventos `TurnoAgendado`, `TurnoCancelado` e `TurnoAtendido` na mesma transação em que o estado do turno muda, para consumo por outros módulos. Origem: CMP-04, DT-02.

#### Scenario: Evento na marcação
- **WHEN** um turno é marcado com sucesso
- **THEN** um `TurnoAgendado` é publicado antes do commit, e uma falha no commit descarta o evento

#### Scenario: Evento no cancelamento
- **WHEN** um turno é cancelado
- **THEN** um `TurnoCancelado` é publicado na mesma transação

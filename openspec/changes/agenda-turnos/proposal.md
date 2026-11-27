## Why

Com usuários, médicos e agendas configurados (1a) e pacientes cadastrados (1b), a Recepção precisa marcar, cancelar e reprogramar turnos sem conflitos. Este é o incremento **1c** da Fase 1 (TDD §22.3): a agenda é o ponto onde a regra mais crítica do sistema — dois turnos nunca no mesmo horário do mesmo médico (RF-19) — deve valer mesmo com requisições concorrentes.

## What Changes

- Cálculo de horários livres de um médico em uma data, dentro da janela de agenda aberta (`hoje` até `hoje + janela`), a partir da `AgendaSemanal` menos os turnos não cancelados (API-28; RF-17, RN-01).
- Marcação de turno pela Recepção/Administrador com paciente, médico, tipo de atendimento e horário que coincida com a grade (API-30; RF-17 a RF-20), com erros `turno.fora_da_janela`, `turno.horario_invalido` e `turno.horario_ocupado`.
- Cancelamento e reprogramação de turnos pela Recepção, mantendo o mesmo turno na reprogramação (API-31, API-32; RF-18).
- Consulta de turnos por data/período e médico (API-29; RF-18), com leitura para o Médico.
- Índice único parcial `ux_turnos_medico_inicio` e concorrência otimista (DT-14): corrida concorrente → 1 sucesso e 1 × 409.
- Eventos de domínio `TurnoAgendado`, `TurnoCancelado` e `TurnoAtendido` (este último disparável pela change 1d) e despachante de eventos dentro da transação, para que a change 1d atualize controles de forma atômica.
- SPA `/app/agenda`: visão diária/semanal por médico; marcar, cancelar e reprogramar; leitura para o Médico.
- Migration aditiva com `turnos`.

Fora desta change: vínculo do turno a um controle futuro e a transição de status do controle (1d), turnos pelo portal e antecedência mínima do paciente (Fase 2, API-33 a API-36), cobrança (Fase 3).

## Capabilities

### New Capabilities
- `turnos-agenda`: horários livres, marcação, cancelamento, reprogramação e consulta de turnos, com regras de janela, grade e unicidade por médico/horário.

### Modified Capabilities
<!-- Nenhuma. A leitura de `medicos-agenda` (cadastros-base) é consumida, não alterada. -->

## Impact

- Backend: agregado `Turno` com máquina de estados (`Agendado`, `Cancelado`, `Atendido`), serviço de domínio de geração de slots, despachante de eventos, handlers e endpoints.
- Frontend: feature `agenda` com `TurnosApi` e `AgendaStore`.
- Rastreabilidade: RF-17, RF-18, RF-19, RF-20, RN-01, RNF-04 (marcar turno em ≤ 3 telas), RNF-03; DT-14, DT-15, DT-26, DT-39; CMP-04; TDD §13.2 (Turnos).
- Dependências: `cadastros-base` (médicos, agendas, tipos, parâmetros) e `pacientes`. Habilita 1d.

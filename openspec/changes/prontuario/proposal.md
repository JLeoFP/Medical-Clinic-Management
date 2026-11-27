## Why

O médico precisa registrar e consultar a história clínica do paciente, com acesso restrito ao perfil Médico. Este é o incremento **1d-i** da Fase 1 (TDD §22.3, parte do 1d): o prontuário isolado e testável, que antecede o acompanhamento de controles (`controles-futuros`, 1d-ii), o qual se apoia nas evoluções.

## What Changes

- O Médico registra evoluções (motivo, observações, medicamentos, indicações) e lê a história clínica completa, em ordem cronológica e com o autor de cada evolução (API-15, API-16, API-17; RF-04 a RF-06, RN-07).
- Registrar a evolução vinculada a um turno `Agendado` do mesmo paciente e médico marca o turno como `Atendido` (evento `TurnoAtendido` da change `agenda-turnos`).
- Correção de evolução auditada com antes/depois; sem exclusão (DT-15, DT-19).
- Acesso exclusivo do Médico (policy `ConteudoClinico`); Administrador, Recepção e Paciente recebem 403; nenhum DTO de outro módulo expõe campo clínico (RNF-01). Toda leitura do prontuário é auditada.
- SPA: `/app/pacientes/:id/prontuario` (história + formulário de nova evolução) e `/app/inicio` do Médico com os turnos do dia.
- Migration aditiva com `evolucoes`.

Fora desta change: próximo controle, sugestão de agendamento, máquina de estados, JOB-01, lista de trabalho e tentativas de contato (todos em `controles-futuros`). O contrato de API-16 já reserva o campo opcional `proximoControle`, que será implementado lá.

## Capabilities

### New Capabilities
- `prontuario`: evoluções clínicas, leitura cronológica, correção auditada e acesso exclusivo do Médico.

### Modified Capabilities
<!-- Nenhuma. -->

## Impact

- Backend: agregado `Evolucao`, handlers e endpoints, policy `ConteudoClinico`, `[AuditarLeitura]`.
- Frontend: feature `prontuario` e início do Médico.
- Rastreabilidade: RF-04, RF-05, RF-06, RN-07, RNF-01, RNF-03; HU-04; DT-14, DT-15, DT-19; CMP-03; TDD §13.2.
- Dependências: `cadastros-base`, `pacientes` e `agenda-turnos`. Habilita `controles-futuros`.

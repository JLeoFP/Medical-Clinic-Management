# Tasks

> Origem: TDD-001 §22.3, incremento 1c (Fase 1, MVP v1.0). Branch sugerida: `feat/agenda-turnos`. Pré-requisitos: `cadastros-base` e `pacientes` integrados.

## 1. Domínio

- [ ] 1.1 Criar o agregado `Turno` (`IAuditavel`, estados `Agendado`/`Cancelado`/`Atendido`, `Cancelar`, `Reprogramar`, `MarcarAtendido`) com os eventos `TurnoAgendado`, `TurnoCancelado` e `TurnoAtendido`; testes unitários de todas as transições válidas e inválidas (`turno.status_invalido`)
- [ ] 1.2 Criar o serviço de domínio `CalculadoraHorarios` (slots da faixa, último slot inteiro, desconto de ocupados, corte de hora passada); testes unitários com `FakeTimeProvider`: faixa 08:00–10:00/30 min com turno às 08:30, faixa não múltipla da duração, dia sem agenda
- [ ] 1.3 Implementar regras de janela e grade (`turno.fora_da_janela`, `turno.horario_invalido`, `turno.no_passado`) com testes de limite: último dia da janela, janela alterada por parâmetro, fuso `America/Sao_Paulo`

## 2. Persistência e eventos

- [ ] 2.1 Mapear `Turno` no EF e criar a migration aditiva `turnos` com FKs `restrict`, `CHECK`s de status/origem, `xmin`, índice único parcial `ux_turnos_medico_inicio` e índices `(paciente_id, inicio)` e `(inicio)`; teste de integração: índice rejeita duplicata ativa e aceita sobre turno cancelado
- [ ] 2.2 Implementar o despachante de eventos de domínio (MediatR `INotification`) executado dentro da transação antes do commit; testes: evento publicado no sucesso, descartado em falha do commit, handler com exceção desfaz a transação

## 3. API

- [ ] 3.1 Implementar `ObterHorariosLivresQuery` (API-28) com policies Administrador/Recepção/Paciente; testes de integração: slots, cancelamento libera, data além da janela, dia sem agenda
- [ ] 3.2 Implementar `MarcarTurnoCommand` (API-30) com validações (paciente, médico e tipo ativos) e tradução do `23505` para 409 `turno.horario_ocupado`; testes de integração: marcação válida, fora da janela, fora da grade, passado, inativos, 403 para Médico e **corrida concorrente → 1×201 e 1×409** (RF-19)
- [ ] 3.3 Implementar `ReprogramarTurnoCommand` e `CancelarTurnoCommand` (API-31, API-32); testes: mesmo id após reprogramar, horário antigo livre, conflito preserva o turno original, cancelar já cancelado/atendido → 422, sem exigência de antecedência para a Recepção
- [ ] 3.4 Implementar `ListarTurnosQuery` (API-29) por data/intervalo e médico, com leitura para Médico; testes: ordenação, cancelados visíveis, 403 de escrita para Médico e Paciente
- [ ] 3.5 Registrar `turnos` na auditoria (criação, alteração e cancelamento com antes/depois); teste de integração da trilha

## 4. Frontend

- [ ] 4.1 Criar a feature `agenda` com `TurnosApi`, `AgendaStore` e a rota lazy `/app/agenda` (guard Administrador, Recepção e Médico somente leitura); testes Vitest do store e do guard
- [ ] 4.2 Implementar a visão diária e semanal por médico com seleção de data e navegação por teclado; teste Vitest de renderização dos turnos e dos estados vazio/cancelado
- [ ] 4.3 Implementar o diálogo de marcação (busca de paciente → tipo e horário livre) em no máximo 3 telas, cancelamento com confirmação e reprogramação; o 409 recarrega os horários livres e avisa o usuário; testes Vitest dos fluxos e do tratamento de `turno.*`
- [ ] 4.4 Verificar acessibilidade básica (foco nos diálogos, `LiveAnnouncer`, contraste de cancelados com ícone e texto)

## 5. Rastreabilidade e verificação

- [ ] 5.1 Atualizar a matriz do TDD §21 (RF-17 a RF-20, RN-01, RNF-04) e documentar no código o contrato dos eventos para a change 1d
- [ ] 5.2 Executar build, lint e todos os testes verdes no CI e conferir o critério do incremento 1c: janela de 2 meses e corrida concorrente com 1 sucesso e 1 × 409

## Workflow follow-up

- Abrir PR com `.github/pull_request_template.md` a partir de `feat/agenda-turnos` após build e testes verdes.
- Arquivar a change (`openspec-archive-change`) após o merge.

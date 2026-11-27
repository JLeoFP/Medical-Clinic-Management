# Design

## Context

CMP-04 (TDD §8.4) e DT-14 definem as regras. Já existem `RelogioClinica` (fuso `America/Sao_Paulo`), `IParametros` (change 1a), `Medico`/`AgendaSemanal`, `TipoAtendimento` e `Paciente` (1a/1b), `AggregateRoot` com `IDomainEvent` (Fase 0) e a infraestrutura de integração com Testcontainers.

## Goals / Non-Goals

**Goals:**
- Marcação correta sob concorrência (RF-19) e dentro da janela (RF-17).
- Eventos de turno disponíveis para o ciclo de vida do controle (1d) sem retrabalho.

**Non-Goals:**
- Antecedência mínima e operações do paciente (Fase 2); `controleId` na marcação (1d acrescenta o campo opcional de forma compatível).

## Decisions

1. **Agregado `Turno` (DT-01):** estados `Agendado → Cancelado | Atendido`; `Cancelar(motivo)`, `Reprogramar(novoInicio, novoFim)` e `MarcarAtendido()` (chamado por 1d) validam o estado e lançam `DomainException` em transição inválida (`turno.status_invalido`). `fim = inicio + duração da faixa`. Reprogramar altera `inicio/fim` do mesmo turno (mantém o vínculo com o controle da 1d).
2. **Geração de slots (serviço de domínio puro):** `CalculadoraHorarios` recebe a agenda do dia da semana, a data local e os turnos ocupados, e devolve slots de `hora_inicio` somando `duracao` enquanto o slot caiba inteiro na faixa. A data de consulta é limitada a `[hoje, hoje + janela]`; fora disso a API devolve lista vazia para datas passadas e `turno.fora_da_janela` para datas além da janela. Horário passado do dia corrente não é oferecido (suposição).
3. **Janela e fuso (DT-26):** comparações em data local de `RelogioClinica`; `inicio` é gravado em UTC (`timestamptz`) e convertido a partir da hora local da grade. `inicio` local com data > `hoje.AddMonths(janela)` → 422 `turno.fora_da_janela`.
4. **Concorrência (DT-14):** além da checagem no handler, o índice `ux_turnos_medico_inicio` (`UNIQUE (medico_id, inicio) WHERE status <> 'Cancelado'`) é a garantia; a violação (`23505`) é traduzida para 409 `turno.horario_ocupado`. Cancelar libera o horário porque o índice é parcial. `xmin` evita lost update em reprogramações simultâneas.
5. **Validações de marcação:** médico e tipo de atendimento devem estar ativos; paciente existente; slot deve coincidir com a grade (`turno.horario_invalido`); horário no passado → 422 `turno.no_passado`. Recepção/Administrador **não** estão sujeitos à antecedência mínima (RN-08 é só do portal).
6. **Eventos (DT-02):** `Turno` levanta `TurnoAgendado`, `TurnoCancelado` e `TurnoAtendido`; um despachante no `SaveChanges`/`TransactionBehavior` publica os eventos (MediatR `INotification`) **antes do commit** e na mesma transação, para que a 1d atualize `controles_futuros` atomicamente. Sem assinantes nesta change.
7. **Consultas:** `GET /turnos?data=&medicoId=` (e intervalo `de/ate` para a visão semanal) devolve turnos com nome do paciente e do tipo; Médico lê, mas só Administrador e Recepção escrevem (TDD §13.2). Ordenação por `inicio`.
8. **Frontend (DT-09, DT-46):** `AgendaStore` com médico, data/semana e turnos em signals; grade diária e semanal em tabelas/CDK (sem biblioteca de calendário); marcação em um diálogo de 2 passos (paciente via busca de 1b → tipo e horário livre), cumprindo RNF-04. Conflito 409 recarrega os horários livres e informa o usuário.

## Risks / Trade-offs

- [Alterar a agenda do médico depois de haver turnos] → turnos existentes mantêm `inicio/fim`; novos horários seguem a grade nova. Sem bloqueio no MVP (risco aceito, documentado na tela do administrador).
- [Mudança de horário de verão] → o Brasil não adota atualmente; a conversão usa a base IANA e testes com datas fixas protegem o comportamento.
- [Despachar eventos antes do commit acopla handlers à transação] → decisão deliberada para a atomicidade do controle; handlers não podem fazer I/O externo (o e-mail é Fase 2, fora da transação).
- [Slots calculados em memória por dia] → volume pequeno (1 médico × 1 dia); sem cache.

# Design

## Context

CMP-05 e CMP-09 (TDD §8.5, §8.9), os fluxos §11.1 e §11.2, a máquina de estados do PRD §9.1 e JOB-01 (§15). Ao iniciar existem: evoluções (`prontuario`), eventos `TurnoAgendado/Cancelado/Atendido` com despacho transacional e `CalculadoraHorarios` (`agenda-turnos`), `proximoControle: null` na ficha (`pacientes`), `BackgroundService` da Fase 0 (JOB-02) e `IAuditoria` com usuário nulo para o job.

## Fluxo resumido

```
Evolução (Médico) ──> Controle Pendente ──(JOB-01: data ≤ hoje+janela)──> NaJanela
NaJanela ──(1ª tentativa de contato)──> Contatado
NaJanela|Contatado ──(turno marcado)──> Agendado ──(turno atendido)──> Realizado
Agendado ──(turno cancelado)──> NaJanela
Pendente|NaJanela|Contatado ──(cancelar com motivo)──> Cancelado
```

## Goals / Non-Goals

**Goals:** fluxo do PRD §9.1 ponta a ponta na SPA (E2E fluxo 1); cenários Gherkin de HU-01 a HU-03.

**Non-Goals:** indicadores, tela do Administrador, avisos automáticos, portal.

## Decisions

1. **`PrazoControle` (RF-07, DT-51):** `{quantidade 1–60, unidade Semanas|Meses}` → `dataAtendimento.AddMonths(n)` ou `AddDays(7·n)` (2026-11-18 + 6 meses = 2027-05-18); ou `dataAproximada` (≥ hoje). Exatamente uma forma. Data-base = data local do registro da evolução (suposição documentada).
2. **`ControleFuturo`:** estados e transições da figura acima; transição inválida → `DomainException` → 422 `controle.transicao_invalida`. Status inicial *NaJanela* se `dataPrevista ≤ hoje.AddMonths(janela)`, senão *Pendente*. `xmin` protege atualizações concorrentes. Criado na mesma transação da evolução.
3. **Sugestão (RF-08):** para controle que nasce *NaJanela*, API-16 devolve até 5 horários livres do médico a partir da data prevista, reutilizando `CalculadoraHorarios`.
4. **Vínculo com turnos (RF-13, DT-50):** handlers dos eventos rodam na transação do turno. `TurnoAgendado`: `controleId` explícito é validado (mesmo paciente, *NaJanela*/*Contatado*, senão 422 `controle.nao_vinculavel`); sem `controleId`, vincula o *NaJanela*/*Contatado* mais antigo do mesmo paciente e médico (PRD §9.2). `TurnoCancelado` → *NaJanela* e libera `turno_id`; `TurnoAtendido` → *Realizado*. Campo `controleId` em API-30 é opcional (compatível).
5. **Lista de trabalho:** `status ∈ {NaJanela, Contatado}` e `turno_id IS NULL`, por `data_prevista`, com telefone, e-mail e telefone do familiar; `vencido = data_prevista < hoje` na consulta. Tentativas só em *NaJanela*/*Contatado*; `funcionario_id` da sessão; resultado `Agendou` não muda o status (quem muda é o turno). Consulta geral (API-19) inclui *Pendentes*.
6. **JOB-01 (DT-23):** no `BackgroundService`, na inicialização e às 00:05 `America/Sao_Paulo`; `UPDATE` condicional idempotente; auditoria agregada (`Alteracao`, usuário nulo, quantidade); falha → log `Error` e nova tentativa a cada hora. Janela via `IParametros`.
7. **Autorização (TDD §13.2):** lista de trabalho e tentativas Adm/Rec; consulta Adm/Med/Rec; cancelar Adm/Med/Rec. DTOs de controle só com paciente, médico, datas e status (RNF-01). `controles_futuros` e `tentativas_contato` auditadas.
8. **Relógio de teste:** endpoint de avanço de relógio só em `Development` (TDD §17), inexistente em Produção (teste de ambiente).
9. **Frontend (DT-09, DT-10, DT-47):** `ListaTrabalhoStore` (filtros, itens, `vencidos` em `computed`), `ControlesApi`; vencidos com ícone Lucide e texto; seletor "prazo (N + unidade)" ou "data aproximada" no formulário de evolução de `prontuario`.

## Risks / Trade-offs

- [Handlers dentro da transação] → controle consistente com o turno; falha reverte a marcação (desejado).
- [Vínculo automático pode escolher o controle errado] → só o mais antigo do mesmo paciente e médico; a Recepção pode informar `controleId`.
- [Job parado perde o relógio] → roda na inicialização e reexecuta a cada hora em falha; idempotente.
- [Concorrência cancelar controle × marcar turno] → `xmin` devolve 409; o usuário repete.

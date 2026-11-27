## Why

O problema central do PRD é perder o acompanhamento de pacientes que precisam voltar quando a agenda ainda não está aberta. Este é o incremento **1d-ii**, que fecha a Fase 1 (TDD §22.3, tag `v1.0.0`): o médico indica o próximo controle ao registrar a evolução, o sistema acompanha o controle até o atendimento (*Pendente → Na janela → Contatado → Agendado → Realizado*) e a Recepção trabalha uma lista de contatos.

## What Changes

- **Criação:** API-16 (change `prontuario`) passa a aceitar `proximoControle` (prazo N semanas/meses ou data aproximada); o sistema calcula a data prevista, define o status inicial (*Pendente* ou *Na janela*) e, se na janela, devolve sugestão de horários próximos (RF-07 a RF-09).
- **Ciclo de vida:** máquina de estados do PRD §9.1, transições inválidas rejeitadas e cancelamento manual com motivo (API-21).
- **Vínculo com turnos:** API-30 ganha o campo opcional `controleId`; marcar turno leva o controle a *Agendado*, cancelar o turno o devolve a *Na janela*, atender o turno o leva a *Realizado* (RF-13, consumindo os eventos da change `agenda-turnos`).
- **JOB-01:** `AtualizacaoControlesJob` move *Pendente → Na janela* na inicialização e diariamente às 00:05 (America/Sao_Paulo), idempotente e auditado (RF-10, RN-02).
- **Recepção:** lista de trabalho com contatos e vencidos destacados (API-18; RF-11, RF-15), tentativas de contato (API-20; RF-12) e consulta geral com filtros (API-19; RF-14); `/app/inicio` da Recepção mostra a lista.
- **Ficha:** `proximoControle` da ficha do paciente (reservado em `pacientes`) passa a ser preenchido.
- **SPA:** `/app/controles`, `/app/controles/todos`, campo de próximo controle no formulário de evolução e início da Recepção.
- **Infra de teste:** endpoint de relógio só em `Development` e **E2E fluxo 1** (TDD §17).
- Migration aditiva com `controles_futuros` e `tentativas_contato`.

Fora desta change: indicadores (API-22, RF-35) e tela inicial do Administrador (Fase 3), avisos automáticos ao paciente (RF-38, v2), marcação pelo portal (Fase 2, que reutiliza o vínculo).

## Capabilities

### New Capabilities
- `controles-futuros`: criação a partir da evolução, sugestão de agendamento, máquina de estados, vínculo com turnos, cancelamento e consulta geral.
- `atualizacao-controles`: job diário e na inicialização que move controles para a janela.
- `lista-trabalho-contatos`: lista de trabalho da Recepção, tentativas de contato, vencidos e início da Recepção.

### Modified Capabilities
<!-- Nenhuma em delta: `proximoControle` em API-16/ficha e `controleId` em API-30 já são reservados por `prontuario`, `pacientes` e `agenda-turnos` e passam a ser exercidos pelos requisitos de `controles-futuros`; assim a change não depende de specs principais ainda não arquivadas. -->

## Impact

- Backend: agregado `ControleFuturo`, VO `PrazoControle`, handlers de eventos de turno, job no `BackgroundService` existente (CMP-09).
- Frontend: features `controles` e início da Recepção.
- Rastreabilidade: RF-07 a RF-15, RF-13, RN-02, RN-03, RNF-01, RNF-03; HU-01 a HU-03; DT-14, DT-15, DT-19, DT-23, DT-26; CMP-05, CMP-09; TDD §11.1, §11.2, §13.2, §15.
- Dependências: `prontuario` (evoluções), `agenda-turnos` (eventos e horários livres), `pacientes`, `cadastros-base`. Conclui a Fase 1.

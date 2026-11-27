# Tasks

> Origem: TDD-001 §22.3, incremento 1d-ii (Fase 1, MVP v1.0, tag `v1.0.0`). Branch sugerida: `feat/controles-futuros`. Pré-requisitos: `prontuario`, `agenda-turnos`, `pacientes` e `cadastros-base` integrados.

## 1. Domínio e persistência

- [ ] 1.1 Criar `PrazoControle` e o agregado `ControleFuturo` (`IAuditavel`) com data prevista, status inicial e máquina de estados; testes unitários com `FakeTimeProvider`: 2026-11-18 + 6 meses = 2027-05-18, + 3 semanas = 2026-12-09, dentro/fora da janela, todas as transições válidas e inválidas
- [ ] 1.2 Mapear as entidades no EF e criar a migration aditiva `controles_futuros` e `tentativas_contato` (FKs `restrict`, `CHECK`s, `xmin`, únicos de `turno_id` e `evolucao_origem_id`, índices `(status, data_prevista)`, `(medico_id, status)`, `(paciente_id)`, `(controle_id, realizada_em)`); teste de integração aplica em banco vazio

## 2. Criação e vínculo

- [ ] 2.1 Estender API-16 com `proximoControle` (criação na mesma transação da evolução) e a sugestão de até 5 horários via `CalculadoraHorarios`; testes: HU-01 (6 meses → `Pendente` em 2027-05-18; 1 mês → sugestão próxima de 2026-12-18), prazo e data juntos, quantidade fora de 1–60, sem horários livres
- [ ] 2.2 Implementar os handlers de `TurnoAgendado`, `TurnoCancelado` e `TurnoAtendido` e o `controleId` opcional em API-30; testes de integração: HU-03 completa (`Contatado → Agendado → Realizado`), turno cancelado volta a `NaJanela`, `controle.nao_vinculavel`, marcação sem controle elegível, falha do handler desfaz o turno
- [ ] 2.3 Implementar `CancelarControleCommand` (API-21) e `ListarControlesQuery` (API-19); testes: transições inválidas, motivo obrigatório, filtros e paginação, 403 por perfil
- [ ] 2.4 Preencher `proximoControle` na ficha (API-13) com o controle ativo mais próximo; teste de integração (Recepção vê data e status, sem conteúdo clínico)

## 3. Lista de trabalho e contatos (API-18, API-20)

- [ ] 3.1 Implementar `ListaTrabalhoQuery` (NaJanela/Contatado sem turno, ordenada, com contatos e `vencido`) e `RegistrarTentativaContatoCommand` (primeira tentativa → `Contatado`); testes: HU-02 (telefone, e-mail e familiar; "não atendeu" grava data/funcionário/resultado e mantém na lista; vencido destacado), canal/resultado inválidos, 403 para Médico

## 4. JOB-01 (CMP-09)

- [ ] 4.1 Implementar `AtualizacaoControlesJob` no `BackgroundService` existente (início + 00:05 `America/Sao_Paulo`, `UPDATE` condicional, auditoria agregada, nova tentativa a cada hora em falha); testes com `FakeTimeProvider`: 2027-05-18 vira `NaJanela` em 2027-03-18 e não em 2027-03-17, idempotência, execução na inicialização, janela alterada, falha transitória
- [ ] 4.2 Registrar o endpoint de avanço de relógio só em `Development` e testar que não existe nos demais ambientes

## 5. Frontend

- [ ] 5.1 Criar `ControlesApi` e `ListaTrabalhoStore` e as rotas lazy `/app/controles` e `/app/controles/todos` com guards por perfil; testes Vitest de guards e stores
- [ ] 5.2 Adicionar ao formulário de evolução (feature `prontuario`) o campo de próximo controle (prazo N + unidade ou data aproximada) e a exibição da sugestão de agendamento; testes Vitest
- [ ] 5.3 Implementar a lista de trabalho (contatos, vencidos com ícone Lucide e texto), o registro de tentativas e a consulta geral com filtros; testes Vitest incluindo estado vazio
- [ ] 5.4 Implementar o `/app/inicio` da Recepção (lista de trabalho) e exibir o próximo controle na ficha do paciente; testes Vitest
- [ ] 5.5 Verificar acessibilidade básica e navegação por teclado nas tabelas

## 6. E2E e rastreabilidade

- [ ] 6.1 Escrever o **E2E fluxo 1** com Playwright (1280 px): o médico registra a evolução com controle de 6 meses, o relógio avança para 2027-03-18, o controle aparece na lista da Recepção, a recepção registra contato, marca o turno (`Agendado`) e o médico registra o atendimento (`Realizado`); executar contra o docker-compose
- [ ] 6.2 Atualizar a matriz do TDD §21 (RF-07 a RF-15, RN-02, RN-03, HU-01 a HU-03) e documentar o contrato de `controleId`
- [ ] 6.3 Executar build, lint e todos os testes verdes no CI e conferir o critério de conclusão da Fase 1 (TDD §22.3): cenários Gherkin de HU-01 a HU-05 automatizados e fluxo do PRD §9.1 funcionando ponta a ponta

## Workflow follow-up

- Abrir PR com `.github/pull_request_template.md` a partir de `feat/controles-futuros` após build e testes verdes.
- Arquivar a change (`openspec-archive-change`) após o merge e criar a tag `v1.0.0`.

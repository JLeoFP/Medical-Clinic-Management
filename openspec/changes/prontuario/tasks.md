# Tasks

> Origem: TDD-001 §22.3, incremento 1d-i (Fase 1, MVP v1.0). Branch sugerida: `feat/prontuario`. Pré-requisitos: `cadastros-base`, `pacientes` e `agenda-turnos` integrados.

## 1. Domínio e persistência

- [ ] 1.1 Criar o agregado `Evolucao` (`IAuditavel`) e a regra de vínculo com turno (mesmo paciente e médico, `Agendado`, uma evolução por turno); testes unitários
- [ ] 1.2 Mapear `Evolucao` no EF e criar a migration aditiva `evolucoes` (FKs `restrict`, índice único de `turno_id`, índice `(paciente_id, data_atendimento)`); teste de integração aplica em banco vazio

## 2. API (API-15 a API-17)

- [ ] 2.1 Implementar `RegistrarEvolucaoCommand` (API-16) com `medicoId` da sessão e turno opcional marcando `Atendido` na mesma transação; testes de integração: evolução simples, vinculada a turno (e segunda evolução rejeitada), turno de outro paciente/médico, motivo vazio, Médico sem cadastro de médico
- [ ] 2.2 Implementar `ListarEvolucoesQuery` (API-15, `[AuditarLeitura]`) e `CorrigirEvolucaoCommand` (API-17); testes: ordem cronológica com autor (HU-04: Dr. B lê o que o Dr. A registrou), linha `Leitura` com `paciente_id`, correção auditada com antes/depois, ausência de rota de exclusão
- [ ] 2.3 Aplicar a policy `ConteudoClinico` e testar cada célula ❌ da matriz §13.2 (Administrador, Recepção, Paciente → 403); teste de que DTOs de ficha e demais módulos não expõem campos clínicos e de que o conteúdo da evolução não aparece nos logs

## 3. Frontend

- [ ] 3.1 Criar `ProntuarioApi`, `ProntuarioStore` e a rota lazy `/app/pacientes/:id/prontuario` com guard Médico; testes Vitest do guard (Recepção redirecionada sem chamar a API) e do store
- [ ] 3.2 Implementar a história clínica e o formulário de evolução em Signal Forms (motivo obrigatório, `turnoId` opcional); testes Vitest: ordem, formulário, erros por campo
- [ ] 3.3 Implementar `/app/inicio` do Médico (turnos do dia via API-29); teste Vitest
- [ ] 3.4 Verificar acessibilidade básica (rótulos, foco, `LiveAnnouncer`)

## 4. Rastreabilidade e verificação

- [ ] 4.1 Atualizar a matriz do TDD §21 (RF-04 a RF-06, RN-07, HU-04) e documentar o contrato de API-16 (`proximoControle` reservado)
- [ ] 4.2 Executar build, lint e todos os testes verdes no CI e conferir os cenários Gherkin da HU-04 (médico lê história; Recepção não vê evoluções)

## Workflow follow-up

- Abrir PR com `.github/pull_request_template.md` a partir de `feat/prontuario` após build e testes verdes.
- Arquivar a change (`openspec-archive-change`) após o merge.

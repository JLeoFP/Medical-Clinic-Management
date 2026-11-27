# Design

## Context

CMP-03 (TDD §8.3) e a policy `ConteudoClinico` definem o comportamento. Ao iniciar existem: pacientes (1b), turnos com `MarcarAtendido()` e despacho de eventos (1c), `AuditoriaLeituraBehavior` e `IAuditoria` (Fase 0).

## Goals / Non-Goals

**Goals:**
- Cenários Gherkin da HU-04 automatizados; conteúdo clínico visível só ao Médico, com leitura auditada.

**Non-Goals:**
- Controles futuros e tudo que dependa deles (change `controles-futuros`).

## Decisions

1. **`Evolucao` (DT-01, DT-15):** sem exclusão; correção (API-17) auditada com antes/depois. O `medico_id` vem do claim `medicoId` da sessão (Médico sem registro de médico → 422 `medico.nao_cadastrado`). `data_atendimento` = instante atual.
2. **Vínculo com turno:** se `turnoId` é informado, o turno deve ser do mesmo paciente e médico e estar `Agendado` (senão 422 `evolucao.turno_invalido`); `turno.MarcarAtendido()` roda na mesma transação e levanta `TurnoAtendido`. `turno_id` é único em `evolucoes`.
3. **Contrato de API-16 extensível:** o corpo aceita `proximoControle` opcional, ignorado/rejeitado até `controles-futuros` o implementar; a resposta já devolve `controle: null` e `sugestaoAgendamento: null`. Evita quebra de contrato depois.
4. **Autorização (TDD §13.2):** todos os endpoints com `ConteudoClinico`; testes cobrem cada célula ❌. DTOs de ficha e de outros módulos não carregam texto clínico — teste que falha se expuserem campos de `Evolucao` (RNF-01).
5. **Auditoria (DT-19):** `evolucoes` é `IAuditavel`; a query do prontuário é `[AuditarLeitura]` com `paciente_id`.
6. **Frontend (DT-09, DT-47):** `ProntuarioStore` (evoluções em signals) e `ProntuarioApi`; formulário em Signal Forms; início do Médico usa `GET /turnos` (1c) filtrado pelo médico da sessão e pela data de hoje.

## Risks / Trade-offs

- [Dados clínicos em logs] → logs não registram corpo (DT-28); teste de que a evolução não aparece nos logs.
- [Evolução registrada para turno já atendido] → unicidade de `turno_id` + validação de status `Agendado`.
- [Correção sobrescreve o histórico] → mitigado pela auditoria com antes/depois; sem versionamento próprio no MVP.

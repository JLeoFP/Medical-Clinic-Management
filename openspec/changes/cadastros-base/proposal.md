## Why

A Fase 0 entregou o esqueleto (login, sessão, auditoria, SPA base), mas ainda não há como configurar a clínica: sem usuários internos, médicos com agenda, tipos de atendimento e parâmetros, nenhuma funcionalidade seguinte (pacientes, turnos, controles) pode ser construída. Este é o incremento **1a** do roadmap da Fase 1 (TDD §22.3, MVP v1.0): os cadastros administrativos de que todos os demais incrementos dependem.

## What Changes

- Gestão de usuários internos pelo Administrador: listar, criar, editar e desativar, com perfis `Administrador`, `Medico` e `Recepcao` (API-07 a API-10; RF-36, RF-37). A desativação reutiliza a revogação imediata de sessões já existente na Fase 0.
- Cadastro de médicos vinculados a um usuário `Medico` e agenda semanal (dia, faixa e duração do turno), substituída integralmente pelo Administrador, com rejeição de faixas sobrepostas (API-23 a API-27; RF-16).
- Tipos de atendimento com tarifa vigente (API-37 em GET/POST/PUT; RF-20, parte cadastral de RF-25).
- Parâmetros da clínica (`janela_agenda_meses`, `desconto_dinheiro_percentual`, `antecedencia_cancelamento_horas`) com validação de faixa, cache invalidado na alteração e auditoria (API-45; RF-17, RF-23, RF-27; DT-16).
- Policies por perfil no `Api` (Administrador, Recepcao, Medico, Interno, ConteudoClinico) e guards equivalentes na SPA.
- SPA: telas `/app/admin/usuarios`, `/medicos`, `/tipos-atendimento` e `/parametros`, com a navegação do layout por perfil.
- Migration aditiva com `medicos`, `agendas_medico` e `tipos_atendimento`.

Fora desta change: pacientes (1b), turnos e horários livres (1c), prontuário e controles (1d), cobrança e cálculo de desconto (Fase 3 — aqui só se cadastram tarifa e percentual), e-mail de redefinição de senha (Fase 2).

## Capabilities

### New Capabilities
- `gestao-usuarios`: CRUD de usuários internos, perfis permitidos, desativação com revogação de sessões e proteção contra autodesativação.
- `medicos-agenda`: cadastro de médicos e agenda semanal com validação de faixas.
- `tipos-atendimento`: manutenção de tipos de atendimento e tarifa vigente.
- `parametros-clinica`: consulta e alteração dos parâmetros da clínica com validação, cache e auditoria.

### Modified Capabilities
<!-- Nenhuma: as capabilities da Fase 0 não mudam de requisito. -->

## Impact

- Backend: novos agregados em `Clinica.Domain` (`Medico`, `AgendaSemanal`, `TipoAtendimento`), commands/queries em `Clinica.Application`, mapeamento EF e migration em `Clinica.Infrastructure`, endpoints e policies em `Clinica.Api`.
- Frontend: nova feature `admin` com lazy routes e guard por perfil; reuso de `shared/ui` e `shared/validacao`.
- Rastreabilidade: RF-16, RF-17, RF-20, RF-25 (cadastral), RF-27 (cadastral), RF-36, RF-37, RNF-01, RNF-03; DT-02, DT-14, DT-15, DT-16, DT-19, DT-25, DT-37, DT-39, DT-45, DT-46, DT-47; TDD §13.2.
- Dependência: Fase 0 (`fundacao-plataforma`, arquivada). Habilita 1b, 1c e 1d.

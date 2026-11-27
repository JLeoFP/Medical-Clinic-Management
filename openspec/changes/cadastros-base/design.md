# Design

## Context

A Fase 0 deixou pronto o pipeline MediatR (Logging → Validation → Transaction), ProblemDetails com `code`, Identity com sessão no servidor, `IAuditavel` + `AuditoriaInterceptor`, a entidade `Parametro` (já semeada) e a policy `Interno`/`ConteudoClinico` em `ConfiguracaoApi`. Esta change só acrescenta cadastros sobre essa base; nenhuma decisão do TDD é reaberta (CMP-01, CMP-04 médicos, CMP-08; DT-14, DT-15, DT-16).

## Goals / Non-Goals

**Goals:**
- Administrador configura usuários, médicos, agendas, tipos de atendimento e parâmetros pela SPA.
- Matriz de autorização do TDD §13.2 aplicada e testada para estes recursos.
- Parâmetros lidos por uma porta com cache (DT-16) que as próximas changes reutilizam.

**Non-Goals:**
- Geração de slots e marcação de turnos (1c); aqui só se valida e persiste a grade.
- Cálculo de cobrança e desconto (Fase 3); troca/recuperação de senha por e-mail (Fase 2).

## Decisions

1. **Agregados e persistência (DT-01, DT-12, DT-13):** `Medico` (raiz, com coleção `AgendaSemanal` de faixas) e `TipoAtendimento` em `Domain`; `usuario_id` do médico referencia `identidade.usuarios` só por Guid (Domain não conhece Identity). Tabelas conforme TDD §9.2; `xmin` como token de concorrência em `medicos` e `tipos_atendimento` (DT-14); `ativo` em vez de exclusão (DT-15).
2. **Usuários internos:** o handler fala com a porta `IGestaoAcesso` (Application) implementada com `UserManager`/`RoleManager` em `Infrastructure`; reutiliza o serviço de revogação de sessões da Fase 0 na desativação. Perfil `Paciente` é rejeitado nesta API (contas de paciente nascem no portal, Fase 2). Senha inicial segue a política DT-18.
3. **Autodesativação (DT-48):** o Administrador não pode desativar a si mesmo (422 `usuario.autodesativacao`), evitando perder o último acesso administrativo — suposição de segurança, não exigida pelo PRD.
4. **Agenda substituída por inteiro (API-27):** o domínio valida `hora_fim > hora_inicio`, duração 5–240 min e ausência de sobreposição na mesma `dia_semana`; a faixa não precisa ser múltipla da duração (o último slot é o que cabe inteiro — consumido em 1c). Substituição em uma transação (remove + insere).
5. **Parâmetros (DT-16):** porta `IParametros` com `IMemoryCache`, invalidada no `PUT`; valores tipados com faixas 1–12, 0–100 e 0–168. Alterações auditadas pelo `AuditoriaInterceptor` (já registrado para `parametros`); `medicos` e `tipos_atendimento` passam a implementar `IAuditavel`.
6. **Autorização (TDD §13.2):** policies nomeadas `Administrador`, `Recepcao`, `Medico`, `Interno` e `ConteudoClinico` em `ConfiguracaoApi.Policies`; endpoints declaram a policy do TDD §10.2. Na SPA, `canMatch` por perfil é só UX.
7. **Frontend (DT-09, DT-10, DT-46, DT-47):** feature `features/admin` com um store de signals por tela, `*Api` tipada sobre `HttpClient`, formulários em Signal Forms, tabelas de `shared/ui`. Sem biblioteca nova. Editor de agenda: uma linha por faixa (dia, início, fim, duração) com validação local espelhando a do domínio.
8. **Sem seed de tipos de atendimento:** o TDD não define tarifas; o Administrador cria os tipos (Consulta, Controle…) e os testes usam fábricas. O seed de demonstração fica para a Fase 4.

## Risks / Trade-offs

- [Agenda substituída por inteiro pode apagar faixas já usadas por turnos futuros] → turnos guardam `inicio`/`fim` próprios (não dependem da grade); a regra de aviso ao alterar agenda com turnos existentes fica registrada como risco aceito para o MVP e revisitada em 1c.
- [Desativar médico com turnos futuros] → `ativo=false` só impede novas marcações (verificado em 1c); turnos existentes permanecem.
- [Duas edições simultâneas do mesmo médico/tipo] → `xmin` devolve 409; a SPA mostra a mensagem e recarrega.
- [Sem troca obrigatória de senha no primeiro acesso de usuários criados] → não exigida pelos RF; senha inicial é definida pelo Administrador (documentado em suposições).

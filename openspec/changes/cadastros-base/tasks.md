# Tasks

> Origem: TDD-001 §22.3, incremento 1a (Fase 1, MVP v1.0). Cada grupo inclui seus testes e a documentação (`<summary>`) dos tipos públicos. Branch sugerida: `feat/cadastros-base`.

## 1. Autorização e base de cadastros

- [ ] 1.1 Ampliar `ConfiguracaoApi.Policies` com `Administrador`, `Recepcao` e `Medico` (mantendo `Interno` e `ConteudoClinico`); teste de integração com uma rota de teste por policy e por perfil
- [ ] 1.2 Criar a migration aditiva `medicos`, `agendas_medico` e `tipos_atendimento` (TDD §9.2, `xmin`, índices e `CHECK`s); teste aplica em banco vazio e confirma os índices únicos

## 2. Usuários internos (API-07 a API-10)

- [ ] 2.1 Criar a porta `IGestaoAcesso` e a implementação com `UserManager`/`RoleManager` (listar paginado, criar, editar, desativar reutilizando a revogação de sessões); testes unitários dos handlers e validadores
- [ ] 2.2 Implementar os endpoints com policy `Administrador`, `usuario.email_duplicado` e `usuario.autodesativacao`; testes de integração: criação, e-mail duplicado, senha fraca, perfil Paciente recusado, desativado não autentica, sessão aberta revogada, 403 para Recepção e Médico

## 3. Médicos e agenda (API-23 a API-27)

- [ ] 3.1 Criar `Medico` e `AgendaSemanal` no `Domain` com validação de faixa, duração e sobreposição; testes unitários cobrindo sobreposição, limites 5/240 e fim ≤ início
- [ ] 3.2 Implementar mapeamento EF, handlers e endpoints (lista ativos para autenticados, escrita só Administrador, agenda legível por Administrador e Recepção); testes de integração: registro duplicado, usuário sem perfil Médico, substituição integral da agenda, faixas sobrepostas preservam a agenda anterior, concorrência 409

## 4. Tipos de atendimento (API-37)

- [ ] 4.1 Criar `TipoAtendimento` (`IAuditavel`), handlers e endpoints (GET Interno; POST/PUT Administrador); testes: nome duplicado, tarifa negativa, alteração de tarifa auditada, 403 de escrita para Recepção/Médico, inativo fora das listagens para novos turnos

## 5. Parâmetros (API-45)

- [ ] 5.1 Implementar a porta `IParametros` com `IMemoryCache` e invalidação, e os endpoints GET (Interno) e PUT (Administrador) com validação de faixas; testes: padrões, faixas inválidas, cache invalidado após o PUT, auditoria com antes/depois, 403 para outros perfis

## 6. Frontend administrativo

- [ ] 6.1 Criar a feature `admin` com rotas lazy `/app/admin/usuarios`, `/medicos`, `/tipos-atendimento`, `/parametros`, guard `canMatch` por perfil Administrador e itens de navegação por perfil no layout; teste Vitest dos guards
- [ ] 6.2 Implementar a tela de usuários (lista, criação, edição, desativação com confirmação em `DialogoService`) com Signal Forms e `UsuariosApi`/store; testes Vitest do store e de erros 409/400 mapeados aos campos
- [ ] 6.3 Implementar as telas de médicos (cadastro + editor de agenda semanal com validação local), tipos de atendimento e parâmetros; testes Vitest do editor de agenda (sobreposição, duração) e dos formulários
- [ ] 6.4 Verificar acessibilidade básica das telas (rótulos, foco no diálogo, mensagens por `LiveAnnouncer`) com testes Vitest

## 7. Rastreabilidade e verificação

- [ ] 7.1 Atualizar a matriz do TDD §21 (RF-16, RF-17, RF-20, RF-36, RF-37, RNF-01, RNF-03) e os READMEs afetados
- [ ] 7.2 Executar build, lint e todos os testes (backend e frontend) verdes no CI e conferir o critério do incremento 1a: matriz §13.2 dos recursos do incremento, usuário desativado não autentica e faixas sobrepostas rejeitadas

## Workflow follow-up

- Abrir PR com `.github/pull_request_template.md` a partir de `feat/cadastros-base` após build e testes verdes.
- Arquivar a change (`openspec-archive-change`) após o merge.

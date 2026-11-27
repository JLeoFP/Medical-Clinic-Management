## Why

Turnos, evoluções e controles dependem de um cadastro confiável de pacientes. Este é o incremento **1b** da Fase 1 (TDD §22.3): permite à Recepção cadastrar, editar, buscar e consultar a ficha do paciente, com unicidade de CPF e e-mail e idade calculada, sem expor conteúdo clínico.

## What Changes

- Cadastro e edição de pacientes (API-12, API-14) com os dados do RF-01, value objects `Cpf` e `Email`, data de nascimento não futura e `origem_cadastro = Recepcao`.
- Bloqueio de CPF e e-mail duplicados com mensagem que indica o campo e, para CPF, o paciente existente; telefone pode repetir (RF-02, RN-09).
- Busca por nome, sobrenome ou CPF, paginada (API-11; RF-03).
- Ficha do paciente com dados pessoais e idade calculada, sem campos clínicos; o campo `proximoControle` já existe no contrato, retornando `null` até a change 1d (API-13; RF-02a, RN-12). A leitura da ficha é auditada (`[AuditarLeitura]`).
- SPA: `/app/pacientes` (busca), `/app/pacientes/novo` e `/app/pacientes/:id` (ficha e edição), reutilizando pipes e validadores da Fase 0.
- Migration aditiva com `pacientes` e o índice de busca por prefixo.

Fora desta change: sincronização da conta do portal ao alterar o e-mail e autocadastro (Fase 2, RN-16/RN-17), próximo controle real na ficha (1d), prontuário (1d).

## Capabilities

### New Capabilities
- `pacientes`: cadastro, edição, busca e ficha do paciente, com unicidade de CPF/e-mail, idade derivada e auditoria de leitura.

### Modified Capabilities
<!-- Nenhuma. -->

## Impact

- Backend: agregado `Paciente`, value objects `Cpf`/`Email`, handlers e endpoints; entidade passa a ser auditada (`IAuditavel`).
- Frontend: feature `pacientes` com três telas.
- Rastreabilidade: RF-01, RF-02, RF-02a, RF-03, RN-09, RN-10, RN-12, RNF-01 (sem conteúdo clínico), RNF-03; HU-05; DT-14, DT-15, DT-19, DT-25, DT-26, DT-39, DT-47; CMP-02.
- Dependência: `cadastros-base` (policies por perfil). Habilita 1c e 1d.

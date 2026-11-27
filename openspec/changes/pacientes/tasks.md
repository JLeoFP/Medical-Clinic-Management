# Tasks

> Origem: TDD-001 §22.3, incremento 1b (Fase 1, MVP v1.0). Branch sugerida: `feat/pacientes`. Cada grupo inclui seus testes e `<summary>` nos tipos públicos.

## 1. Domínio

- [ ] 1.1 Criar os value objects `Cpf` e `Email` em `Clinica.Domain`; testes unitários: CPFs válidos/ inválidos (todos dígitos iguais, dígitos verificadores), com e sem máscara, e-mail normalizado em minúsculas
- [ ] 1.2 Criar o agregado `Paciente` (`IAuditavel`) com `IdadeEm(DateOnly)` e validação de data de nascimento não futura; testes com `FakeTimeProvider`: 1990-05-10 em 2026-11-24 → 36, dia do aniversário e 29/02

## 2. Persistência

- [ ] 2.1 Mapear `Paciente` no EF (snake_case, `xmin`, `CHECK` de sexo e origem) e criar a migration aditiva `pacientes` com índices únicos de `cpf` e `email` e `ix_pacientes_nome` (`lower(nome)`, `lower(sobrenome)`, `varchar_pattern_ops`); teste de integração: índices únicos rejeitam duplicatas e a busca por prefixo usa o índice (plano de execução)

## 3. API de pacientes

- [ ] 3.1 Implementar `CadastrarPacienteCommand` e `EditarPacienteCommand` (API-12, API-14) com validadores (FluentValidation), checagem de duplicidade com `paciente.cpf_duplicado`/`paciente.email_duplicado`, tradução da violação de índice único para o mesmo 409 e policies `Administrador`/`Recepcao`; testes de integração: cenários Gherkin da HU-05 (CPF duplicado mostra o existente, e-mail duplicado, telefone compartilhado), cadastro concorrente (1×201 + 1×409), edição concorrente 409, Médico recebe 403
- [ ] 3.2 Implementar `BuscarPacientesQuery` (API-11) com prefixo de nome/sobrenome e CPF, paginação e ordenação; testes: busca por nome sem diferenciar caixa, por CPF com e sem máscara, sem resultados, página máxima 100
- [ ] 3.3 Implementar `ObterFichaPacienteQuery` (API-13) com `idade` e `proximoControle: null`, marcada `[AuditarLeitura]`; testes: idade 36 (HU-05), DTO sem campos clínicos, linha `Leitura` com `paciente_id`, 404 e 403 para o perfil Paciente
- [ ] 3.4 Verificar que os logs não registram CPF, e-mail nem termos de busca (DT-28); teste do enriquecimento do Serilog

## 4. Frontend

- [ ] 4.1 Criar a feature `pacientes` com `PacientesApi`, `PacientesStore` (busca e paginação com signals) e rotas lazy `/app/pacientes`, `/app/pacientes/novo`, `/app/pacientes/:id` com guard por perfil; teste Vitest dos guards e do store
- [ ] 4.2 Implementar a tela de busca (campo único, tabela com `shared/ui` e paginação, CPF e telefone formatados pelos pipes) com teste Vitest da busca e do estado vazio
- [ ] 4.3 Implementar o formulário de cadastro/edição em Signal Forms (validadores `cpfValido`, `telefoneValido`, `dataNaoFutura`; erros 409 ligados ao campo) e a ficha com idade (`calcularIdade`); testes Vitest: validação por campo, mensagem de duplicidade com link para o paciente existente, ficha sem seção clínica
- [ ] 4.4 Verificar acessibilidade básica (rótulos, foco no primeiro erro, `LiveAnnouncer`) e responsividade em tablet

## 5. Rastreabilidade e verificação

- [ ] 5.1 Atualizar a matriz do TDD §21 (RF-01 a RF-03, RF-02a, RN-09, RN-12, HU-05) e documentar o contrato `proximoControle` para a change 1d
- [ ] 5.2 Executar build, lint e todos os testes verdes no CI e conferir o critério do incremento 1b: cenários Gherkin da HU-05 automatizados (CPF e e-mail duplicados, telefone compartilhado, idade 36)

## Workflow follow-up

- Abrir PR com `.github/pull_request_template.md` a partir de `feat/pacientes` após build e testes verdes.
- Arquivar a change (`openspec-archive-change`) após o merge.

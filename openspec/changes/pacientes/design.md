# Design

## Context

CMP-02 (TDD §8.2) define as regras do paciente. A Fase 0 já entrega os pipes `cpf`, `telefone`, `idade` e os validadores `cpfValido`, `telefoneValido`, `dataNaoFutura` no frontend, `RelogioClinica` e o `AuditoriaLeituraBehavior`. As policies por perfil chegam com `cadastros-base`.

## Goals / Non-Goals

**Goals:**
- Cenários Gherkin da HU-05 automatizados (CPF e e-mail duplicados, telefone compartilhado, idade 36).
- Ficha sem nenhum dado clínico (RNF-01) e com leitura auditada.

**Non-Goals:**
- Conta do portal, vínculo por e-mail e autocadastro (Fase 2).
- Exibição real do próximo controle (1d).

## Decisions

1. **Value objects (DT-01):** `Cpf` (11 dígitos, dígitos verificadores, armazenado sem máscara) e `Email` (trim + minúsculas, formato básico) em `Domain`; `Paciente.IdadeEm(DateOnly hoje)` calcula a idade sem persistir (RN-12); `DataNascimento` futura é rejeitada na criação e edição, comparando com a data local de `RelogioClinica`.
2. **Unicidade em duas camadas (DT-14):** o handler consulta CPF/e-mail para devolver mensagem útil (`paciente.cpf_duplicado` com id e nome do paciente existente; `paciente.email_duplicado`), e os índices únicos de `pacientes` garantem a regra sob concorrência — violação do índice é traduzida para o mesmo 409. `xmin` protege edições concorrentes.
3. **Busca (RF-03):** termo só com dígitos (≥ 3) → CPF (prefixo); caso contrário, prefixo case-insensitive em `lower(nome)`/`lower(sobrenome)` aproveitando o índice `varchar_pattern_ops` (TDD §9.2); termo vazio lista tudo paginado por nome. Máximo 100 por página (DT-39). Mínimo de 2 caracteres para termos textuais — suposição para evitar varredura ampla.
4. **Contrato da ficha:** `PacienteFichaDto` com dados pessoais, `idade`, `origemCadastro` e `proximoControle: null` (campo reservado; 1d o preenche sem quebrar o contrato). Nenhum DTO deste módulo contém campos clínicos.
5. **Auditoria (DT-19):** `Paciente` implementa `IAuditavel`; a query da ficha é marcada `[AuditarLeitura]` com `paciente_id`.
6. **API-14 sem portal:** a edição valida unicidade de e-mail contra outros pacientes; a sincronização de conta do portal (RN-17) é adicionada na Fase 2 no mesmo handler.
7. **Frontend (DT-09, DT-47):** `PacientesApi`, `PacientesStore` (filtro e paginação em signals, `httpResource` para a busca), formulário em Signal Forms com os validadores existentes; erros 409 ligados ao campo CPF/e-mail. CPF e telefone trafegam só com dígitos e são formatados pelos pipes.

## Risks / Trade-offs

- [Busca por prefixo não encontra "contém"] → aceito pelo RF-03 (nome, sobrenome ou CPF); documentado na tela.
- [Revelar o paciente existente no CPF duplicado] → exigido pela HU-05 para a Recepção; no portal (Fase 2) a mensagem será genérica (RN-16).
- [CPF no log/URL] → busca por CPF vai em query string; logs não registram query de busca com dados pessoais (DT-28) — verificar o enriquecimento do Serilog.
- [Dados pessoais em testes] → usar CPFs gerados válidos e fictícios.

## ADDED Requirements

### Requirement: Esquema inicial do banco
As migrations SHALL criar os schemas `clinica` e `identidade` com as tabelas de Identity renomeadas (`usuarios`, `perfis`, `usuarios_perfis` etc.), `sessoes`, `chaves_protecao_dados`, `parametros` e `auditoria`, com chaves UUID, instantes em `timestamptz` e enums como `varchar` com `CHECK`. Origem: TDD §9.2, DT-11 a DT-13. A API MUST NOT aplicar migrations ao iniciar.

#### Scenario: Banco vazio
- **WHEN** as migrations são aplicadas em um PostgreSQL 17 vazio
- **THEN** todas as tabelas e índices da Fase 0 existem e a API inicia sem alterar o esquema

### Requirement: Perfis e parâmetros padrão
O sistema SHALL semear os perfis `Administrador`, `Medico`, `Recepcao` e `Paciente` e os parâmetros `janela_agenda_meses = 2`, `desconto_dinheiro_percentual = 10` e `antecedencia_cancelamento_horas = 24`. Origem: RF-36, TDD §9.3.

#### Scenario: Semeadura
- **WHEN** as migrations terminam
- **THEN** os 4 perfis e os 3 parâmetros existem com os valores padrão

### Requirement: Administrador inicial idempotente
Na inicialização, o sistema SHALL criar o administrador a partir de `ADMIN_EMAIL` e `ADMIN_SENHA_INICIAL`, com e-mail confirmado e troca de senha obrigatória no primeiro acesso, sem duplicá-lo em reinícios. Origem: DT-20, TDD §9.3.

#### Scenario: Primeira inicialização
- **WHEN** a API inicia sem nenhum administrador e com as variáveis definidas
- **THEN** o administrador é criado

#### Scenario: Reinício
- **WHEN** a API reinicia com o administrador já existente
- **THEN** nenhum usuário é criado nem alterado

#### Scenario: Variáveis ausentes
- **WHEN** as variáveis não estão definidas e não há administrador
- **THEN** a API registra um aviso em log e continua sem criar o usuário

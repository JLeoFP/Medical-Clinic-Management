## ADDED Requirements

### Requirement: Cadastro de paciente
O sistema SHALL permitir a Administrador e Recepção cadastrar um paciente com nome, sobrenome, sexo (Homem ou Mulher), data de nascimento, CPF, e-mail, telefone, telefone de familiar (opcional), obra social (opcional), bairro, logradouro, número e complemento (opcional), registrando a origem `Recepcao` (API-12). CPF MUST ter 11 dígitos válidos e ser armazenado sem máscara; e-mail MUST ser armazenado em minúsculas. Origem: RF-01, RN-09, RN-10.

#### Scenario: Cadastro válido
- **WHEN** a Recepção cadastra uma paciente com dados válidos
- **THEN** a resposta é 201 e a paciente é gravada com CPF só com dígitos e e-mail em minúsculas

#### Scenario: CPF inválido
- **WHEN** o CPF informado não tem dígitos verificadores válidos
- **THEN** a resposta é 400 com erro no campo `cpf`

#### Scenario: Data de nascimento futura
- **WHEN** a data de nascimento é posterior à data atual
- **THEN** a resposta é 400 com erro no campo `dataNascimento`

#### Scenario: Médico não cadastra
- **WHEN** um usuário Médico tenta cadastrar ou editar um paciente
- **THEN** a resposta é 403

### Requirement: Unicidade de CPF e e-mail
O sistema SHALL impedir o cadastro e a edição de um paciente com CPF ou e-mail já registrados para outro paciente, informando qual dado está duplicado e, para CPF, o paciente existente; o telefone MAY repetir entre pacientes. A regra MUST valer também sob requisições concorrentes. Origem: RF-02, RN-09, HU-05.

#### Scenario: CPF duplicado
- **WHEN** existe um paciente com o CPF 123.456.789-09 e outro cadastro usa o mesmo CPF
- **THEN** a resposta é 409 com `code` `paciente.cpf_duplicado` e os dados de identificação do paciente existente

#### Scenario: E-mail duplicado
- **WHEN** existe um paciente com o e-mail "ana@exemplo.com" e outro cadastro usa o mesmo e-mail (inclusive com outra caixa de letras)
- **THEN** a resposta é 409 com `code` `paciente.email_duplicado`

#### Scenario: Telefone compartilhado por familiares
- **WHEN** outro paciente é cadastrado com o telefone "(11) 98765-4321" já usado por um paciente
- **THEN** o cadastro é aceito

#### Scenario: Cadastros concorrentes com o mesmo CPF
- **WHEN** dois cadastros com o mesmo CPF chegam ao mesmo tempo
- **THEN** exatamente um é criado e o outro recebe 409

#### Scenario: Edição com CPF de outro paciente
- **WHEN** a Recepção edita um paciente informando o CPF de outro
- **THEN** a resposta é 409 com `code` `paciente.cpf_duplicado`

### Requirement: Edição de paciente
O sistema SHALL permitir a Administrador e Recepção editar os dados do paciente (API-14), protegendo contra edições concorrentes, e MUST NOT permitir exclusão física. Origem: RF-01, DT-14, DT-15.

#### Scenario: Edição válida
- **WHEN** a Recepção altera o telefone de um paciente
- **THEN** a resposta é 200 e a alteração é registrada na auditoria com valores anterior e novo

#### Scenario: Edição concorrente
- **WHEN** duas edições usam a mesma versão do registro
- **THEN** a segunda recebe 409 de concorrência

### Requirement: Busca de pacientes
O sistema SHALL permitir a Administrador, Médico e Recepção buscar pacientes por prefixo de nome ou sobrenome (sem diferenciar maiúsculas) ou por CPF, com resultado paginado e ordenado por nome (API-11). Origem: RF-03, DT-39.

#### Scenario: Busca por nome
- **WHEN** a Recepção busca por "ana"
- **THEN** recebe os pacientes cujo nome ou sobrenome começa com "ana"

#### Scenario: Busca por CPF
- **WHEN** a Recepção busca pelo CPF completo, com ou sem máscara
- **THEN** recebe o paciente correspondente

#### Scenario: Sem resultados
- **WHEN** nenhum paciente corresponde à busca
- **THEN** a resposta é 200 com `items` vazio e `totalItems` 0

### Requirement: Ficha do paciente com idade
O sistema SHALL exibir, a Administrador, Médico e Recepção, a ficha com os dados pessoais, a idade calculada a partir da data de nascimento e o campo `proximoControle`, sem nenhum conteúdo clínico, e MUST auditar a leitura (API-13). A idade MUST NOT ser persistida. Origem: RF-02a, RN-12, RNF-01, RNF-03, HU-04, HU-05.

#### Scenario: Idade calculada
- **WHEN** a ficha de uma paciente nascida em 1990-05-10 é aberta em 2026-11-24
- **THEN** a idade exibida é 36 anos

#### Scenario: Idade no dia do aniversário
- **WHEN** a ficha é aberta exatamente no aniversário
- **THEN** a idade já considera o novo ano completo

#### Scenario: Recepção não vê conteúdo clínico
- **WHEN** a Recepção abre a ficha
- **THEN** a resposta contém os dados pessoais e `proximoControle`, e nenhum campo de evolução clínica

#### Scenario: Leitura auditada
- **WHEN** a ficha é consultada
- **THEN** existe um registro de auditoria `Leitura` com o usuário e o `paciente_id`

#### Scenario: Paciente inexistente
- **WHEN** o id não corresponde a nenhum paciente
- **THEN** a resposta é 404

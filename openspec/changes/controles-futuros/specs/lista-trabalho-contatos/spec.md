## ADDED Requirements

### Requirement: Lista de trabalho da Recepção
O sistema SHALL exibir a Administrador e Recepção os controles `NaJanela` e `Contatado` sem turno, ordenados por data prevista, com nome do paciente, médico, data prevista, telefone, e-mail e telefone do familiar (API-18), sem conteúdo clínico. Origem: RF-11, RN-02, RN-03, HU-02.

#### Scenario: Controle entra na lista
- **WHEN** um controle passa a `NaJanela`
- **THEN** ele aparece na lista com telefone, e-mail e contato de familiar do paciente

#### Scenario: Controle agendado sai da lista
- **WHEN** o controle passa a `Agendado`
- **THEN** ele deixa de aparecer na lista de trabalho

#### Scenario: Médico não acessa a lista
- **WHEN** um usuário Médico chama a lista de trabalho
- **THEN** a resposta é 403

### Requirement: Destaque de controles vencidos
O sistema SHALL marcar como vencido todo controle da lista cuja data prevista seja anterior à data atual, e a interface MUST destacá-lo com ícone e texto, não apenas cor. Origem: RF-15, RNF-09.

#### Scenario: Controle vencido
- **WHEN** um controle `NaJanela` sem turno tem data prevista anterior a hoje
- **THEN** o item vem com `vencido` verdadeiro e aparece destacado na lista

#### Scenario: Controle no prazo
- **WHEN** a data prevista é hoje ou futura
- **THEN** `vencido` é falso

### Requirement: Registro de tentativas de contato
O sistema SHALL permitir a Administrador e Recepção registrar uma tentativa de contato em controle `NaJanela` ou `Contatado` com canal (Telefone, WhatsApp, Email ou Presencial), resultado (Agendou, NaoAtendeu, RetornarDepois, NumeroInvalido ou Recusou) e observação opcional, gravando data e funcionário da sessão (API-20). Origem: RF-12, HU-02.

#### Scenario: Tentativa sem resposta
- **WHEN** a Recepção registra uma ligação com resultado "não atendeu"
- **THEN** a tentativa fica gravada com data, funcionário e resultado, e o controle continua na lista (como `Contatado`)

#### Scenario: Controle sem janela
- **WHEN** se tenta registrar contato em controle `Pendente`, `Agendado`, `Realizado` ou `Cancelado`
- **THEN** a resposta é 422 com `code` `controle.transicao_invalida`

#### Scenario: Canal ou resultado inválido
- **WHEN** o canal ou o resultado não pertence ao conjunto permitido
- **THEN** a resposta é 400 com erro no campo correspondente

#### Scenario: Histórico de tentativas
- **WHEN** a Recepção abre um controle com várias tentativas
- **THEN** vê todas, da mais antiga à mais recente

### Requirement: Telas da Recepção para controles
O sistema SHALL abrir `/app/inicio` da Recepção com a lista de trabalho e oferecer as rotas `/app/controles` e `/app/controles/todos` conforme a matriz de perfis. Origem: RNF-04, TDD §12.1.

#### Scenario: Recepção abre o início
- **WHEN** um usuário Recepção entra em `/app/inicio`
- **THEN** vê a lista de trabalho com vencidos destacados

#### Scenario: Rota sem permissão
- **WHEN** um usuário Médico navega para `/app/controles`
- **THEN** é redirecionado e nenhuma lista de trabalho é solicitada à API

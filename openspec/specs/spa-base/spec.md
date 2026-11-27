# spa-base Specification

## Purpose
Definir o shell da SPA, a restauração de sessão ao recarregar, a tela de entrada, o tratamento de 401 e de erros e o locale pt-BR.

## Requirements

### Requirement: Tela de login interno
A SPA SHALL oferecer `/entrar` com e-mail e senha, mensagens de erro em pt-BR e redirecionamento para `/app/inicio` (ou para o `returnUrl`) após o login. Origem: RF-36, DT-47.

#### Scenario: Login do administrador semeado
- **WHEN** o administrador informa credenciais válidas em `/entrar`
- **THEN** a SPA navega para `/app/inicio`

#### Scenario: Credenciais inválidas
- **WHEN** o login é rejeitado com 401
- **THEN** a SPA mostra mensagem em pt-BR sem limpar o e-mail digitado

### Requirement: Sessão sobrevive ao recarregar
A SPA SHALL chamar API-02 na inicialização, antes de ativar as rotas, e restaurar o usuário no `AuthStore`; nenhum token fica acessível ao JavaScript. Origem: DT-45, RNF-02.

#### Scenario: Recarga com sessão ativa
- **WHEN** a página é recarregada com sessão válida
- **THEN** o usuário continua em `/app` sem novo login

#### Scenario: Recarga sem sessão
- **WHEN** a página `/app/inicio` é aberta sem sessão
- **THEN** a SPA redireciona para `/entrar` com `returnUrl`

### Requirement: Tratamento de 401 e erros da API
A SPA SHALL, ao receber 401 em uma chamada, limpar o `AuthStore` e levar ao login com `returnUrl`; e SHALL converter ProblemDetails em mensagens pt-BR, devolvendo erros de validação aos campos do formulário. A SPA SHALL enviar `X-XSRF-TOKEN` e `X-Correlation-Id` nas chamadas à API.

#### Scenario: Sessão expirada
- **WHEN** uma chamada retorna 401 com a SPA aberta
- **THEN** o usuário é levado a `/entrar`

#### Scenario: Erro de validação
- **WHEN** a API retorna 400 com `errors` por campo
- **THEN** cada mensagem aparece junto ao campo correspondente

### Requirement: Layout e locale pt-BR
A área `/app` SHALL ter layout base carregado sob demanda e acessível (foco visível, contraste AA, navegação por teclado), e a SPA SHALL formatar datas `dd/MM/yyyy` e valores `R$ 0.000,00`. Origem: RNF-09, DT-26, DT-46.

#### Scenario: Formatação
- **WHEN** a SPA exibe a data 2026-11-18 e o valor 1234.5
- **THEN** mostra `18/11/2026` e `R$ 1.234,50`

### Requirement: Validadores de formulário compartilhados
A SPA SHALL fornecer validadores reutilizáveis de CPF, telefone e data não futura para os formulários das fases seguintes. Origem: DT-47.

#### Scenario: CPF inválido
- **WHEN** um CPF com dígito verificador incorreto é validado
- **THEN** o validador devolve erro

## ADDED Requirements

### Requirement: Entrada automática na janela (JOB-01)
O sistema SHALL mover controles `Pendente` para `NaJanela` quando a data prevista ficar menor ou igual a hoje mais a janela de agenda, executando na inicialização da API e diariamente às 00:05 (America/Sao_Paulo), de forma idempotente. Origem: RF-10, RN-02, DT-23.

#### Scenario: Controle entra na janela
- **WHEN** existe um controle `Pendente` com data prevista 2027-05-18, janela de 2 meses, e a data atual chega a 2027-03-18
- **THEN** na execução do job o controle passa a `NaJanela` e aparece na lista de trabalho

#### Scenario: Controle ainda fora da janela
- **WHEN** a data atual é 2027-03-17
- **THEN** o controle permanece `Pendente`

#### Scenario: Idempotência
- **WHEN** o job é executado duas vezes no mesmo dia
- **THEN** a segunda execução não altera nenhum controle

#### Scenario: Execução na inicialização
- **WHEN** a API reinicia depois de dias parada
- **THEN** a execução inicial atualiza todos os controles que já deveriam estar na janela

#### Scenario: Janela alterada
- **WHEN** o Administrador aumenta a janela e o job roda
- **THEN** os controles passam a ser avaliados com o novo valor

### Requirement: Auditoria e tolerância a falhas do job
O sistema SHALL registrar na auditoria, sem usuário, uma alteração agregada com a quantidade de controles movidos, e em caso de falha registrar `Error` e tentar novamente a cada hora até o sucesso, sem derrubar a API. Origem: CMP-09, DT-23, §15.

#### Scenario: Auditoria agregada
- **WHEN** o job move 3 controles
- **THEN** existe um registro `Alteracao` com usuário nulo e a quantidade 3

#### Scenario: Falha transitória
- **WHEN** o banco está indisponível durante o job
- **THEN** a falha é registrada, a API continua de pé e o job tenta novamente na hora seguinte

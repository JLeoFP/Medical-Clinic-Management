## ADDED Requirements

### Requirement: Ambiente local por docker-compose
O repositório SHALL fornecer `docker-compose.yml` que sobe `postgres:17`, `mailpit`, `seq`, `api` e `web` com healthchecks, sem versionar segredos. Origem: DT-34, DT-20.

#### Scenario: Subida limpa
- **WHEN** o desenvolvedor executa `docker compose up` com o `.env` local
- **THEN** `/health/ready` responde 200 e a SPA é servida pelo serviço `web`

### Requirement: Pipeline de CI
O `ci.yml` SHALL executar, em pull request e push: restore e build com warnings como erro, `dotnet format --verify-no-changes`, testes unitários e de integração com cobertura, `npm ci`, `ng lint`, `ng test` e `ng build`. Origem: DT-30 a DT-32, DT-36.

#### Scenario: Lint falho
- **WHEN** o código viola a formatação ou o lint
- **THEN** o CI falha e o merge é bloqueado

### Requirement: Gate de cobertura
O CI SHALL falhar se a cobertura de linhas de `Clinica.Domain` ou `Clinica.Application` ficar abaixo de 80 %. Origem: DT-32.

#### Scenario: Cobertura insuficiente
- **WHEN** a cobertura de `Clinica.Application` é 79 %
- **THEN** o job de testes falha

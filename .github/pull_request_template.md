<!--
Modelo padrão de Pull Request.
Fluxo obrigatório do projeto: PRD → TDD → SDD → Pull Request (ver CLAUDE.md).
Preencha todas as seções; use "N/A" quando não se aplicar.
-->

## Resumo

<!-- O que este PR entrega e por quê, em 1 a 3 frases. -->

## Tipo de mudança

- [ ] Funcionalidade (`feat`)
- [ ] Correção (`fix`)
- [ ] Documentação (`docs`) — PRD, TDD ou specs
- [ ] Refatoração (`refactor`)
- [ ] Testes (`test`)
- [ ] Infraestrutura / configuração (`chore`, `ci`)

## Rastreabilidade

| Etapa | Referência |
|---|---|
| **PRD** | `docs/prd/PRD-NNN-<slug>.md` — RF-xx, RN-xx, HU-xx |
| **TDD** | `docs/tdd/TDD-NNN-<slug>.md` — DT-xx |
| **SDD (change OpenSpec)** | `openspec/changes/<nome-da-change>/` |

## Alterações

<!-- Lista objetiva das principais mudanças (camadas, módulos, arquivos relevantes). -->

-

## Decisões e desvios

<!-- Decisões tomadas durante a implementação que não estavam no TDD, ou desvios em relação ao PRD/TDD/specs. Se houver, indique se os documentos de origem foram atualizados. -->

- N/A

## Como testar

<!-- Passos para validar manualmente e comandos para os testes automatizados. -->

1.

## Evidências

<!-- Prints, saídas de teste, logs ou links. Obrigatório para mudanças de interface. -->

## Checklist

- [ ] A change segue a ordem PRD → TDD → SDD e referencia os IDs de origem.
- [ ] Nenhuma contradição com o PRD ou com as decisões técnicas (DT) vigentes.
- [ ] Todas as tarefas de `tasks.md` da change estão concluídas.
- [ ] Build sem erros e testes automatizados passando.
- [ ] Testes novos/atualizados cobrem o comportamento alterado.
- [ ] Código e documentação em português brasileiro, com documentação de código (ex.: `<summary>`, TSDoc) quando aplicável.
- [ ] Specs sincronizadas/arquivadas no OpenSpec, quando a change estiver concluída.
- [ ] Sem segredos, credenciais ou dados reais de pacientes no diff.

## Observações para revisão

<!-- Pontos de atenção, riscos ou itens pendentes para um PR futuro. -->

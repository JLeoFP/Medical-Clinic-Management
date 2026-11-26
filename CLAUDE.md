# Instruções para agentes

1. Responde siempre en español.
2. Codigo o documentos generados por Agentes debe ser escritos en portugues.

## Geral

- Sempre utilizar o sistema de questões do ambiente (harness) ao invés de fazer perguntas na resposta.
- Escrever documentação (legível pela linguagem utilizada, ex: <summary> no C#) e comentários técnicos durante a implementação quando aplicável.
- A nomenclatura do código deve ser feita em português brasileiro onde razoável, seguindo padrões recomendados pela comunidade.

## Fluxo de trabalho

Toda funcionalidade segue, obrigatoriamente, a ordem **PRD → TDD → SDD → Pull Request**.

**Branches:** o agente deve executar o fluxo completo (todas as etapas, inclusive PRD e TDD) em uma *feature branch* criada a partir da `main` atualizada, nunca diretamente na `main`. A integração na `main` ocorre exclusivamente via Pull Request com destino (`base`) `main`, usando o modelo `.github/pull_request_template.md`. Antes de abrir o PR, atualizar a branch com a `main` e resolver conflitos.

Não avançar para a etapa seguinte sem que a anterior exista e esteja coerente com ela; se uma etapa posterior revelar mudança de requisito ou de decisão técnica, atualizar primeiro o documento da etapa de origem.

1. **PRD (requisitos de produto)**
   - Gerado com a skill `generate-prd` a partir de `docs/Entrevista.md`.
   - Saída: `docs/prd/PRD-<NNN>-<slug>.md`.
   - Define o *quê* e o *porquê* (RF, RNF, RN, HU, fases). Não contém decisões técnicas.
2. **TDD (desenho técnico)**
   - Gerado com a skill `generate-tdd` a partir de um PRD aprovado.
   - Saída: `docs/tdd/TDD-<NNN>-<slug>.md`, com registro centralizado de decisões técnicas (`DT-NN`).
   - Define o *como*: arquitetura, stack, dados, segurança, infraestrutura e testes.
3. **SDD (desenvolvimento guiado por especificações, OpenSpec)**
   - Cada entrega é uma *change* em `openspec/changes/<nome>/`, criada com `openspec-propose` (proposal, design, specs, tasks) e implementada com `openspec-apply-change`.
   - Cada change deve referenciar os IDs de origem (`RF-xx`, `RN-xx`, `DT-xx`) e não pode contradizer o PRD nem o TDD.
   - Ao concluir, sincronizar/arquivar com `openspec-sync-specs` / `openspec-archive-change`.
4. **Pull Request**
   - Uma branch por change (ex.: `feat/<nome-da-change>`, `docs/<nome>`), nunca commitar direto na `main`.
   - Abrir o PR usando o modelo `.github/pull_request_template.md`, preenchendo todas as seções (rastreabilidade PRD/TDD/SDD, testes e checklist).
   - O PR só é aberto com build e testes passando e com as tarefas da change marcadas como concluídas.

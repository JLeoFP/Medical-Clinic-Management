---
description: Genera un Documento Técnico de Diseño (TDD) detallado a partir de un PRD, centralizando las decisiones técnicas
argument-hint: <ruta del PRD o descripción del alcance técnico>
allowed-tools: Read, Write, Edit, Glob, Grep, AskUserQuestion, Skill, mcp__context7__resolve-library-id, mcp__context7__query-docs
---

Genera un Documento Técnico de Diseño (TDD) usando la skill `generate-tdd`.

**Prompt inicial:** $ARGUMENTS

Instrucciones:

1. Invoca la skill `generate-tdd` y sigue sus pasos al pie de la letra, usando el prompt inicial de arriba como entrada.
2. Si el prompt inicial está vacío, usa el PRD más reciente de `docs/prd/`; si hay más de uno o ninguno, pregunta al usuario cuál usar antes de continuar.
3. Lee completo el PRD de origen (fuente primaria) y el contexto técnico existente (`README.md`, código, `openspec/`, TDDs previos en `docs/tdd/`).
4. Pregunta con `AskUserQuestion` todas las decisiones de arquitectura, lenguaje, framework, bibliotecas, datos, seguridad, infraestructura, pruebas y convenciones que no estén ya tomadas. No decidas por tu cuenta.
5. Registra cada decisión en el registro centralizado de decisiones técnicas (`DT-NN`) del TDD, con opciones consideradas, justificación y consecuencias.
6. Incluye el roadmap de implementación: fase 0 de fundación, las fases del PRD divididas en incrementos por dependencia técnica y, si aplica, la fase de despliegue; cada fase con alcance, entregables, criterio de conclusión y tag, sin fechas inventadas.
7. Redacta el TDD en el idioma que indique `CLAUDE.md` (actualmente portugués).
8. Guarda el resultado en `docs/tdd/` y responde solo con la ruta, un resumen breve, las decisiones tomadas, las fases del roadmap, los supuestos y las preguntas abiertas.

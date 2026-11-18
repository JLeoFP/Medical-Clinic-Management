---
description: Genera un PRD completo a partir de un prompt inicial
argument-hint: <descripción de la idea, problema o funcionalidad>
allowed-tools: Read, Write, Edit, Glob, Grep, AskUserQuestion, Skill
---

Genera un Documento de Requisitos de Producto (PRD) usando la skill `generate-prd`.

**Prompt inicial:** $ARGUMENTS

Instrucciones:

1. Invoca la skill `generate-prd` y sigue sus pasos al pie de la letra, usando el prompt inicial de arriba como entrada.
2. Si el prompt inicial está vacío, pide al usuario que describa el producto o la funcionalidad antes de continuar.
3. Lee completa la entrevista `docs/Entrevista.md`: es la fuente primaria del PRD.
4. Redacta el PRD en el idioma que indique `CLAUDE.md` (actualmente portugués).
5. Clarifica todas las especificaciones ambiguas con `AskUserQuestion` antes de escribir la versión final.
6. Guarda el resultado en `docs/prd/` y responde solo con la ruta, un resumen breve, los supuestos y las preguntas abiertas.

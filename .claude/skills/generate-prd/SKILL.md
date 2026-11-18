---
name: generate-prd
description: Genera un Documento de Requisitos de Producto (PRD) completo a partir de un prompt inicial, usando como fuente obligatoria la entrevista de levantamiento (docs/Entrevista.md) y clarificando toda ambigüedad con preguntas al usuario antes de finalizar. Usar cuando el usuario pida "generar un PRD", "documento de requisitos", "product requirements", "especificar el producto", o invoque /generate-prd o /generate-prd-command.
allowed-tools: Read, Write, Edit, Glob, Grep, AskUserQuestion
metadata:
  author: local
  version: "1.0"
---

Genera un **Documento de Requisitos de Producto (PRD)** completo a partir del prompt inicial del usuario.

**Idioma**: el PRD se redacta en el idioma que exija `CLAUDE.md` para documentos generados por agentes (actualmente **portugués**). Si `CLAUDE.md` no indica nada, usa el idioma del prompt. Los títulos de [template.md](template.md) se traducen a ese idioma, manteniendo su estructura. Las respuestas al usuario en el chat siguen el idioma que indique `CLAUDE.md` para conversar (actualmente español).

**Límite de alcance**: este flujo solo produce documentación. No escribas ni modifiques código del proyecto. Al terminar el PRD, detente y presenta el resultado.

---

## Entrada

El prompt inicial llega como argumento (ej.: `/generate-prd agenda de controles futuros para pacientes`). Puede ser una idea de una línea o una descripción extensa.

Si no se recibe ningún prompt, pregunta al usuario con **AskUserQuestion** (o en texto) qué producto o funcionalidad quiere documentar, y no continúes hasta tenerlo.

---

## Pasos

### 1. Leer la entrevista (obligatorio)

La **entrevista de levantamiento de requisitos** es la fuente primaria y obligatoria del PRD:

- Lee **completa** `docs/Entrevista.md` con **Read** antes de redactar nada. Si hay otras entrevistas (`docs/Entrevista*.md`), léelas también.
- Si la carpeta o el archivo no existen, avisa al usuario y pregúntale con **AskUserQuestion** si desea continuar solo con el prompt o indicar otra ruta.
- Extrae de la entrevista, como notas de trabajo:
  - Dolores y problemas del negocio.
  - Reglas de negocio explícitas (ej.: límites de agenda, restricciones de turnos).
  - Actores/roles mencionados (médicos, recepción, pacientes, etc.).
  - Datos que se registran (pacientes, historia clínica, contactos…).
  - Necesidades y expectativas expresadas por el entrevistado.
- Cada requisito, regla y objetivo que salga de la entrevista debe citar su sección (ej.: _Fuente: Entrevista §2_). La entrevista prevalece sobre supuestos propios: nunca contradigas lo que dijo el entrevistado.

### 2. Reunir contexto complementario

Lee además lo que exista y sea relevante:

- `README.md` — visión general del producto.
- `openspec/specs/**/spec.md` y `openspec/config.yaml` — especificaciones y restricciones existentes.
- `docs/prd/*.md` — PRDs previos, para no duplicar y mantener consistencia de numeración y terminología.

Usa `Glob`/`Grep` para localizar fragmentos relacionados con el prompt.

### 2b. Clarificar ambigüedades (obligatorio, antes de finalizar)

Compara el prompt con la entrevista y el contexto, y lista **todas** las especificaciones ambiguas, incompletas o contradictorias. Por ejemplo:

- Reglas de negocio vagas (plazos, límites, excepciones).
- Roles y permisos no definidos (¿quién puede ver/editar qué?).
- Comportamiento esperado no especificado (notificaciones: canal, frecuencia, destinatario).
- Contradicciones entre el prompt y la entrevista.
- Alcance difuso, integraciones, restricciones legales/de privacidad.

Reglas:

- **Toda ambigüedad debe clarificarse con la herramienta de preguntas del entorno (`AskUserQuestion`)** antes de escribir la versión final del PRD. No la resuelvas por tu cuenta.
- Cada pregunta debe ser autocontenida (el usuario puede ver solo el diálogo): menciona el contexto y, si aplica, la sección de la entrevista que la origina.
- Ofrece 2–4 opciones concretas por pregunta; si recomiendas una, colócala primero con "(Recomendado)".
- Agrupa hasta 4 preguntas por llamada y haz **tantas rondas como sean necesarias**, priorizando las que más impactan en el alcance. Si una respuesta abre nuevas dudas, pregunta de nuevo.
- Incorpora cada respuesta al PRD citando la fuente como _Fuente: Clarificación con usuario_.
- Solo puede quedar como `[POR DEFINIR]` (en **Preguntas abiertas**) lo que el usuario responda explícitamente que aún no está decidido. Solo puede quedar como `[SUPUESTO]` lo que el usuario acepte explícitamente dejar como supuesto.
- No pidas confirmación sobre hechos ya establecidos claramente en la entrevista.

### 3. Definir nombre y ubicación

- Slug en kebab-case derivado del prompt (ej.: `agenda-controles-futuros`).
- Ruta: `docs/prd/PRD-<NNN>-<slug>.md`, donde `<NNN>` es el siguiente número correlativo de 3 dígitos según los PRDs existentes (empezar en `001`).
- Si ya existe un PRD sobre el mismo tema, pregunta si se debe actualizar ese en lugar de crear uno nuevo.

### 4. Redactar el PRD

Sigue **exactamente** la estructura de [template.md](template.md). Reglas de redacción:

- **Problema antes que solución**: el "por qué" debe quedar claro antes del "qué".
- **Requisitos funcionales** con ID (`RF-01`, `RF-02`…), prioridad **MoSCoW** (Must / Should / Could / Won't) y redactados como comportamientos verificables ("El sistema debe…").
- **Historias de usuario** con formato `Como <rol>, quiero <acción>, para <beneficio>` y **criterios de aceptación** en Gherkin (`Dado / Cuando / Entonces`).
- **Requisitos no funcionales** con ID (`RNF-01`…) y métricas concretas cuando sea posible (tiempos, disponibilidad, volúmenes). En contextos de salud, considera privacidad de datos sensibles (LGPD/leyes locales), auditoría y trazabilidad.
- **Métricas de éxito** medibles (línea base → objetivo).
- **Fuera de alcance** explícito: siempre al menos un ítem.
- Trazabilidad: cuando un requisito venga de una entrevista o documento, cítalo brevemente (ej.: _Fuente: Entrevista §1_).
- `[SUPUESTO]` y `[POR DEFINIR]` solo para lo que el usuario aceptó así durante la clarificación (paso 2b).
- Nada de relleno genérico: cada sección debe ser específica al prompt y al proyecto. Si una sección realmente no aplica, escribe "No aplica" y el motivo.

### 5. Autoverificación

Antes de guardar, comprueba:

- [ ] Cada objetivo tiene al menos una métrica de éxito asociada.
- [ ] Cada requisito funcional `Must` está cubierto por al menos una historia de usuario con criterios de aceptación.
- [ ] Los IDs son únicos y correlativos.
- [ ] Hay sección de fuera de alcance, riesgos y preguntas abiertas.
- [ ] La entrevista fue leída completa y sus dolores, reglas y actores están reflejados y citados.
- [ ] No hay contradicciones con las reglas de negocio de la entrevista ni de otras fuentes.
- [ ] Todas las ambigüedades detectadas fueron clarificadas con `AskUserQuestion`; no queda ningún `[SUPUESTO]` o `[POR DEFINIR]` que el usuario no haya aceptado explícitamente.

Corrige lo que falle. Si al verificar aparecen nuevas ambigüedades, vuelve al paso 2b antes de guardar.

### 6. Guardar y presentar

1. Escribe el archivo con **Write** en la ruta definida.
2. Responde al usuario con:
   - Ruta del archivo creado.
   - Resumen de 3–5 líneas (problema, solución propuesta, cantidad de RF/RNF/historias).
   - Lista de supuestos y preguntas abiertas más relevantes.
   - Siguiente paso sugerido (ej.: revisar el PRD o convertirlo en un cambio con `/opsx:propose`).

No pegues el PRD completo en el chat.

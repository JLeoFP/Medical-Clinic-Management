---
name: generate-tdd
description: Genera un Documento Técnico de Diseño (TDD) detallado a partir de un PRD, preguntando al usuario cada decisión de arquitectura, lenguaje, framework, bibliotecas, datos, seguridad, infraestructura, pruebas y convenciones, y centralizándolas en un registro único de decisiones técnicas (DT/ADR). Usar cuando el usuario pida "generar un TDD", "documento técnico", "diseño técnico", "technical design", "arquitectura del sistema", "definir el stack", "registrar decisiones técnicas", o invoque /generate-tdd o /generate-tdd-command.
allowed-tools: Read, Write, Edit, Glob, Grep, AskUserQuestion, mcp__context7__resolve-library-id, mcp__context7__query-docs
metadata:
  author: local
  version: "1.1"
---

Genera un **Documento Técnico de Diseño (TDD)** detallado que traduce los requisitos de un PRD en una solución técnica implementable, y que funciona como **fuente única de verdad de las decisiones técnicas** del proyecto.

**Idioma**: el TDD se redacta en el idioma que exija `CLAUDE.md` para documentos generados por agentes (actualmente **portugués**). Si `CLAUDE.md` no indica nada, usa el idioma del prompt. Los títulos de [template.md](template.md) se traducen a ese idioma, manteniendo su estructura. Los nombres propios de tecnologías, comandos y fragmentos de código no se traducen. Las respuestas al usuario en el chat y las preguntas de `AskUserQuestion` siguen el idioma que indique `CLAUDE.md` para conversar (actualmente español).

**Límite de alcance**: este flujo solo produce documentación. No escribas, generes ni modifiques código del proyecto (ni scaffolding, ni archivos de configuración). Los fragmentos de código dentro del TDD son ilustrativos. Al terminar el TDD, detente y presenta el resultado.

**Principio central**: las decisiones técnicas las toma el usuario. Tu papel es detectar cada decisión necesaria, presentar opciones con sus trade-offs y una recomendación fundamentada, y registrar lo decidido. Nunca elijas una tecnología, patrón o biblioteca sin que el usuario la haya decidido o aceptado explícitamente.

---

## Entrada

El prompt inicial llega como argumento. Puede ser:

- Una ruta a un PRD (ej.: `/generate-tdd docs/prd/PRD-001-plataforma-gestao-clinica.md`).
- Una descripción del alcance técnico (ej.: `/generate-tdd diseño del módulo de controles futuros`).
- Vacío.

Resolución del PRD de origen:

1. Si el prompt es una ruta, usa ese archivo.
2. Si no, busca en `docs/prd/*.md` el PRD que corresponda al prompt (por título o tema).
3. Si no hay prompt y existe un único PRD, úsalo. Si hay varios o ninguno, pregunta con **AskUserQuestion** cuál usar (o si se quiere continuar sin PRD, solo con el prompt). No continúes sin resolverlo.

Si el prompt delimita un subconjunto (un módulo, una fase como "MVP"), el TDD cubre solo ese alcance, pero las decisiones transversales (stack, arquitectura, seguridad) se registran igual.

---

## Pasos

### 1. Leer el PRD (obligatorio)

Lee **completo** el PRD de origen con **Read**. Extrae como notas de trabajo:

- Requisitos funcionales (`RF-*`) y su fase de entrega.
- Requisitos no funcionales (`RNF-*`) con sus métricas: son los **drivers arquitectónicos** principales.
- Reglas de negocio (`RN-*`) con impacto técnico (concurrencia, cálculos, ventanas de tiempo, unicidad).
- Modelo de datos conceptual, flujos principales, integraciones, riesgos y preguntas abiertas.
- Plan de entregas / fases del PRD (base del roadmap de implementación) e historias de usuario (`HU-*`) de cada fase.
- Restricciones legales o de privacidad (ej.: LGPD, datos de salud).

Cada elemento del diseño que derive del PRD debe citar su ID (ej.: _Atende: RF-12, RNF-06_).

### 2. Reunir contexto técnico existente

Lee lo que exista y sea relevante, para no volver a decidir lo que ya está decidido:

- `CLAUDE.md` — reglas de idioma y convenciones del proyecto.
- `README.md` — puede declarar tecnologías ya elegidas (badges, secciones de stack).
- `docs/tdd/*.md` — TDDs previos y su registro de decisiones (respeta numeración `DT-NN` y no contradigas decisiones vigentes sin preguntar).
- `docs/Entrevista*.md` — solo como contexto secundario.
- `openspec/config.yaml` y `openspec/specs/**/spec.md` — restricciones y especificaciones.
- Código y manifiestos si existen (usa `Glob`): `*.csproj`, `*.sln`, `package.json`, `angular.json`, `pom.xml`, `requirements.txt`, `pyproject.toml`, `go.mod`, `Dockerfile`, `docker-compose*.yml`, `.github/workflows/*`, etc.

Clasifica cada tecnología o patrón encontrado como **decisión preexistente** (fuente: README, código, TDD previo). Las decisiones preexistentes no se reabren: se confirman todas juntas en una sola pregunta (paso 3, ronda 0) y se registran como `DT-NN` con _Fuente: Decisión preexistente (<archivo>)_.

### 3. Levantar y preguntar las decisiones técnicas (obligatorio)

Usa el catálogo [decision-catalog.md](decision-catalog.md) como checklist de las decisiones que un TDD completo debe cubrir. Para cada ítem del catálogo determina su estado:

- **Preexistente**: ya está decidido en el repositorio → solo se confirma.
- **Derivada**: la impone sin ambigüedad el PRD o una decisión ya tomada (ej.: con Entity Framework Core elegido, las migraciones se hacen con EF Migrations) → se presenta como recomendación en la pregunta de su categoría, no se asume en silencio.
- **Abierta**: requiere decisión del usuario → se pregunta.
- **No aplica**: el alcance no la necesita → se registra como "No aplica" con el motivo.

#### Orden de las rondas

Haz las preguntas en rondas con **AskUserQuestion** (máximo 4 preguntas por llamada), de lo más estructural a lo más específico, porque las primeras respuestas cambian las recomendaciones posteriores:

0. **Contexto y restricciones**: confirmación de decisiones preexistentes; experiencia del equipo, presupuesto de infraestructura, plazo y fechas límite (ej.: entrega, defensa, vencimiento de créditos de nube), restricciones de hosting o institucionales. El plazo alimenta el roadmap; si el usuario no informa fechas, el roadmap queda sin fechas.
1. **Arquitectura**: estilo arquitectónico, patrón de organización interna, tipo de cliente/render, estructura del repositorio, estilo de API.
2. **Lenguaje, runtime y frameworks**: backend, frontend, UI/estilos, versiones (LTS o última estable).
3. **Datos**: SGBD, ORM/acceso a datos, migraciones, caché, archivos, estrategia de concurrencia.
4. **Seguridad y privacidad**: autenticación, autorización, gestión de secretos, cifrado, auditoría, cumplimiento legal.
5. **Integraciones y procesamiento asíncrono**: proveedores externos (e-mail, mensajería), tareas programadas, colas.
6. **Bibliotecas transversales**: validación, mapeo, fechas/zonas horarias, i18n/formatos, logging, reportes/PDF, documentación de API.
7. **Calidad y pruebas**: frameworks de prueba por nivel, cobertura mínima, lint/formatter, análisis estático.
8. **Infraestructura y operación**: hosting, contenedores, entornos, CI/CD, backups, observabilidad.
9. **Convenciones**: estructura de carpetas, nombres, idioma de los identificadores en el código, ramas y commits, versionado.

Salta las rondas que no tengan decisiones abiertas. Si una respuesta abre nuevas dudas o invalida una recomendación anterior, pregunta de nuevo antes de avanzar.

#### Reglas para cada pregunta

- **Autocontenida**: el usuario puede ver solo el diálogo. Menciona el requisito o restricción que motiva la decisión (ej.: "RNF-07 exige 99 % de disponibilidad del portal…").
- **2–4 opciones concretas** y mutuamente excluyentes, cada una con su trade-off en la descripción (costo, complejidad, curva de aprendizaje, madurez, ajuste a los RNF).
- **Recomendación fundamentada**: si recomiendas una opción, colócala primero con "(Recomendado)" y justifícala en su descripción con base en el PRD, las decisiones previas y las restricciones del equipo. Prioriza la coherencia con las decisiones ya tomadas y la simplicidad adecuada a la escala del proyecto (no recomiendes microservicios, Kubernetes o colas distribuidas si los RNF no lo justifican).
- **Previews** para comparar alternativas visuales: árboles de carpetas, organización en capas, ejemplos de contrato de API o de convención de código. Usa el campo `preview` de las opciones (solo en preguntas de selección única).
- **multiSelect** solo cuando las opciones no sean excluyentes (ej.: niveles de prueba a adoptar, entornos a mantener).
- **Compatibilidad**: no ofrezcas combinaciones incompatibles con decisiones ya tomadas (ej.: un ORM que no soporte el SGBD elegido).
- **Versiones**: cuando esté disponible el MCP `context7`, consúltalo (`resolve-library-id` → `query-docs`) para confirmar la versión estable/LTS vigente y la compatibilidad entre bibliotecas antes de proponerlas. Si no está disponible, indica la versión como "LTS vigente" y regístralo como supuesto aceptado por el usuario.
- No preguntes lo que ya es preexistente o lo que el usuario ya respondió.

#### Registro de lo decidido

Cada respuesta se convierte en una entrada del **registro centralizado de decisiones técnicas** (`DT-01`, `DT-02`…, continuando la numeración de TDDs previos) con: título, categoría, estado, contexto/driver, opciones consideradas, decisión, justificación, consecuencias (positivas y negativas), requisitos relacionados y fuente (_Fuente: Clarificación con usuario (AAAA-MM-DD)_ o _Fuente: Decisión preexistente (<archivo>)_).

- Solo puede quedar como `[POR DEFINIR]` (en **Preguntas abiertas**) lo que el usuario responda explícitamente que aún no está decidido; en ese caso la DT queda con estado **Propuesta**.
- Solo puede quedar como `[SUPUESTO]` lo que el usuario acepte explícitamente dejar como supuesto.
- Si una decisión nueva sustituye a una de un TDD previo, la anterior se marca **Sustituida por DT-NN** (pregunta antes de sustituir).

### 4. Definir nombre y ubicación

- Slug en kebab-case derivado del PRD o del alcance (ej.: `plataforma-gestao-clinica`).
- Ruta: `docs/tdd/TDD-<NNN>-<slug>.md`, donde `<NNN>` es el siguiente número correlativo de 3 dígitos según los TDDs existentes (empezar en `001`).
- Si ya existe un TDD para el mismo PRD o alcance, pregunta si se debe actualizar ese (nueva versión, nuevas DT, DT sustituidas) en lugar de crear uno nuevo.

### 5. Redactar el TDD

Sigue **exactamente** la estructura de [template.md](template.md). Reglas de redacción:

- **Del requisito a la solución**: cada componente, endpoint, tabla y tarea programada debe indicar qué `RF`/`RNF`/`RN` atiende. Todo `RF` del alcance debe aparecer en la matriz de trazabilidad.
- **Decisiones centralizadas**: el registro de decisiones (sección del template) es la única fuente de verdad. El resto del documento referencia las decisiones por ID (ej.: "ORM: Entity Framework Core (DT-07)") en vez de repetir su justificación. Ninguna tecnología puede mencionarse en el documento sin una DT que la respalde.
- **Diagramas en Mermaid**: contexto (C4 nivel 1), contenedores (C4 nivel 2), modelo entidad-relación y diagramas de secuencia de los flujos críticos del PRD.
- **Modelo de datos físico**: tablas, columnas con tipo, nulabilidad, claves, índices, restricciones de unicidad y reglas de borrado, derivado del modelo conceptual del PRD.
- **API**: convenciones generales (versionado, paginación, formato de errores, autenticación) y tabla de endpoints con método, ruta, perfil autorizado y requisito atendido.
- **Seguridad**: matriz perfil × recurso/acción derivada de los RF, y cómo se cumplen los RNF de privacidad y auditoría.
- **Pruebas**: estrategia por nivel con herramientas decididas y cómo se verifican los criterios de aceptación del PRD.
- **Roadmap de implementación**: fases ordenadas por dependencia técnica, derivadas del plan de entregas del PRD:
  - Empieza con una **fase 0 de fundación** (estructura de la solución, pipeline transversal, autenticación base, primera migración, entorno local y CI) y termina con una fase de **despliegue/entrega** cuando el alcance incluya infraestructura.
  - Cada fase del PRD se mantiene como fase del roadmap (no se mezclan requisitos de fases distintas). Si la implementación de una fase necesita adelantar una pieza de otra (ej.: catálogo usado por un RF del MVP), regístralo explícitamente como adelanto con su motivo.
  - Las fases grandes se dividen en incrementos verticales (dominio + API + pantalla + pruebas) en orden de dependencia.
  - Cada fase indica: objetivo, alcance (`RF`/`RN`/`HU`), entregables (`CMP`, `API`, tablas, `JOB`, pantallas, archivos de infraestructura) con las `DT` aplicadas, dependencias, criterio de conclusión verificable (pruebas o escenarios Gherkin) y versión/tag según la DT de versionado.
  - Sugiere un nombre de cambio OpenSpec (`kebab-case`) por fase o incremento cuando el proyecto use OpenSpec.
  - Incluye un diagrama Mermaid de dependencias entre fases.
  - No inventes fechas ni duraciones: úsalas solo si el usuario las informó. Las restricciones temporales derivadas de decisiones (ej.: créditos de nube con vencimiento) sí se registran.
  - El roadmap no introduce decisiones nuevas: solo ordena lo decidido. Si al planificar surge una decisión abierta, vuelve al paso 3.
- **Concreción**: nada de relleno genérico. Si una sección no aplica al alcance, escribe "No aplica" y el motivo.

### 6. Autoverificación

Antes de guardar, comprueba:

- [ ] El PRD fue leído completo y cada `RF` del alcance aparece en la matriz de trazabilidad con al menos un componente y una forma de prueba.
- [ ] Cada `RNF` tiene una táctica o decisión técnica que lo atiende.
- [ ] Todas las categorías de [decision-catalog.md](decision-catalog.md) están decididas, marcadas "No aplica" con motivo, o en Preguntas abiertas con aceptación explícita del usuario.
- [ ] Cada tecnología, biblioteca o patrón mencionado en el documento tiene una `DT` en el registro, y cada `DT` está referenciada en al menos una sección.
- [ ] Las decisiones son compatibles entre sí (lenguaje ↔ framework ↔ ORM ↔ SGBD ↔ hosting ↔ CI/CD).
- [ ] No hay contradicciones con el PRD, con decisiones preexistentes ni con TDDs previos.
- [ ] Los IDs (`DT`, componentes, endpoints) son únicos y correlativos.
- [ ] El roadmap cubre todos los `RF` del alcance (cada uno en exactamente una fase o incremento), respeta las fases del PRD, ordena las fases por dependencia y cada fase tiene criterio de conclusión verificable; no contiene fechas que el usuario no haya informado.
- [ ] No queda ningún `[SUPUESTO]` o `[POR DEFINIR]` que el usuario no haya aceptado explícitamente.

Corrige lo que falle. Si al verificar aparecen nuevas decisiones abiertas, vuelve al paso 3 antes de guardar.

### 7. Guardar y presentar

1. Escribe el archivo con **Write** en la ruta definida (crea `docs/tdd/` si no existe).
2. Responde al usuario con:
   - Ruta del archivo creado.
   - Resumen de 3–5 líneas (estilo arquitectónico, stack principal, cantidad de componentes/endpoints/tablas).
   - Tabla breve de las decisiones técnicas tomadas (ID, decisión).
   - Fases del roadmap en una línea cada una (fase, alcance, tag).
   - Supuestos y preguntas abiertas más relevantes.
   - Siguiente paso sugerido (ej.: revisar el TDD o convertir la primera fase del roadmap en un cambio con `/opsx:propose <nombre sugerido>`).

### Actualizar un TDD existente

Si el TDD ya existe y la petición es agregar o revisar el roadmap (u otra sección), no rehagas el documento: lee el TDD completo, edita solo lo necesario, incrementa la versión con una fila de revisión en el encabezado, renumera las secciones afectadas y corrige las referencias cruzadas a ellas (dentro del TDD y en otros documentos que las citen).

No pegues el TDD completo en el chat.

# Catálogo de decisiones técnicas

Checklist de las decisiones que un TDD completo debe cubrir. Para cada ítem, determina si es **preexistente**, **derivada**, **abierta** o **no aplica** (ver paso 3 de [SKILL.md](SKILL.md)). Las opciones listadas son ejemplos para orientar las preguntas, no una lista cerrada: adáptalas al stack ya decidido y a los RNF del PRD.

Cada decisión tomada se registra como `DT-NN` con la categoría indicada entre corchetes.

---

## 0. Contexto y restricciones [CTX]

No son decisiones técnicas en sí, pero condicionan todas las recomendaciones.

| Ítem | Qué preguntar |
|---|---|
| Decisiones preexistentes | Confirmar en bloque las tecnologías ya declaradas (README, código, TDDs previos). |
| Experiencia del equipo | Lenguajes y frameworks que el equipo domina. |
| Presupuesto | Costo mensual aceptable de infraestructura (gratuito, bajo, sin restricción). |
| Plazo y tamaño del equipo | Fecha de entrega del MVP; cantidad de desarrolladores. |
| Restricciones externas | Hosting institucional, requisitos académicos, normativa (LGPD, datos de salud). |

## 1. Arquitectura [ARQ]

| Decisión | Opciones típicas |
|---|---|
| Estilo arquitectónico | Monolito modular · Monolito en capas · Microservicios · Serverless |
| Organización interna del backend | Capas (N-tier) · Clean Architecture · Hexagonal (ports & adapters) · Vertical slices |
| Patrones de aplicación | CQRS/mediador · Servicios de aplicación simples · Repository/Unit of Work · Domain events |
| Tipo de cliente | SPA · SSR · MPA · PWA · App móvil nativa/híbrida |
| Separación de clientes | Un frontend con áreas por perfil · Frontends separados (ej.: portal del paciente vs. backoffice) |
| Estructura del repositorio | Monorepo · Multirepo; herramienta de monorepo si aplica |
| Estilo de API | REST · GraphQL · gRPC · BFF |
| Comunicación entre componentes | Síncrona (HTTP) · Asíncrona (eventos/colas) · Mixta |

## 2. Lenguaje, runtime y frameworks [LNG] [FRW]

| Decisión | Opciones típicas |
|---|---|
| Lenguaje y versión del backend | C#/.NET (LTS) · Java/Spring · TypeScript/Node · Python · Go |
| Framework backend | ASP.NET Core (Controllers · Minimal APIs) · Spring Boot · NestJS · Express · Django/FastAPI |
| Lenguaje y versión del frontend | TypeScript (versión) · JavaScript |
| Framework frontend | Angular · React (+ Next.js) · Vue (+ Nuxt) · Blazor |
| Gestión de estado del frontend | Signals/servicios · NgRx · Redux/Zustand · Pinia |
| UI y estilos | Tailwind CSS · SCSS · Angular Material · PrimeNG · Bootstrap · shadcn/ui; combinación y criterio de uso |
| Gestor de paquetes / build | npm · pnpm · yarn; dotnet CLI · Maven · Gradle |

## 3. Datos [DAT]

| Decisión | Opciones típicas |
|---|---|
| SGBD | PostgreSQL · SQL Server · MySQL/MariaDB · MongoDB · SQLite (solo dev/test) |
| Acceso a datos | ORM (Entity Framework Core · Hibernate · Prisma · TypeORM) · Micro-ORM (Dapper) · SQL puro |
| Migraciones | Migraciones del ORM · Flyway · Liquibase · scripts versionados |
| Estrategia de IDs | Autoincremental · UUID/GUID v4 · UUID v7/ULID |
| Concurrencia | Bloqueo optimista (rowversion/xmin) · Bloqueo pesimista · Restricciones únicas en BD |
| Borrado | Borrado físico · Soft delete · Archivado |
| Caché | Sin caché · En memoria · Redis |
| Almacenamiento de archivos | No aplica · Disco local/volumen · Object storage (S3/Azure Blob/MinIO) |
| Datos iniciales | Seeds en migraciones · Scripts separados |

## 4. Seguridad y privacidad [SEG]

| Decisión | Opciones típicas |
|---|---|
| Autenticación | ASP.NET Core Identity · Keycloak · Auth0/Entra ID · Implementación propia |
| Formato de sesión/token | JWT (access + refresh) · Cookies de sesión · Cookies HttpOnly con JWT |
| Almacenamiento del token en el cliente | Cookie HttpOnly · Memoria + refresh en cookie · localStorage (no recomendado) |
| Autorización | RBAC por perfil · Políticas/claims · ABAC |
| Política de contraseñas y bloqueo | Longitud mínima, complejidad, intentos fallidos, expiración |
| Hash de contraseñas | Por defecto del framework (PBKDF2) · Argon2 · bcrypt |
| Cifrado | TLS en tránsito; cifrado en reposo (disco/BD/columnas sensibles) |
| Gestión de secretos | Variables de entorno · User Secrets (dev) · Vault/Key Vault · Secrets del CI |
| Auditoría | Tabla de auditoría propia · Interceptor del ORM · Event sourcing parcial · Logs estructurados |
| Cumplimiento | LGPD: base legal, consentimiento, derechos del titular, retención y anonimización |
| Protecciones web | CORS, rate limiting, CSRF, cabeceras de seguridad, validación de entrada |

## 5. Integraciones y procesamiento asíncrono [INT] [ASY]

| Decisión | Opciones típicas |
|---|---|
| E-mail | SMTP propio · SendGrid · Amazon SES · Mailgun; servidor fake para dev (MailHog/Mailpit) |
| Mensajería (SMS/WhatsApp) | Twilio · WhatsApp Business API · Zenvia · No aplica en esta fase |
| Tareas programadas | Hangfire · Quartz.NET · BackgroundService/HostedService · cron del sistema · node-cron |
| Colas | No aplica · RabbitMQ · Azure Service Bus · Redis streams |
| Patrón de resiliencia | Reintentos con backoff (Polly) · Outbox · Idempotencia |

## 6. Bibliotecas transversales [LIB]

| Decisión | Opciones típicas |
|---|---|
| Validación | FluentValidation · DataAnnotations · Zod · class-validator · Validadores de formularios del framework |
| Mapeo de objetos | Manual · AutoMapper · Mapster · Mapperly |
| Fechas y zonas horarias | Tipos nativos + UTC en BD · NodaTime · date-fns · Luxon; zona horaria de referencia del negocio |
| Internacionalización y formatos | Locale fijo (ej.: pt-BR) · i18n del framework · ngx-translate/Transloco |
| Logging | Serilog · NLog · Microsoft.Extensions.Logging · Pino/Winston |
| Documentación de API | Swagger/OpenAPI (Swashbuckle · NSwag · Scalar) · Redoc |
| Generación de cliente de API | Manual · NSwag · openapi-generator · orval |
| Reportes / PDF / exportación | QuestPDF · iText · ClosedXML · exportación CSV · jsPDF |
| Componentes de calendario/agenda | FullCalendar · Componente del kit UI · Desarrollo propio |

## 7. Calidad y pruebas [QA]

| Decisión | Opciones típicas |
|---|---|
| Niveles de prueba adoptados | Unitarias · Integración · Contrato · E2E · Carga |
| Framework de pruebas backend | xUnit · NUnit · MSTest · JUnit · Jest/Vitest · pytest |
| Mocks y aserciones | Moq · NSubstitute · FluentAssertions/Shouldly |
| Pruebas de integración con BD | Testcontainers · BD en memoria · BD compartida de pruebas |
| Framework de pruebas frontend | Jasmine/Karma · Jest · Vitest · Testing Library |
| Pruebas E2E | Playwright · Cypress · Selenium |
| Cobertura mínima | Sin umbral · 60 % · 80 % · por capa |
| Lint y formato | dotnet format/analyzers · ESLint · Prettier · StyleCop · SonarQube/SonarCloud |

## 8. Infraestructura y operación [INF] [OBS]

| Decisión | Opciones típicas |
|---|---|
| Hosting | PaaS (Azure App Service · Render · Railway · Fly.io) · VPS · Cloud IaaS · On-premise |
| Contenedores | Docker + docker-compose · Kubernetes · Sin contenedores |
| Base de datos gestionada | Servicio gestionado · Contenedor propio |
| Entornos | Local · Desarrollo · Homologación/Staging · Producción |
| CI/CD | GitHub Actions · GitLab CI · Azure DevOps; etapas (build, test, lint, deploy) |
| Infraestructura como código | No aplica · Terraform · Bicep · docker-compose como única definición |
| Backups y recuperación | Frecuencia, retención, prueba de restauración (RPO/RTO) |
| Logs centralizados | Consola + proveedor del hosting · Seq · ELK/OpenSearch · Grafana Loki |
| Métricas y trazas | Sin APM · OpenTelemetry · Application Insights · Prometheus + Grafana |
| Monitoreo de errores | Sentry · Application Insights · Logs únicamente |
| Health checks y alertas | Endpoints de salud · Uptime monitor · Alertas por canal |

## 9. Convenciones [CNV]

| Decisión | Opciones típicas |
|---|---|
| Estructura de carpetas | Por capa · Por funcionalidad (feature folders) · Por módulo de dominio |
| Idioma de los identificadores | Inglés · Portugués · Mixto (dominio en portugués, técnico en inglés); considerar `CLAUDE.md` |
| Convenciones de nombres | Por lenguaje (PascalCase en C#, camelCase en TS, snake_case en BD) |
| Estrategia de ramas | GitHub Flow · Git Flow · Trunk-based |
| Mensajes de commit | Conventional Commits · Libre con plantilla |
| Versionado | SemVer · CalVer; versionado de la API (URL · header) |
| Formato de errores de la API | ProblemDetails (RFC 9457) · Formato propio |
| Documentación técnica | ADRs en el TDD · Carpeta `docs/adr` · Wiki |

# Gestão de clínica médica

Sistema web para administracion de clinica medica

# Tecnologias

![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316FF6?style=for-the-badge&logo=postgresql&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-512BD4?style=for-the-badge&logo=entity-framework&logoColor=white)
![Identity](https://img.shields.io/badge/Identity-512BD4?style=for-the-badge&logo=microsoftidentity&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)
![API REST](https://img.shields.io/badge/API%20REST-00B4D8?style=for-the-badge)
![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![Node.js](https://img.shields.io/badge/Node.js-339933?style=for-the-badge&logo=nodedotjs&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind%20CSS-06B6D4?style=for-the-badge&logo=tailwind-css&logoColor=white)
[![Lucide Icons](https://img.shields.io/badge/Lucide%20Icons-F56565?style=for-the-badge&logo=lucide&logoColor=white)](https://lucide.dev/)

# Como executar (desenvolvimento)

Pré-requisitos: .NET SDK 10, Node 24, Docker.

```bash
cp .env.example .env          # ajuste as senhas
docker compose up -d --build  # postgres, mailpit, seq, migrador, api e web
```

| Serviço | URL |
|---|---|
| SPA | http://localhost:4200 |
| API / Swagger | http://localhost:5071 · http://localhost:5071/swagger |
| Mailpit | http://localhost:8025 |
| Seq (logs) | http://localhost:5341 |

Entre com `ADMIN_EMAIL` / `ADMIN_SENHA_INICIAL` do `.env` (troca de senha obrigatória no primeiro acesso).

# Testes

```bash
cd backend  && dotnet test --solution Clinica.slnx      # unitários + integração (Docker necessário)
cd frontend && npm ci && npm test -- --no-watch         # Vitest
cd frontend && npx playwright install chromium && npx playwright test   # fumaça E2E contra o compose
```

Detalhes em [`backend/README.md`](backend/README.md) e [`frontend/README.md`](frontend/README.md). O CI
(`.github/workflows/ci.yml`) executa build, formatação, lint, testes e o gate de 80 % de cobertura em Domain e Application.


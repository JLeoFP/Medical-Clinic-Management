# Frontend — SPA da Clínica

Angular 22 (standalone, zoneless, signals) com Tailwind CSS 4, Angular CDK e Lucide. Decisões: DT-07 a DT-10, DT-46 e DT-47
no [TDD-001](../docs/tdd/TDD-001-plataforma-gestao-clinica.md).

## Pré-requisitos

- Node 24 e npm 11
- API em execução em `http://localhost:5071` (veja [`backend/README.md`](../backend/README.md)) ou o `docker-compose.yml` da raiz

## Comandos

```bash
cd frontend
npm ci
npm start             # ng serve em http://localhost:4200 com proxy de /api e /health para a API
npm run lint          # ng lint (angular-eslint)
npm run format:check  # Prettier
npm test -- --no-watch  # Vitest via ng test
npm run build         # ng build (produção)
```

O `ng serve` usa `proxy.conf.json`, de modo que a SPA e a API ficam na **mesma origem**: o cookie de sessão
`__Host-clinica-sessao` e o token anti-CSRF `__Host-XSRF-TOKEN` funcionam sem CORS (DT-45).

## Estrutura

```text
src/app/
├── core/      auth (AuthStore, guards), http (interceptors), erros (ErroApi), layout
├── shared/    ui (botão, campo, aviso, diálogo, tabela, paginação), pipes (cpf, telefone, idade), validacao
└── features/  entrar, inicio (demais módulos nas próximas fases)
```

## Convenções

- Locale `pt-BR`: datas `dd/MM/yyyy`, moeda `R$ 0.000,00`.
- Nenhum token fica acessível ao JavaScript; ao carregar, a SPA chama `GET /api/v1/auth/sessao` antes de ativar as rotas.
- Erros da API (ProblemDetails) viram `ErroApi` com mensagem em pt-BR e `erros` por campo.
- Formulários com Signal Forms (`@angular/forms/signals`); validadores reutilizáveis em `shared/validacao`.

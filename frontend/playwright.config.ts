import { defineConfig } from '@playwright/test';

// Teste de fumaça contra o docker-compose (`docker compose up -d`). Variáveis: E2E_BASE_URL, ADMIN_EMAIL, ADMIN_SENHA_INICIAL.
export default defineConfig({
  testDir: './e2e',
  timeout: 60_000,
  // A primeira carga do ng serve (vite) é lenta.
  expect: { timeout: 20_000 },
  use: { baseURL: process.env['E2E_BASE_URL'] ?? 'http://localhost:4200', locale: 'pt-BR' },
  projects: [{ name: 'chromium', use: { browserName: 'chromium' } }],
});

import { expect, test } from '@playwright/test';

const email = process.env['ADMIN_EMAIL'] ?? 'admin@clinica.local';
const senha = process.env['ADMIN_SENHA_INICIAL'] ?? 'Troque123';

test('login do admin semeado, sessão sobrevive à recarga e escrita sem token é rejeitada', async ({ page }) => {
  // Sem sessão, a rota protegida leva ao login preservando o destino.
  await page.goto('/app/inicio');
  await expect(page).toHaveURL(/\/entrar\?returnUrl=%2Fapp%2Finicio/);

  await page.getByLabel('E-mail').fill(email);
  await page.getByLabel('Senha').fill(senha);
  await page.getByRole('button', { name: 'Entrar' }).click();
  await expect(page).toHaveURL(/\/app\/inicio$/);
  await expect(page.getByRole('heading', { name: 'Início' })).toBeVisible();

  // A sessão (cookie HttpOnly) sobrevive à recarga: a SPA chama API-02 antes das rotas.
  await page.reload();
  await expect(page).toHaveURL(/\/app\/inicio$/);
  await expect(page.getByRole('heading', { name: 'Início' })).toBeVisible();

  // Escrita sem X-XSRF-TOKEN (o contexto de requisição compartilha o cookie, mas não envia o cabeçalho) → 400.
  const resposta = await page.request.post('/api/v1/auth/logout');
  expect(resposta.status()).toBe(400);
  expect((await resposta.json()).code).toBe('seguranca.antiforgery_invalido');
});

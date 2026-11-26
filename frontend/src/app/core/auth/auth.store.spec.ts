import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthStore, UsuarioSessao } from './auth.store';

const usuario: UsuarioSessao = {
  id: '1',
  nome: 'Admin',
  perfis: ['Administrador'],
  pacienteId: null,
  medicoId: null,
  cadastroPendente: false,
  trocaSenhaObrigatoria: false,
};

describe('AuthStore', () => {
  let store: AuthStore;
  let ctrl: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    store = TestBed.inject(AuthStore);
    ctrl = TestBed.inject(HttpTestingController);
  });

  it('restaura o usuário quando a API-02 responde 200', async () => {
    const p = store.restaurar();
    ctrl.expectOne('/api/v1/auth/sessao').flush(usuario);
    await p;
    expect(store.autenticado()).toBe(true);
    expect(store.usuario()?.nome).toBe('Admin');
  });

  it('fica sem usuário quando a API-02 responde 204', async () => {
    const p = store.restaurar();
    ctrl.expectOne('/api/v1/auth/sessao').flush(null, { status: 204, statusText: 'No Content' });
    await p;
    expect(store.autenticado()).toBe(false);
  });

  it('entrar guarda o usuário e sair o limpa', async () => {
    const p = store.entrar('a@b.com', 'x');
    const req = ctrl.expectOne('/api/v1/auth/login');
    expect(req.request.body).toEqual({ email: 'a@b.com', senha: 'x' });
    req.flush(usuario);
    await p;
    expect(store.autenticado()).toBe(true);
    const s = store.sair();
    ctrl.expectOne('/api/v1/auth/logout').flush(null, { status: 204, statusText: 'No Content' });
    await s;
    expect(store.autenticado()).toBe(false);
  });
});

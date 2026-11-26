import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, UrlTree } from '@angular/router';
import { AuthStore } from '../../core/auth/auth.store';
import { autenticadoGuard } from '../../core/auth/guards';
import { ErroApi } from '../../core/erros/erro-api';
import { Entrar } from './entrar';

function preencher(el: HTMLElement, email: string, senha: string): void {
  for (const [id, v] of [
    ['email', email],
    ['senha', senha],
  ] as const) {
    const i = el.querySelector('#' + id) as HTMLInputElement;
    i.value = v;
    i.dispatchEvent(new Event('input'));
  }
}

describe('Entrar', () => {
  const entrar = vi.fn();

  beforeEach(() => {
    entrar.mockReset();
    TestBed.configureTestingModule({
      imports: [Entrar],
      providers: [provideRouter([]), { provide: AuthStore, useValue: { entrar, autenticado: signal(false) } }],
    });
  });

  it('401 mostra mensagem em pt-BR e mantém o e-mail digitado', async () => {
    entrar.mockRejectedValue(new ErroApi(401, 'auth.credenciais_invalidas', 'E-mail ou senha incorretos.'));
    const f = TestBed.createComponent(Entrar);
    await f.whenStable();
    const el = f.nativeElement as HTMLElement;
    preencher(el, 'a@b.com', 'errada');
    await f.whenStable();
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    await f.whenStable();

    expect(entrar).toHaveBeenCalledWith('a@b.com', 'errada');
    expect(el.textContent).toContain('E-mail ou senha incorretos.');
    expect((el.querySelector('#email') as HTMLInputElement).value).toBe('a@b.com');
  });

  it('login válido navega para o returnUrl', async () => {
    entrar.mockResolvedValue({});
    const nav = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    const f = TestBed.createComponent(Entrar);
    f.componentRef.setInput('returnUrl', '/app/pacientes');
    await f.whenStable();
    const el = f.nativeElement as HTMLElement;
    preencher(el, 'a@b.com', 'Senha123');
    await f.whenStable();
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    await f.whenStable();
    expect(nav).toHaveBeenCalledWith('/app/pacientes');
  });

  it('campos vazios não chamam a API e mostram erros', async () => {
    const f = TestBed.createComponent(Entrar);
    await f.whenStable();
    const el = f.nativeElement as HTMLElement;
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    await f.whenStable();
    expect(entrar).not.toHaveBeenCalled();
    expect(el.textContent).toContain('Informe o e-mail.');
  });
});

describe('autenticadoGuard', () => {
  it('sem sessão redireciona para /entrar com returnUrl', () => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthStore, useValue: { autenticado: () => false } }],
    });
    const r = TestBed.runInInjectionContext(() =>
      autenticadoGuard({} as never, [{ path: 'app' }, { path: 'inicio' }] as never, {} as never),
    ) as UrlTree;
    expect(TestBed.inject(Router).serializeUrl(r)).toBe('/entrar?returnUrl=%2Fapp%2Finicio');
  });

  it('com sessão libera', () => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthStore, useValue: { autenticado: () => true } }],
    });
    expect(TestBed.runInInjectionContext(() => autenticadoGuard({} as never, [] as never, {} as never))).toBe(true);
  });
});

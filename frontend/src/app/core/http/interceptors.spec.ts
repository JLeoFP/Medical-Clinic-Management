import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthStore } from '../auth/auth.store';
import { ErroApi } from '../erros/erro-api';
import { correlationInterceptor, errorInterceptor, sessaoInterceptor } from './interceptors';

describe('interceptors', () => {
  let http: HttpClient;
  let ctrl: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([correlationInterceptor, sessaoInterceptor, errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    ctrl = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => ctrl.verify());

  it('envia X-Correlation-Id nas chamadas à API', () => {
    http.get('/api/v1/x').subscribe();
    const req = ctrl.expectOne('/api/v1/x');
    expect(req.request.headers.get('X-Correlation-Id')).toMatch(/^[0-9a-f-]{36}$/);
    req.flush({});
  });

  it('converte ProblemDetails em ErroApi com erros por campo', async () => {
    const p = new Promise<unknown>((resolve) => http.post('/api/v1/x', {}).subscribe({ error: resolve }));
    ctrl
      .expectOne('/api/v1/x')
      .flush(
        { code: 'validacao.invalida', errors: { email: ['Informe o e-mail.'] } },
        { status: 400, statusText: 'Bad Request' },
      );
    const e = (await p) as ErroApi;
    expect(e).toBeInstanceOf(ErroApi);
    expect(e.code).toBe('validacao.invalida');
    expect(e.erros['email']).toEqual(['Informe o e-mail.']);
  });

  it('em 401 limpa o AuthStore e vai ao login com returnUrl', async () => {
    const auth = TestBed.inject(AuthStore);
    vi.spyOn(auth, 'limpar');
    const nav = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const p = new Promise((resolve) => http.get('/api/v1/protegido').subscribe({ error: resolve }));
    ctrl.expectOne('/api/v1/protegido').flush({}, { status: 401, statusText: 'Unauthorized' });
    await p;
    expect(auth.limpar).toHaveBeenCalled();
    expect(nav).toHaveBeenCalledWith(['/entrar'], { queryParams: { returnUrl: router.url } });
  });

  it('401 do próprio login não redireciona', async () => {
    const nav = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const p = new Promise((resolve) => http.post('/api/v1/auth/login', {}).subscribe({ error: resolve }));
    ctrl
      .expectOne('/api/v1/auth/login')
      .flush({ code: 'auth.credenciais_invalidas' }, { status: 401, statusText: 'x' });
    const e = (await p) as ErroApi;
    expect(e.message).toBe('E-mail ou senha incorretos.');
    expect(nav).not.toHaveBeenCalled();
  });
});

import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { AuthStore } from './auth.store';

/** Protege `/app`: sem sessão, redireciona para `/entrar` preservando o destino em `returnUrl`. */
export const autenticadoGuard: CanMatchFn = (_rota, segmentos) => {
  if (inject(AuthStore).autenticado()) return true;
  const destino = '/' + segmentos.map((s) => s.path).join('/');
  return inject(Router).createUrlTree(['/entrar'], { queryParams: { returnUrl: destino } });
};

/** Impede quem já tem sessão de ver o login. */
export const visitanteGuard: CanMatchFn = () =>
  inject(AuthStore).autenticado() ? inject(Router).createUrlTree(['/app/inicio']) : true;

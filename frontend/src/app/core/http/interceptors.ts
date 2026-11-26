import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthStore } from '../auth/auth.store';
import { ErroApi } from '../erros/erro-api';

const ehApi = (url: string) => url.startsWith('/api/');

/** Envia `X-Correlation-Id` em todas as chamadas à API (rastreia a requisição nos logs). */
export const correlationInterceptor: HttpInterceptorFn = (req, next) =>
  ehApi(req.url) ? next(req.clone({ setHeaders: { 'X-Correlation-Id': crypto.randomUUID() } })) : next(req);

/** Converte erros HTTP (ProblemDetails) em `ErroApi` com mensagens pt-BR. */
export const errorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((e: unknown) =>
      throwError(() => (e instanceof HttpErrorResponse && ehApi(req.url) ? ErroApi.de(e) : e)),
    ),
  );

/** Em 401 de qualquer chamada (exceto o próprio login), limpa a sessão local e leva ao login com `returnUrl`. */
export const sessaoInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  return next(req).pipe(
    catchError((e: unknown) => {
      if (e instanceof ErroApi && e.status === 401 && !req.url.endsWith('/auth/login')) {
        auth.limpar();
        void router.navigate(['/entrar'], { queryParams: { returnUrl: router.url } });
      }
      return throwError(() => e);
    }),
  );
};

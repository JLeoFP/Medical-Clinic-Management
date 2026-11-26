import { provideHttpClient, withInterceptors, withXsrfConfiguration } from '@angular/common/http';
import {
  ApplicationConfig,
  DEFAULT_CURRENCY_CODE,
  LOCALE_ID,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  inject,
} from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { routes } from './app.routes';
import { AuthStore } from './core/auth/auth.store';
import { correlationInterceptor, errorInterceptor, sessaoInterceptor } from './core/http/interceptors';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      // A resposta passa por errorInterceptor primeiro e depois por sessaoInterceptor (ordem inversa da lista).
      withInterceptors([correlationInterceptor, sessaoInterceptor, errorInterceptor]),
      // O servidor emite o token no cookie legível; o HttpClient o devolve no cabeçalho das escritas (DT-45).
      withXsrfConfiguration({ cookieName: '__Host-XSRF-TOKEN', headerName: 'X-XSRF-TOKEN' }),
    ),
    { provide: LOCALE_ID, useValue: 'pt-BR' },
    { provide: DEFAULT_CURRENCY_CODE, useValue: 'BRL' },
    // Restaura a sessão (API-02) antes de ativar as rotas.
    provideAppInitializer(() => inject(AuthStore).restaurar()),
  ],
};

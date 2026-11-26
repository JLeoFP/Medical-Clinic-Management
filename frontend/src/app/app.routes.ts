import { Routes } from '@angular/router';
import { autenticadoGuard, visitanteGuard } from './core/auth/guards';

export const routes: Routes = [
  {
    path: 'entrar',
    canMatch: [visitanteGuard],
    loadComponent: () => import('./features/entrar/entrar').then((m) => m.Entrar),
  },
  {
    path: 'app',
    canMatch: [autenticadoGuard],
    loadComponent: () => import('./core/layout/layout-app').then((m) => m.LayoutApp),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'inicio' },
      { path: 'inicio', loadComponent: () => import('./features/inicio/inicio').then((m) => m.Inicio) },
    ],
  },
  { path: '', pathMatch: 'full', redirectTo: 'app/inicio' },
  { path: '**', redirectTo: 'app/inicio' },
];

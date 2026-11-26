import { Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { LucideLogOut } from '@lucide/angular';
import { Botao } from '../../shared/ui/botao';
import { AuthStore } from '../auth/auth.store';

/** Layout base da área interna (`/app`), carregado sob demanda. */
@Component({
  selector: 'app-layout-app',
  imports: [RouterOutlet, Botao, LucideLogOut],
  template: `
    <a href="#conteudo" class="sr-only focus:not-sr-only focus:absolute focus:m-2 focus:bg-white focus:p-2">
      Ir para o conteúdo
    </a>
    <header class="flex items-center justify-between bg-marca-700 px-4 py-3 text-white">
      <span class="text-lg font-semibold">Clínica</span>
      <div class="flex items-center gap-3">
        <span>{{ auth.usuario()?.nome }}</span>
        <app-botao variante="secundario" (clicar)="sair()">
          <svg lucideLogOut aria-hidden="true" class="size-4"></svg>
          Sair
        </app-botao>
      </div>
    </header>
    <main id="conteudo" class="mx-auto max-w-5xl p-4" tabindex="-1">
      <router-outlet />
    </main>
  `,
})
export class LayoutApp {
  protected readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  protected async sair(): Promise<void> {
    await this.auth.sair();
    await this.router.navigate(['/entrar']);
  }
}

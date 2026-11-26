import { Component, inject, input, signal } from '@angular/core';
import { email, form, FormField, FormRoot, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { LucideLogIn } from '@lucide/angular';
import { AuthStore } from '../../core/auth/auth.store';
import { ErroApi } from '../../core/erros/erro-api';
import { Aviso } from '../../shared/ui/aviso';
import { Botao } from '../../shared/ui/botao';
import { Campo } from '../../shared/ui/campo';

/** Tela de login interno (`/entrar`). */
@Component({
  selector: 'app-entrar',
  imports: [FormField, FormRoot, Campo, Botao, Aviso, LucideLogIn],
  template: `
    <main class="mx-auto mt-16 max-w-sm rounded-campo bg-superficie p-6 shadow">
      <h1 class="mb-4 text-2xl font-semibold">Entrar</h1>
      <form [formRoot]="formulario" class="flex flex-col gap-4" novalidate>
        <app-campo para="email" rotulo="E-mail" [erros]="mensagens(formulario.email().errors())">
          <input
            id="email"
            type="email"
            autocomplete="username"
            [formField]="formulario.email"
            aria-describedby="email-erro"
            class="rounded-campo border border-borda p-2"
          />
        </app-campo>
        <app-campo para="senha" rotulo="Senha" [erros]="mensagens(formulario.senha().errors())">
          <input
            id="senha"
            type="password"
            autocomplete="current-password"
            [formField]="formulario.senha"
            aria-describedby="senha-erro"
            class="rounded-campo border border-borda p-2"
          />
        </app-campo>
        <app-aviso [mensagem]="erroGeral()" />
        <app-botao tipo="submit" [carregando]="formulario().submitting()">
          <svg lucideLogIn aria-hidden="true" class="size-4"></svg>
          Entrar
        </app-botao>
      </form>
    </main>
  `,
})
export class Entrar {
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  /** Destino após o login (query param `returnUrl`, ligado por `withComponentInputBinding`). */
  readonly returnUrl = input<string | undefined>();
  protected readonly erroGeral = signal<string | null>(null);

  private readonly modelo = signal({ email: '', senha: '' });

  protected readonly formulario = form(
    this.modelo,
    (c) => {
      required(c.email, { message: 'Informe o e-mail.' });
      email(c.email, { message: 'Informe um e-mail válido.' });
      required(c.senha, { message: 'Informe a senha.' });
    },
    {
      submission: {
        action: async (campo) => {
          this.erroGeral.set(null);
          const { email: e, senha } = campo().value();
          try {
            await this.auth.entrar(e, senha);
          } catch (erro) {
            const ea = erro instanceof ErroApi ? erro : null;
            // 401 mantém o e-mail digitado: o modelo não é reiniciado.
            this.erroGeral.set(ea?.message ?? 'Ocorreu um erro inesperado. Tente novamente.');
            const porCampo = Object.entries(ea?.erros ?? {}).flatMap(([nome, msgs]) =>
              nome === 'email' || nome === 'senha'
                ? msgs.map((message) => ({ fieldTree: campo[nome], kind: 'server', message }))
                : [],
            );
            return porCampo.length > 0 ? porCampo : undefined;
          }
          const destino = this.returnUrl();
          await this.router.navigateByUrl(destino && destino.startsWith('/app') ? destino : '/app/inicio');
          return undefined;
        },
      },
    },
  );

  protected mensagens(erros: readonly { message?: string }[]): string[] {
    return erros.map((e) => e.message ?? '').filter(Boolean);
  }
}

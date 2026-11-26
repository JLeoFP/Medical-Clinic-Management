import { LiveAnnouncer } from '@angular/cdk/a11y';
import { Component, effect, inject, input } from '@angular/core';

/** Aviso visual que também é anunciado a leitores de tela via `LiveAnnouncer`. */
@Component({
  selector: 'app-aviso',
  template: `
    @if (mensagem()) {
      <div
        [class]="tipo() === 'erro' ? 'border-perigo text-perigo' : 'border-sucesso text-sucesso'"
        class="rounded-campo border bg-superficie p-3"
        [attr.role]="tipo() === 'erro' ? 'alert' : 'status'"
      >
        {{ mensagem() }}
      </div>
    }
  `,
})
export class Aviso {
  private readonly anunciador = inject(LiveAnnouncer);
  readonly mensagem = input<string | null>(null);
  readonly tipo = input<'erro' | 'sucesso'>('erro');

  constructor() {
    effect(() => {
      const m = this.mensagem();
      if (m) void this.anunciador.announce(m, this.tipo() === 'erro' ? 'assertive' : 'polite');
    });
  }
}

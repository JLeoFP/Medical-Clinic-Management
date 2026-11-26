import { Component, computed, input, output } from '@angular/core';

/** Botão base. Sempre declara `type` para não enviar formulários sem querer. */
@Component({
  selector: 'app-botao',
  template: `
    <button
      [type]="tipo()"
      [disabled]="desabilitado() || carregando()"
      [attr.aria-busy]="carregando() ? 'true' : null"
      [class]="classes()"
      (click)="clicar.emit($event)"
    >
      <ng-content />
    </button>
  `,
})
export class Botao {
  readonly tipo = input<'button' | 'submit'>('button');
  readonly variante = input<'primario' | 'secundario' | 'perigo'>('primario');
  readonly desabilitado = input(false);
  readonly carregando = input(false);
  readonly clicar = output<MouseEvent>();

  protected readonly classes = computed(() => {
    const base =
      'inline-flex items-center justify-center gap-2 rounded-campo px-4 py-2 font-medium disabled:cursor-not-allowed disabled:opacity-60';
    const cor = {
      primario: 'bg-marca-600 text-white hover:bg-marca-700',
      secundario: 'border border-borda bg-superficie text-texto hover:bg-fundo',
      perigo: 'bg-perigo text-white hover:opacity-90',
    }[this.variante()];
    return `${base} ${cor}`;
  });
}

import { Component, computed, input, output } from '@angular/core';

/** Paginação por offset (`page`, `pageSize`, `totalItems`), conforme o contrato da API (DT-39). */
@Component({
  selector: 'app-paginacao',
  template: `
    <nav aria-label="Paginação" class="flex items-center gap-3">
      <button
        type="button"
        class="rounded-campo border border-borda px-3 py-1 disabled:opacity-50"
        [disabled]="pagina() <= 1"
        (click)="mudar.emit(pagina() - 1)"
        aria-label="Página anterior"
      >
        Anterior
      </button>
      <span aria-live="polite">Página {{ pagina() }} de {{ totalPaginas() }}</span>
      <button
        type="button"
        class="rounded-campo border border-borda px-3 py-1 disabled:opacity-50"
        [disabled]="pagina() >= totalPaginas()"
        (click)="mudar.emit(pagina() + 1)"
        aria-label="Próxima página"
      >
        Próxima
      </button>
    </nav>
  `,
})
export class Paginacao {
  readonly pagina = input(1);
  readonly tamanhoPagina = input(20);
  readonly totalItens = input(0);
  readonly mudar = output<number>();
  protected readonly totalPaginas = computed(() => Math.max(1, Math.ceil(this.totalItens() / this.tamanhoPagina())));
}

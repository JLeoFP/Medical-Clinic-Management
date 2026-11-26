import { Component, computed, input } from '@angular/core';

/**
 * Rótulo + controle + mensagens de erro. O `<input>` projetado deve ter o mesmo `id` informado aqui
 * e `aria-describedby="<id>-erro"` (feito pelos formulários) para leitores de tela.
 */
@Component({
  selector: 'app-campo',
  template: `
    <div class="flex flex-col gap-1">
      <label [for]="para()" class="font-medium">{{ rotulo() }}</label>
      <ng-content />
      @if (erros().length > 0) {
        <ul [id]="para() + '-erro'" class="text-sm text-perigo" role="alert">
          @for (e of erros(); track e) {
            <li>{{ e }}</li>
          }
        </ul>
      }
    </div>
  `,
})
export class Campo {
  readonly para = input.required<string>();
  readonly rotulo = input.required<string>();
  readonly erros = input<readonly string[]>([]);
  protected readonly temErro = computed(() => this.erros().length > 0);
}

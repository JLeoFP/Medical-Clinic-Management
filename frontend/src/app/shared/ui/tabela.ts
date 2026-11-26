import { CdkTableModule } from '@angular/cdk/table';
import { Component, computed, input } from '@angular/core';

/** Coluna da tabela. */
export interface ColunaTabela {
  chave: string;
  titulo: string;
}

/** Tabela básica sobre o CDK Table, com cabeçalhos `scope="col"` e legenda acessível. */
@Component({
  selector: 'app-tabela',
  imports: [CdkTableModule],
  template: `
    <table cdk-table [dataSource]="linhas()" class="w-full border-collapse text-left" [attr.aria-label]="descricao()">
      @for (c of colunas(); track c.chave) {
        <ng-container [cdkColumnDef]="c.chave">
          <th cdk-header-cell *cdkHeaderCellDef scope="col" class="border-b border-borda p-2 font-semibold">
            {{ c.titulo }}
          </th>
          <td cdk-cell *cdkCellDef="let linha" class="border-b border-borda p-2">{{ linha[c.chave] }}</td>
        </ng-container>
      }
      <tr cdk-header-row *cdkHeaderRowDef="chaves()"></tr>
      <tr cdk-row *cdkRowDef="let linha; columns: chaves()"></tr>
    </table>
    @if (linhas().length === 0) {
      <p class="p-2 text-texto-suave">Nenhum registro encontrado.</p>
    }
  `,
})
export class Tabela {
  readonly colunas = input.required<readonly ColunaTabela[]>();
  readonly linhas = input<readonly Record<string, unknown>[]>([]);
  readonly descricao = input.required<string>();
  protected readonly chaves = computed(() => this.colunas().map((c) => c.chave));
}

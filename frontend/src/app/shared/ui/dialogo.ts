import { Dialog, DialogRef, DIALOG_DATA } from '@angular/cdk/dialog';
import { Component, inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Botao } from './botao';

/** Dados do diálogo de confirmação. */
export interface DadosConfirmacao {
  titulo: string;
  mensagem: string;
  confirmar?: string;
}

@Component({
  selector: 'app-dialogo-confirmacao',
  imports: [Botao],
  template: `
    <div
      class="w-96 max-w-full rounded-campo bg-superficie p-6 shadow-xl"
      role="alertdialog"
      aria-labelledby="dlg-titulo"
      aria-describedby="dlg-msg"
    >
      <h2 id="dlg-titulo" class="text-lg font-semibold">{{ dados.titulo }}</h2>
      <p id="dlg-msg" class="mt-2 text-texto-suave">{{ dados.mensagem }}</p>
      <div class="mt-6 flex justify-end gap-2">
        <app-botao variante="secundario" (clicar)="ref.close(false)">Cancelar</app-botao>
        <app-botao (clicar)="ref.close(true)">{{ dados.confirmar ?? 'Confirmar' }}</app-botao>
      </div>
    </div>
  `,
})
export class DialogoConfirmacao {
  protected readonly dados = inject<DadosConfirmacao>(DIALOG_DATA);
  protected readonly ref = inject<DialogRef<boolean>>(DialogRef);
}

/** Abre diálogos acessíveis (foco preso e devolvido, Esc fecha) sobre o CDK Dialog. */
@Injectable({ providedIn: 'root' })
export class DialogoService {
  private readonly dialog = inject(Dialog);

  /** Pergunta e resolve `true` se o usuário confirmar. */
  async confirmar(dados: DadosConfirmacao): Promise<boolean> {
    const ref = this.dialog.open<boolean>(DialogoConfirmacao, { data: dados, ariaModal: true });
    return (await firstValueFrom(ref.closed)) === true;
  }
}

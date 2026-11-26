import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Botao } from './botao';
import { Campo } from './campo';
import { Paginacao } from './paginacao';
import { Tabela } from './tabela';

@Component({
  imports: [Campo, Botao, Tabela, Paginacao],
  template: `
    <app-campo para="nome" rotulo="Nome" [erros]="erros()"><input id="nome" aria-describedby="nome-erro" /></app-campo>
    <app-botao>Salvar</app-botao>
    <app-tabela descricao="Pacientes" [colunas]="colunas" [linhas]="linhas" />
    <app-paginacao [pagina]="2" [tamanhoPagina]="10" [totalItens]="35" (mudar)="destino.set($event)" />
  `,
})
class Hospedeiro {
  erros = signal<string[]>([]);
  destino = signal(0);
  colunas = [{ chave: 'nome', titulo: 'Nome' }];
  linhas = [{ nome: 'Ana' }];
}

describe('shared/ui — acessibilidade básica', () => {
  it('o rótulo aponta para o campo e o erro é anunciado', async () => {
    const f = TestBed.createComponent(Hospedeiro);
    f.componentInstance.erros.set(['Obrigatório']);
    await f.whenStable();
    const el = f.nativeElement as HTMLElement;
    expect(el.querySelector('label')?.getAttribute('for')).toBe('nome');
    expect(el.querySelector('#nome-erro')?.getAttribute('role')).toBe('alert');
  });

  it('botão declara type e pode receber foco', async () => {
    const f = TestBed.createComponent(Hospedeiro);
    document.body.appendChild(f.nativeElement);
    await f.whenStable();
    const b = (f.nativeElement as HTMLElement).querySelector('app-botao button') as HTMLButtonElement;
    expect(b.type).toBe('button');
    b.focus();
    expect(document.activeElement).toBe(b);
  });

  it('tabela tem cabeçalho com scope=col e renderiza linhas', async () => {
    const f = TestBed.createComponent(Hospedeiro);
    await f.whenStable();
    const el = f.nativeElement as HTMLElement;
    expect(el.querySelector('th')?.getAttribute('scope')).toBe('col');
    expect(el.querySelector('td')?.textContent).toContain('Ana');
  });

  it('paginação calcula páginas e emite a mudança', async () => {
    const f = TestBed.createComponent(Hospedeiro);
    await f.whenStable();
    const el = f.nativeElement as HTMLElement;
    expect(el.querySelector('nav span')?.textContent).toContain('Página 2 de 4');
    (el.querySelector('button[aria-label="Próxima página"]') as HTMLButtonElement).click();
    expect(f.componentInstance.destino()).toBe(3);
  });
});

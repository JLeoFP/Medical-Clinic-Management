import { CurrencyPipe, DatePipe, registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { DEFAULT_CURRENCY_CODE, LOCALE_ID } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { calcularIdade, formatarCpf, formatarTelefone } from './pipes/formatos';
import { cpfValido, dataNaoFutura, telefoneValido } from './validacao/validadores';

registerLocaleData(localePt, 'pt-BR');

describe('formatos pt-BR', () => {
  beforeEach(() =>
    TestBed.configureTestingModule({
      providers: [
        { provide: LOCALE_ID, useValue: 'pt-BR' },
        { provide: DEFAULT_CURRENCY_CODE, useValue: 'BRL' },
        DatePipe,
        CurrencyPipe,
      ],
    }),
  );

  it('formata a data 2026-11-18 como 18/11/2026', () => {
    expect(TestBed.inject(DatePipe).transform('2026-11-18T12:00:00', 'dd/MM/yyyy')).toBe('18/11/2026');
  });

  it('formata 1234.5 como R$ 1.234,50', () => {
    expect(TestBed.inject(CurrencyPipe).transform(1234.5)?.replace(/\s/g, ' ')).toBe('R$ 1.234,50');
  });

  it('formata CPF e telefone', () => {
    expect(formatarCpf('12345678909')).toBe('123.456.789-09');
    expect(formatarTelefone('11912345678')).toBe('(11) 91234-5678');
    expect(formatarTelefone('1112345678')).toBe('(11) 1234-5678');
  });

  it('calcula a idade completa', () => {
    expect(calcularIdade('2000-11-18', new Date(2026, 10, 17))).toBe(25);
    expect(calcularIdade('2000-11-18', new Date(2026, 10, 18))).toBe(26);
    expect(calcularIdade('lixo')).toBeNull();
  });
});

describe('validadores', () => {
  it('CPF: aceita válido, rejeita dígito errado e repetidos', () => {
    expect(cpfValido('123.456.789-09')).toBe(true);
    expect(cpfValido('12345678900')).toBe(false);
    expect(cpfValido('11111111111')).toBe(false);
  });

  it('telefone: 10 ou 11 dígitos com DDD', () => {
    expect(telefoneValido('(11) 91234-5678')).toBe(true);
    expect(telefoneValido('1112345678')).toBe(true);
    expect(telefoneValido('12345')).toBe(false);
  });

  it('data não futura', () => {
    const hoje = new Date(2026, 10, 18);
    expect(dataNaoFutura('2026-11-18', hoje)).toBe(true);
    expect(dataNaoFutura('2026-11-19', hoje)).toBe(false);
    expect(dataNaoFutura('2026-02-31', hoje)).toBe(false);
  });
});

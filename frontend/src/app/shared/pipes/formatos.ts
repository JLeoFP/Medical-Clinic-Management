import { Pipe, PipeTransform } from '@angular/core';

const soDigitos = (v: string | null | undefined) => (v ?? '').replace(/\D/g, '');

/** `12345678909` → `123.456.789-09`. Valores incompletos são devolvidos sem máscara. */
export function formatarCpf(valor: string | null | undefined): string {
  const d = soDigitos(valor);
  return d.length === 11 ? d.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4') : (valor ?? '');
}

/** `11912345678` → `(11) 91234-5678`; `1112345678` → `(11) 1234-5678`. */
export function formatarTelefone(valor: string | null | undefined): string {
  const d = soDigitos(valor);
  if (d.length === 11) return d.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
  if (d.length === 10) return d.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
  return valor ?? '';
}

/** Idade completa em anos a partir de `YYYY-MM-DD` (RN-12: nunca persistida). */
export function calcularIdade(nascimento: string | null | undefined, hoje: Date = new Date()): number | null {
  const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(nascimento ?? '');
  if (!m) return null;
  const [, a, mes, dia] = m.map(Number) as [number, number, number, number];
  let idade = hoje.getFullYear() - a;
  if (hoje.getMonth() + 1 < mes || (hoje.getMonth() + 1 === mes && hoje.getDate() < dia)) idade--;
  return idade;
}

@Pipe({ name: 'cpf' })
export class CpfPipe implements PipeTransform {
  transform(valor: string | null | undefined): string {
    return formatarCpf(valor);
  }
}

@Pipe({ name: 'telefone' })
export class TelefonePipe implements PipeTransform {
  transform(valor: string | null | undefined): string {
    return formatarTelefone(valor);
  }
}

@Pipe({ name: 'idade' })
export class IdadePipe implements PipeTransform {
  transform(nascimento: string | null | undefined): string {
    const i = calcularIdade(nascimento);
    return i === null ? '' : `${i} ${i === 1 ? 'ano' : 'anos'}`;
  }
}

import { PathKind, SchemaPath, validate } from '@angular/forms/signals';

/** Valida CPF pelos dígitos verificadores (RN-09). Aceita com ou sem máscara. */
export function cpfValido(valor: string): boolean {
  const d = valor.replace(/\D/g, '');
  if (d.length !== 11 || /^(\d)\1{10}$/.test(d)) return false;
  const dv = (base: string, pesoInicial: number) => {
    const soma = [...base].reduce((s, c, i) => s + Number(c) * (pesoInicial - i), 0);
    const resto = (soma * 10) % 11;
    return resto === 10 ? 0 : resto;
  };
  return dv(d.slice(0, 9), 10) === Number(d[9]) && dv(d.slice(0, 10), 11) === Number(d[10]);
}

/** Telefone brasileiro: DDD + 8 ou 9 dígitos (10 ou 11 dígitos no total). */
export function telefoneValido(valor: string): boolean {
  return /^[1-9]{2}9?\d{8}$/.test(valor.replace(/\D/g, ''));
}

/** `YYYY-MM-DD` real e não posterior a `hoje`. */
export function dataNaoFutura(valor: string, hoje: Date = new Date()): boolean {
  const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(valor);
  if (!m) return false;
  const [a, mes, dia] = [Number(m[1]), Number(m[2]), Number(m[3])];
  const d = new Date(a, mes - 1, dia);
  if (d.getFullYear() !== a || d.getMonth() !== mes - 1 || d.getDate() !== dia) return false;
  const fimDeHoje = new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate());
  return d <= fimDeHoje;
}

type Caminho = SchemaPath<string, 1, PathKind.Child | PathKind.Root | PathKind.Item>;

/** Regra de Signal Forms: CPF válido (campo vazio é ignorado; combine com `required`). */
export function validarCpf(caminho: Caminho, mensagem = 'CPF inválido'): void {
  validate(caminho, ({ value }) =>
    value() === '' || cpfValido(value()) ? undefined : { kind: 'cpf', message: mensagem },
  );
}

/** Regra de Signal Forms: telefone válido. */
export function validarTelefone(caminho: Caminho, mensagem = 'Telefone inválido'): void {
  validate(caminho, ({ value }) =>
    value() === '' || telefoneValido(value()) ? undefined : { kind: 'telefone', message: mensagem },
  );
}

/** Regra de Signal Forms: data (`YYYY-MM-DD`) não futura. */
export function validarDataNaoFutura(caminho: Caminho, mensagem = 'A data não pode ser futura'): void {
  validate(caminho, ({ value }) =>
    value() === '' || dataNaoFutura(value()) ? undefined : { kind: 'dataFutura', message: mensagem },
  );
}

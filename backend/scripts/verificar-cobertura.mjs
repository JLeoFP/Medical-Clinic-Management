// Gate de cobertura (DT-32): falha se a cobertura de linhas de Clinica.Domain ou Clinica.Application ficar abaixo do mínimo.
// Uso: node scripts/verificar-cobertura.mjs <pasta-com-cobertura.xml> [minimo=80]
// Une todos os relatórios Cobertura da pasta: uma linha conta como coberta se qualquer suíte a executou.
import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join } from 'node:path';

const pasta = process.argv[2];
const minimo = Number(process.argv[3] ?? 80);
const alvos = ['Clinica.Domain', 'Clinica.Application'];

function* arquivos(dir) {
  for (const nome of readdirSync(dir)) {
    const caminho = join(dir, nome);
    if (statSync(caminho).isDirectory()) yield* arquivos(caminho);
    else if (nome.endsWith('.cobertura.xml') || nome.endsWith('.xml')) yield caminho;
  }
}

// alvo -> "arquivo:linha" -> máximo de execuções
const linhas = Object.fromEntries(alvos.map((a) => [a, new Map()]));
for (const arquivo of arquivos(pasta)) {
  const xml = readFileSync(arquivo, 'utf8');
  if (!xml.includes('<coverage')) continue;
  for (const pacote of xml.matchAll(/<package\b[^>]*\bname="([^"]*)"[^>]*>([\s\S]*?)<\/package>/g)) {
    const alvo = alvos.find((a) => pacote[1] === a);
    if (!alvo) continue;
    for (const classe of pacote[2].matchAll(/<class\b[^>]*\bfilename="([^"]*)"[^>]*>([\s\S]*?)<\/class>/g)) {
      for (const l of classe[2].matchAll(/<line\b[^>]*\bnumber="(\d+)"[^>]*\bhits="(\d+)"/g)) {
        const chave = `${classe[1]}:${l[1]}`;
        linhas[alvo].set(chave, Math.max(linhas[alvo].get(chave) ?? 0, Number(l[2])));
      }
    }
  }
}

let falhou = false;
for (const alvo of alvos) {
  const todas = [...linhas[alvo].values()];
  if (todas.length === 0) {
    console.error(`${alvo}: nenhuma linha encontrada nos relatórios de ${pasta}`);
    falhou = true;
    continue;
  }
  const pct = (todas.filter((h) => h > 0).length / todas.length) * 100;
  const ok = pct >= minimo;
  console.log(`${alvo}: ${pct.toFixed(1)}% (mínimo ${minimo}%) ${ok ? 'OK' : 'FALHOU'}`);
  falhou ||= !ok;
}
process.exit(falhou ? 1 : 0);

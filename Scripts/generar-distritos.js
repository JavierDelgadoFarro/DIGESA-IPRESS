const fs = require('fs');
const path = process.argv[2] || require('path').join(__dirname, '..', 'agent-tools', 'c4e4e039-6475-42e9-9874-1a3466a3864d.txt');
const outPath = require('path').join(__dirname, 'Insert_Ubigeo_Peru_Completo.sql');
let content;
try {
  content = fs.readFileSync(path, 'utf8');
} catch (e) {
  console.log('Archivo ubigeo no encontrado en:', path);
  process.exit(1);
}
const lines = content.split(/\r?\n/);
const out = [];
let inDist = false;
const re = /^\s*\('(\d{6})',\s*'([^']*)',\s*'(\d{4})',\s*'\d{2}'\)/;
for (const line of lines) {
  if (line.indexOf('ubigeo_peru_districts') >= 0) {
    inDist = true;
    continue;
  }
  if (inDist && re.test(line)) {
    const m = line.match(re);
    const ubigeo = m[1];
    const nom = m[2].replace(/'/g, "''").trim();
    const cod = m[3];
    out.push("INSERT INTO IPRESS_Distrito (Ubigeo, Nombre, CodigoProvincia) VALUES ('" + ubigeo + "', '" + nom + "', '" + cod + "');");
  }
  if (inDist && /^\s*\);\s*$/.test(line)) break;
}
const append = '\n-- =============================================\n-- DISTRITOS (' + out.length + ')\n-- =============================================\n' + out.join('\n') + '\nGO\nPRINT \'Ubigeo Peru cargado: 25 departamentos, 196 provincias, ' + out.length + ' distritos.\';\n';
fs.appendFileSync(outPath, append);
console.log('Distritos anadidos:', out.length);

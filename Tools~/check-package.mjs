// Package lint without Unity: package.json fields, asmdef JSON, .meta coverage, no secrets.
import { readFileSync, readdirSync, statSync, existsSync } from 'node:fs';
import { join, extname } from 'node:path';

let fail = 0;
const err = (m) => { console.error('ERROR ' + m); fail = 1; };
const pkg = JSON.parse(readFileSync('package.json', 'utf8'));
for (const k of ['name', 'version', 'displayName', 'unity', 'license']) if (!pkg[k]) err(`package.json missing ${k}`);
if (!/^\d+\.\d+\.\d+/.test(pkg.version)) err('package.json version is not semver');
if (pkg.license !== 'MIT') err('license must be MIT');

function walk(dir, top) {
  for (const name of readdirSync(dir)) {
    if (name.startsWith('.')) continue;
    if (top && (name === 'Tools~')) continue;
    const p = join(dir, name);
    const isDir = statSync(p).isDirectory();
    if (!name.endsWith('.meta') && !existsSync(p + '.meta')) err(`missing ${p}.meta`);
    if (!isDir) {
      const ext = extname(name);
      if (ext === '.asmdef') { try { JSON.parse(readFileSync(p, 'utf8')); } catch { err(`${p} is not valid JSON`); } }
      if (['.cs', '.json', '.md', '.mjs'].includes(ext)) {
        const text = readFileSync(p, 'utf8');
        if (/(^|[^0-9a-fA-F])0x[0-9a-fA-F]{64}([^0-9a-fA-F]|$)/.test(text) && !/Tests|Samples~|CHANGELOG/.test(p)) err(`${p} contains a 32-byte hex value (private key?)`);
        if (/\b(rk|sk|gid)_[A-Za-z0-9]{16,}/.test(text)) err(`${p} contains what looks like an API key or session token`);
      }
    }
    if (isDir) walk(p, false);
  }
}
walk('.', true);
if (fail) process.exit(1);
console.log('check-package: ok');

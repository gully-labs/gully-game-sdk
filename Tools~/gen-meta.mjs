// Creates a Unity .meta for every package file and folder that lacks one (UPM git packages must ship metas).
//   node Tools~/gen-meta.mjs
import { createHash } from 'node:crypto';
import { readdirSync, statSync, existsSync, writeFileSync } from 'node:fs';
import { join, extname } from 'node:path';

const guid = (p) => createHash('md5').update('gully-game-sdk:' + p.replace(/\\/g, '/')).digest('hex');
const importer = (file) => {
  const ext = extname(file).toLowerCase();
  if (ext === '.cs') return 'MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n';
  if (ext === '.asmdef') return 'AssemblyDefinitionImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n';
  if (['.md', '.txt', '.json', '.xml', '.yaml', '.yml', ''].includes(ext)) return 'TextScriptImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n';
  return 'DefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n';
};
let made = 0;
function walk(dir, top) {
  for (const name of readdirSync(dir)) {
    if (name.endsWith('.meta') || name.startsWith('.') || (name.endsWith('~') && !top)) continue;
    if (top && (name === 'Tools~' || name === '.github')) continue;
    const p = join(dir, name);
    const isDir = statSync(p).isDirectory();
    const meta = p + '.meta';
    if (!existsSync(meta)) {
      const body = isDir
        ? `fileFormatVersion: 2\nguid: ${guid(p)}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n`
        : `fileFormatVersion: 2\nguid: ${guid(p)}\n${importer(name)}`;
      writeFileSync(meta, body);
      made++;
    }
    if (isDir) walk(p, false);
  }
}
walk('.', true);
console.log(`gen-meta: ${made} .meta file(s) created`);

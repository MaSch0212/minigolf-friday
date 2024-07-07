import crypt from "crypto";
import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";
import { argv } from "process";

export const repoRootDir = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../..");
export const imageName = "minigolf-friday";
export const repository = "masch0212";

export function getVersionTag() {
  const dateIso = new Date().toISOString();
  return dateIso.replace(/[-T:.Z]/g, (x) => (x === "T" ? "-" : "")).substring(0, 15);
}

export function getArg(longName, shortName) {
  const longIndex = argv.indexOf(longName);
  if (longIndex > -1) {
    return argv[longIndex + 1];
  }
  const shortIndex = argv.indexOf(shortName);
  if (shortIndex > -1) {
    return argv[shortIndex + 1];
  }
  return null;
}

export async function setIndexHtmlVersion(distDir, version) {
  const indexHtmlPath = path.resolve(distDir, "index.html");
  if (!fs.existsSync(indexHtmlPath)) {
    throw new Error(`File ${indexHtmlPath} not found`);
  }

  let indexHtml = fs.readFileSync(indexHtmlPath, "utf8");
  indexHtml = indexHtml.replace(
    /<meta name="version" content(="[^"]*")?\s*(\/)?>/,
    `<meta name="version" content="${version}" \/>`
  );
  fs.writeFileSync(indexHtmlPath, indexHtml);
  await rebuildNgswFileHash(distDir, "/index.html");
}

function rebuildNgswFileHash(distDir, file) {
  const ngswPath = path.resolve(distDir, "ngsw.json");
  if (!fs.existsSync(ngswPath)) {
    console.error(`File ${ngswPath} not found`);
    process.exit(1);
  }

  const ngsw = JSON.parse(fs.readFileSync(ngswPath, "utf8"));
  if (!ngsw.hashTable[file]) {
    console.error(`File ${file} not found in ngsw.json`);
    process.exit(1);
  }

  const filePath = path.resolve(distDir, file.replace(/^\//, ""));
  if (!fs.existsSync(filePath)) {
    console.error(`File ${filePath} not found`);
    process.exit(1);
  }

  const fd = fs.createReadStream(filePath);
  const hash = crypt.createHash("sha1");
  hash.setEncoding("hex");

  return new Promise((resolve, reject) => {
    fd.on("end", () => {
      hash.end();

      try {
        console.log(`Old hash for ${file}: ${ngsw.hashTable[file]}`);
        ngsw.hashTable[file] = hash.read();
        console.log(`New hash for ${file}: ${ngsw.hashTable[file]}`);
        fs.writeFileSync(ngswPath, JSON.stringify(ngsw, null, 2));
        resolve();
      } catch (error) {
        reject(error);
      }
    });

    fd.pipe(hash);
  });
}

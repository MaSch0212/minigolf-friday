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

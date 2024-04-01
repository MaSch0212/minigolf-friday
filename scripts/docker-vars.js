import path from "path";
import { fileURLToPath } from "url";

export const repoRootDir = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
export const imageName = "minigolf-friday";
export const repository = "masch0212";

export function getVersionTag() {
  const dateIso = new Date().toISOString();
  return dateIso.replace(/[-T:.Z]/g, (x) => (x === "T" ? "-" : "")).substring(0, 15);
}

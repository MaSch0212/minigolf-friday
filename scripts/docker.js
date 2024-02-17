import yargs from "yargs";
import { hideBin } from "yargs/helpers";
import { spawnSync } from "child_process";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const imageName = "minigolf-friday";
const repository = "marc-nas:4000";

yargs(hideBin(process.argv))
  .command(
    "build",
    "Build the docker image",
    (x) => x.options({}),
    (args) => {
      const buildtime = getVersionTag();
      spawnSync(
        "docker",
        [
          "build",
          `-t=${repository}/${imageName}:latest`,
          `--build-arg=BUILDTIME=${buildtime}`,
          ".",
        ],
        { cwd: path.resolve(__dirname, ".."), stdio: "inherit" }
      );
    }
  )
  .command(
    "push",
    "Push the docker image",
    (x) => x.options({}),
    (args) => {
      const tag = getVersionTag();
      spawnSync(
        "docker",
        ["tag", `${repository}/${imageName}:latest`, `${repository}/${imageName}:${tag}`],
        { cwd: path.resolve(__dirname, ".."), stdio: "inherit" }
      );
      spawnSync("docker", ["push", `${repository}/${imageName}:${tag}`], {
        cwd: path.resolve(__dirname, ".."),
        stdio: "inherit",
      });
      spawnSync("docker", ["push", `${repository}/${imageName}:latest`], {
        cwd: path.resolve(__dirname, ".."),
        stdio: "inherit",
      });
    }
  )
  .help().argv;

function getVersionTag() {
  const dateIso = new Date().toISOString();
  return dateIso.replace(/[-T:.Z]/g, (x) => (x === "T" ? "-" : "")).substring(0, 15);
}

import { spawnSync } from "child_process";
import { repository, imageName, repoRootDir, getVersionTag } from "./vars.js";

const buildtime = getVersionTag();
spawnSync(
  "docker",
  [
    "build",
    `-t=${repository}/${imageName}:latest`,
    `-t=${repository}/${imageName}:${buildtime}`,
    `--build-arg=BUILDTIME=${buildtime}`,
    ".",
  ],
  { cwd: repoRootDir, stdio: "inherit" }
);

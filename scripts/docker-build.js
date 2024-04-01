import { spawnSync } from "child_process";
import { repository, imageName, repoRootDir, getVersionTag } from "./docker-vars.js";

const buildtime = getVersionTag();
spawnSync(
  "docker",
  ["build", `-t=${repository}/${imageName}:latest`, `--build-arg=BUILDTIME=${buildtime}`, "."],
  { cwd: repoRootDir, stdio: "inherit" }
);

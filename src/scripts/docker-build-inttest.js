import { spawnSync } from "child_process";
import { repository, imageName, repoRootDir, getArg, getVersionTag } from "./utils.js";

const configuration = getArg("--configuration", "-c") ?? "Release";

console.log(`Building integration test image with configuration ${configuration}`);

spawnSync(
  "docker",
  [
    "build",
    `-t=${repository}/${imageName}:intttest`,
    `--build-arg=CONFIGURATION=${configuration}`,
    `-f=IntegrationTests.Dockerfile`,
    ".",
  ],
  { cwd: repoRootDir, stdio: "inherit" }
);

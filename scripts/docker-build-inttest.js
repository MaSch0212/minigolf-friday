import { spawnSync } from "child_process";
import { repository, imageName, repoRootDir, getVersionTag } from "./docker-vars.js";
import { argv } from "process";

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

function getArg(longName, shortName) {
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

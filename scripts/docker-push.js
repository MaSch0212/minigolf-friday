import { spawnSync } from "child_process";
import { repository, imageName, repoRootDir, getVersionTag } from "./docker-vars.js";
import { appendFileSync } from "fs";

const tag = getVersionTag();
spawnSync(
  "docker",
  ["tag", `${repository}/${imageName}:latest`, `${repository}/${imageName}:${tag}`],
  { cwd: repoRootDir, stdio: "inherit" }
);
spawnSync("docker", ["push", `${repository}/${imageName}:${tag}`], {
  cwd: repoRootDir,
  stdio: "inherit",
});
spawnSync("docker", ["push", `${repository}/${imageName}:latest`], {
  cwd: repoRootDir,
  stdio: "inherit",
});

appendFileSync(process.env.GITHUB_OUTPUT, `docker_tag=${tag}\n`);

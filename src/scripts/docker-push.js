import { spawnSync } from "child_process";
import { repository, imageName, repoRootDir, getVersionTag } from "./vars.js";
import { appendFileSync } from "fs";

const inspectResult = spawnSync(
  "docker",
  ["inspect", "--format={{json .RepoTags}}", `${repository}/${imageName}:latest`],
  {
    cwd: repoRootDir,
    stdio: "pipe",
  }
);

if (inspectResult.status !== 0) {
  console.error(inspectResult.stdout.toString());
  console.error(inspectResult.stderr.toString());
  process.exit(1);
}

const tag = JSON.parse(inspectResult.stdout.toString())
  .map((t) => t.split(":")[1])
  .filter((t) => t && t !== "latest")
  .sort((a, b) => b.localeCompare(a))[0];

if (!tag) {
  console.error("No tags other than 'latest' found.");
  process.exit(1);
}

console.log(`Found tag to publish: ${tag}`);

spawnSync("docker", ["push", `${repository}/${imageName}:${tag}`], {
  cwd: repoRootDir,
  stdio: "inherit",
});
spawnSync("docker", ["push", `${repository}/${imageName}:latest`], {
  cwd: repoRootDir,
  stdio: "inherit",
});

appendFileSync(process.env.GITHUB_OUTPUT, `docker_tag=${tag}\n`);

import { spawnSync } from "child_process";
import { repository, imageName, repoRootDir, getVersionTag, setIndexHtmlVersion } from "./utils.js";

(async () => {
  const buildtime = getVersionTag();

  await setIndexHtmlVersion("src/client/dist/minigolf-friday/browser", buildtime);

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
})();

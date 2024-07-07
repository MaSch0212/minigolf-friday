import { spawnSync } from "child_process";
import { repoRootDir } from "./utils.js";
import path from "path";

const buildResult = spawnSync("dotnet", ["build"], {
  cwd: path.resolve(repoRootDir, "src/server/host"),
  stdio: "inherit",
});

if (buildResult.status !== 0) {
  process.exit(buildResult.status);
}

migrate("Sqlite");
migrate("MsSql");
migrate("PostgreSql");

function migrate(provider) {
  spawnSync(
    "dotnet",
    [
      "ef",
      "migrations",
      "remove",
      `--project=../migrations/${provider.toLowerCase()}`,
      "--no-build",
      "--force",
      "--",
      `--database:provider=${provider}`,
      "--database:skipmigration=true",
    ],
    { cwd: path.resolve(repoRootDir, "src/server/host"), stdio: "inherit" }
  );
}

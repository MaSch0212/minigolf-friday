import { spawnSync } from "child_process";
import { argv } from "process";
import { repoRootDir } from "./vars.js";
import path from "path";

const name = argv[2];

if (!name) {
  console.error("No migration name provided");
  process.exit(1);
}

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
      "add",
      name,
      `--project=../migrations/${provider.toLowerCase()}`,
      "--no-build",
      "--",
      `--database:provider=${provider}`,
      "--database:skipmigration=true",
    ],
    { cwd: path.resolve(repoRootDir, "src/server/host"), stdio: "inherit" }
  );
}

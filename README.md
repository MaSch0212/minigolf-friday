# MinigolfFriday

## Run in Docker

The docker container is available in Docker Hub as [masch0212/minigolf-friday:latest](https://hub.docker.com/r/masch0212/minigolf-friday).

### Example

This exaple hosts the application on port 8080 and uses the `/path/to/your/data` directory to store the data.

```bash
docker run -d \
    -e IDS__SEED=<RandomSeed> \
    -e AUTHENTICATION__JWT__SECRET=<RandomSecret> \
    -e WEBPUSH__SUBJECT=mailto:<YourEmail> \
    -e WEBPUSH__PUBLICKEY=<VapidPublicKey> \
    -e WEBPUSH__PRIVATEKEY=<VapidPrivateKey> \
    -e ADMIN__LOGINTOKEN=<AdminPassword> \
    -v /path/to/your/data:/app/data \
    -p 8080:80 \
    masch0212/minigolf-friday:latest
```

### Environment Variables

| Variable                                  | Description                                                                       | Required | Default                              |
| ----------------------------------------- | --------------------------------------------------------------------------------- | -------- | ------------------------------------ |
| `IDS__SEED`                               | The seed for the id obfuscation.                                                  | Yes      | -                                    |
| `AUTHENTICATION__JWT__SECRET`             | The secret for the JWT token.                                                     | Yes      | -                                    |
| `WEBPUSH__SUBJECT`                        | The VAPID subject for sending push notifications. Should be `mailto:<YoutEmail>`. | Yes      | -                                    |
| `WEBPUSH__PUBLICKEY`                      | The VAPID public key for sending push notifications. \*1                          | Yes      | -                                    |
| `WEBPUSH__PRIVATEKEY`                     | The VAPID private key for sending push notifications. \*1                         | Yes      | -                                    |
| `ADMIN__LOGINTOKEN`                       | The password for the admin user.                                                  | No       | `admin`                              |
| `AUTHENTICATION__JWT__ISSUER`             | The issuer for the JWT token.                                                     | No       | `/api/auth/token`                    |
| `AUTHENTICATION__JWT__AUDIENCE`           | The audience for the JWT token.                                                   | No       | `MinigolfFridayAudience`             |
| `AUTHENTICATION__JWT__EXPIRATION`         | The expiration time for the JWT token.                                            | No       | `00:15:00`                           |
| `LOGGING__LOGLEVEL__DEFAULT`              | The default log level.                                                            | No       | `Information`                        |
| `LOGGING__LOGLEVEL__MICROSOFT.ASPNETCORE` | The log level for the ASP.NET Core specific logs.                                 | No       | `Warning`                            |
| `LOGGING__ENABLEDBLOGGING`                | Enable database logging.                                                          | No       | `false`                              |
| `DATABASE__PROVIDER`                      | The database provider. Possible values: `Sqlite`, `MsSql`, `PostgreSql`           | No       | `Sqlite`                             |
| `DATABASE__SQLITECONNECTIONSTRING`        | The connection string for the SQLite database.                                    | No       | `Data Source=data/MinigolfFriday.db` |
| `DATABASE__MSSQLCONNECTIONSTRING`         | The connection string for the MS SQL database.                                    | No       | -                                    |
| `DATABASE__POSTGRESQLCONNECTIONSTRING`    | The connection string for the PostgreSQL database.                                | No       | -                                    |

\*1: The VAPID keys can be generated by running `npx web-push generate-vapid-keys` int the terminal.

## Build

### Prerequisites

- [Node.js](https://nodejs.org/en/) (minimum version: 18.17.1)
  - Enable corepack by running `corepack enable` in a terminal.
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) or [Docker Engine](https://docs.docker.com/engine/install/)
- [Visual Studio Code](https://code.visualstudio.com/) (optional)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [C# Dev Kit for VS Code](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) (optional)

### Preparation

1. Create VAPID keys for the push notifications (run all command in the root directory).
   - Run `npx web-push generate-vapid-keys`
   - Run `dotnet user-secrets set WebPush:Subject mailto:<YourEmail> -p src/server/host`
   - Run `dotnet user-secrets set WebPush:PublicKey <PublicKey> -p src/server/host`
   - Run `dotnet user-secrets set WebPush:PrivateKey <PrivateKey> -p src/server/host`
2. Run `pnpm install` in the root directory to install all dependencies.
3. Run `dotnet dev-certs https --trust` to trust the development certificate.

### Run

1. Run `dotnet watch run` in the `src/server/host` directory to start the server.
2. Run `pnpm run start` in the `src/client` directory to start the client.

or

1. Run the `🚀 Start` task in Visual Studio Code.

Now you can visit [`https://localhost:5001`](https://localhost:5001) in your browser.
The swagger UI is available at [`https://localhost:5001/swagger/index.html`](https://localhost:5001/swagger/index.html).

#### With Service Worker

If you want to test the service worker locally, follow these steps:

1. Run `dotnet watch run` in the `src/server/host` directory to start the server.
2. Run `pnpm run build` in the `src/client` directory to build the client.
3. Run `pnpm run start:sw` in the `src/client` directory to start the client with the service worker.

or

1. Run the `🚀 Start Server (watch)` task in Visual Studio Code.
2. Run the `🚀 Start Client (with Service Worker)` task in Visual Studio Code.
   - If you change the client, you need to rerun this task.

### Build

1. Run `pnpm build` in the root directory to build the server, client and docker container.
2. The image `masch0212/minigolf-friday:latest` is now available in your local docker registry.

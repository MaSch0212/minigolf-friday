# MinigolfFriday

## Run in Docker

The docker container is available in Docker Hub as [masch0212/minigolf-friday:latest](https://hub.docker.com/r/masch0212/minigolf-friday).

### Example

This exaple hosts the application on port 8080 and uses the `/path/to/your/data` directory to store the data.

```bash
docker run -d \
    -e IDS__SEED=<RandomSeed> \
    -e AUTHENTICATION__JWT__SECRET=<RandomSecret> \
    -e ADMIN__LOGINTOKEN=<AdminPassword> \
    -v /path/to/your/data:/app/data \
    -p 8080:80 \
    masch0212/minigolf-friday:latest
```

### Environment Variables

| Variable                                  | Description                                       | Required | Default                  |
| ----------------------------------------- | ------------------------------------------------- | -------- | ------------------------ |
| `IDS__SEED`                               | The seed for the id obfuscation.                  | Yes      | -                        |
| `AUTHENTICATION__JWT__SECRET`             | The secret for the JWT token.                     | Yes      | -                        |
| `ADMIN__LOGINTOKEN`                       | The password for the admin user.                  | No       | `admin`                  |
| `AUTHENTICATION__JWT__ISSUER`             | The issuer for the JWT token.                     | No       | `/api/auth/token`        |
| `AUTHENTICATION__JWT__AUDIENCE`           | The audience for the JWT token.                   | No       | `MinigolfFridayAudience` |
| `AUTHENTICATION__JWT__EXPIRATION`         | The expiration time for the JWT token.            | No       | `00:15:00`               |
| `LOGGING__LOGLEVEL__DEFAULT`              | The default log level.                            | No       | `Information`            |
| `LOGGING__LOGLEVEL__MICROSOFT.ASPNETCORE` | The log level for the ASP.NET Core specific logs. | No       | `Warning`                |
| `LOGGING__ENABLEDBLOGGING`                | Enable database logging.                          | No       | `false`                  |

## Build

### Prerequisites

- [Node.js](https://nodejs.org/en/) (minimum version: 18.17.1)
  - Enable corepack: `corepack enable`
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) or [Docker Engine](https://docs.docker.com/engine/install/)
- [Visual Studio Code](https://code.visualstudio.com/) (optional)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [C# Dev Kit for VS Code](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) (optional)

### Preparation

1. Run `pnpm install` in the root directory to install all dependencies.
2. Run `dotnet dev-certs https --trust` to trust the development certificate.

### Run

1. Run `dotnet watch run` in the `server` directory to start the server.
2. Run `pnpm run start` in the `client` directory to start the client.

or

1. Run the `🚀 Start` task in Visual Studio Code.

Now you can visit [`https://localhost:5001`](https://localhost:5001) in your browser.
The swagger UI is available at [`https://localhost:5001/swagger/index.html`](https://localhost:5001/swagger/index.html).

### Build

1. Run `pnpm build` in the root directory to build the server, client and docker container.
2. The image `masch0212/minigolf-friday:latest` is now available in your local docker registry.

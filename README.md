# MinigolfFriday

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

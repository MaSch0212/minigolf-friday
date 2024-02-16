FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV ADMIN__USERNAME=admin
ENV ADMIN__PASSWORD=admin
ENV AUTHENTICATION__FACEBOOK__APPSECRET=<secret>
ENV AUTHENTICATION__FACEBOOK__APPID=<appid>
ENV AUTHENTICATION__JWT__SECRET=<guid>

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-server
WORKDIR /src
COPY server .
RUN dotnet publish -c Release -o /app/publish

FROM node:20-slim AS pre-build-client
RUN corepack enable
WORKDIR /src
COPY client/package.json client/pnpm-lock.yaml ./
RUN pnpm install

FROM pre-build-client AS build-client
COPY client .
RUN pnpm build --output-path /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build-server /app/publish .
COPY --from=build-client /app/publish ./wwwroot/
ENTRYPOINT ["dotnet", "MinigolfFriday.dll"]

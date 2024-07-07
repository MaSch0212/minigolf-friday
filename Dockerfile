FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

ARG BUILDTIME=Unknown

# Install the ICU package
RUN apk update
RUN apk add --no-cache icu-libs
RUN apk add --no-cache icu-data-full

WORKDIR /app
EXPOSE 80

ENV BUILDTIME=$BUILDTIME
ENV ASPNETCORE_URLS=http://+:80
ENV ADMIN__LOGINTOKEN=admin
ENV AUTHENTICATION__JWT__SECRET=
ENV IDS__SEED=
ENV DATABASE__PROVIDER=Sqlite
ENV DATABASE__SQLITECONNECTIONSTRING="Data Source=data/MinigolfFriday.db"
ENV DATABASE__MSSQLCONNECTIONSTRING=
ENV DATABASE__POSTGRESQLCONNECTIONSTRING=
ENV WEBPUSH__SUBJECT=
ENV WEBPUSH__PUBLICKEY=
ENV WEBPUSH__PRIVATEKEY=

COPY src/server/host/bin/Release/publish .
COPY src/client/dist/minigolf-friday/browser ./wwwroot/

HEALTHCHECK CMD wget --no-verbose --tries=1 --spider http://localhost/healthz || exit 1

ENTRYPOINT ["dotnet", "MinigolfFriday.Host.dll"]

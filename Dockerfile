FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

ARG BUILDTIME=Unknown

WORKDIR /app
EXPOSE 80

ENV BUILDTIME=$BUILDTIME
ENV ASPNETCORE_URLS=http://+:80
ENV ADMIN__LOGINTOKEN=admin
ENV AUTHENTICATION__JWT__SECRET=
ENV IDS__SEED=

COPY server/bin/Release/publish .
COPY client/dist/minigolf-friday/browser ./wwwroot/
RUN sed -i "s/\$VERSION/$BUILDTIME/g" wwwroot/index.html

HEALTHCHECK CMD wget --no-verbose --tries=1 --spider http://localhost/healthz || exit 1

ENTRYPOINT ["dotnet", "MinigolfFriday.dll"]

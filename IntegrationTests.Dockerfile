FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

ARG CONFIGURATION=Release

WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80
ENV ADMIN__LOGINTOKEN=admin
ENV AUTHENTICATION__JWT__SECRET=1781c798-d076-5b3f-8002-9aefab283e29
ENV AUTHENTICATION__JWT__EXPIRATION=365.00:00:00
ENV IDS__SEED=inttest
ENV ENABLE_DEV_ENDPOINTS=true

COPY server/bin/$CONFIGURATION .
RUN rm -rf ./data

HEALTHCHECK CMD wget --no-verbose --tries=1 --spider http://localhost/healthz || exit 1

ENTRYPOINT ["dotnet", "MinigolfFriday.dll"]

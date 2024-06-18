FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

ARG CONFIGURATION=Release

# Install the ICU package
RUN apk update
RUN apk add --no-cache icu-libs
RUN apk add --no-cache icu-data-full

WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80
ENV ADMIN__LOGINTOKEN=admin
ENV AUTHENTICATION__JWT__SECRET=1781c798-d076-5b3f-8002-9aefab283e29
ENV AUTHENTICATION__JWT__EXPIRATION=365.00:00:00
ENV IDS__SEED=inttest
ENV ENABLE_DEV_ENDPOINTS=true
ENV WEBPUSH__SUBJECT=mailto:example@example.com
ENV WEBPUSH__PUBLICKEY=BKJZDFC-2bfX_haLCCzYKt6wljyeGvW5mrB_I_MEvl0m8UgjK6RgEMvSHWMQriHWZacrT5_jQgp-vEMIerV9B8E
ENV WEBPUSH__PRIVATEKEY=pWA_vgcrOoZrwGbGjLn56weUQXuTpE_bz_7dTS0NgU0

COPY src/server/host/bin/$CONFIGURATION .
RUN rm -rf ./data

HEALTHCHECK CMD wget --no-verbose --tries=1 --spider http://localhost/healthz || exit 1

ENTRYPOINT ["dotnet", "MinigolfFriday.Host.dll"]

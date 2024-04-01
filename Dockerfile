FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
EXPOSE 80
ARG BUILDTIME=Unknown
ENV BUILDTIME=$BUILDTIME
ENV ASPNETCORE_URLS=http://+:80
ENV ADMIN__USERNAME=admin
ENV ADMIN__PASSWORD=admin
ENV AUTHENTICATION__FACEBOOK__APPSECRET=<secret>
ENV AUTHENTICATION__FACEBOOK__APPID=<appid>
ENV AUTHENTICATION__JWT__SECRET=<guid>
COPY server/bin/Release/publish .
COPY client/dist/minigolf-friday/browser ./wwwroot/
ENTRYPOINT ["dotnet", "MinigolfFriday.dll"]

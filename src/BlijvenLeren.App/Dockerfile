FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BlijvenLeren.sln ./
COPY src/BlijvenLeren.App/BlijvenLeren.App.csproj src/BlijvenLeren.App/
RUN dotnet restore BlijvenLeren.sln

COPY src/BlijvenLeren.App/. ./src/BlijvenLeren.App/
RUN dotnet publish src/BlijvenLeren.App/BlijvenLeren.App.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BlijvenLeren.App.dll"]

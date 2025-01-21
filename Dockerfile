# syntax=docker/dockerfile:experimental

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /Source

COPY . .
RUN dotnet restore
WORKDIR /Source/AsM/
RUN dotnet publish -c Debug -o /App --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App

COPY --from=build /App ./

EXPOSE 5000 5001
ENTRYPOINT ["dotnet", "AsM.dll"]

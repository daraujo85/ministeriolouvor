#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MinisterioLouvor.csproj", ""]
RUN dotnet restore "./MinisterioLouvor.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "MinisterioLouvor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MinisterioLouvor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN useradd -m myappuser
USER myappuser

CMD ASPNETCORE_URLS="http://*:$PORT" dotnet MinisterioLouvor.dll

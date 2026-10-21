# Build stage: restaura e publica a aplicação com o SDK completo
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Restaura as dependências em camada separada para aproveitar o cache do Docker
COPY NanoLink.Api.csproj .
RUN dotnet restore NanoLink.Api.csproj

COPY . .
RUN dotnet publish NanoLink.Api.csproj -c Release -o /app/publish

# Runtime stage: apenas o ASP.NET Core Runtime, sem o SDK
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .

# Usuário não-root definido pela imagem base (APP_UID)
USER $APP_UID

ENTRYPOINT ["dotnet", "NanoLink.Api.dll"]

# Planejamento Técnico: Fase 01

## Estrutura Inicial
/
  /src
    /NanoLink.Api
      NanoLink.Api.csproj
      Program.cs
      Dockerfile
  NanoLink.sln
  docker-compose.yml
  .dockerignore
  .gitignore

## Imagens Base Docker
- **Build**: `mcr.microsoft.com/dotnet/sdk:8.0` (Alpine ou Ubuntu, preferencialmente Alpine para menor superfície de ataque e tamanho).
- **Runtime**: `mcr.microsoft.com/dotnet/aspnet:8.0-alpine`

## Docker Compose
- Definição de volumes nomeados para os dados do Postgres (garantindo persistência entre as execuções locais).
- Uso de `depends_on` (ou *healthchecks*) para garantir que a API aguarde o banco de dados e o cache subirem.

## Variáveis de Ambiente
- `ConnectionStrings__Postgres`: String de conexão direcionada ao contêiner do DB.
- `ConnectionStrings__Redis`: String de conexão direcionada ao contêiner do Cache.
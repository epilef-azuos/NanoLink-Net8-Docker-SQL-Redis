# Planeamento Técnico: Fase 02

## Estrutura de Diretórios (VSA)
/src/NanoLink.Api
  /Data
    AppDbContext.cs
    /Entities
      Link.cs
  /Features
    /ShortenUrl
      ShortenUrlEndpoint.cs
      ShortenUrlRequest.cs
      ShortenUrlResponse.cs
      ShortenUrlHandler.cs
      ShortenUrlValidator.cs
  /Common
    Base62Generator.cs

## Modelação de Dados (PostgreSQL)
Tabela: `Links`
- `Id`: `Guid` (Chave Primária)
- `Code`: `Varchar(10)` (Índice Único)
- `OriginalUrl`: `Varchar(2048)` (Obrigatório)
- `CreatedAt`: `Timestamp with time zone` (Obrigatório, UTC)

## Dependências NuGet
- `Npgsql.EntityFrameworkCore.PostgreSQL` (v9.*)
- `Microsoft.EntityFrameworkCore.Design` (v9.*)
- `FluentValidation.DependencyInjectionExtensions` (v11.*)
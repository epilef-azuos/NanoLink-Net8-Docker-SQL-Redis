# Planejamento Técnico: Fase 05

## Estrutura de Diretórios (VSA)
/src/NanoLink.Api
  /Data
    /Entities
      ClickEvent.cs (atualizado com novas colunas)
  /Infrastructure
    /Enrichment
      IAnalyticsEnricher.cs
      AnalyticsEnricher.cs
      /Geo
        IGeoLocationResolver.cs
        MaxMindGeoLocationResolver.cs
  /Resources
    GeoLite2-City.mmdb (ou base de fallback leve)

## Pacotes NuGet Necessários
- `MaxMind.GeoIP2` (Leitor oficial de alta performance para arquivos .mmdb)
- `UAParser` (Parser determinístico e leve de User-Agent)

## Alterações no Schema (Tabela `ClickEvents`)
Novas colunas:
- `CountryCode`: `Varchar(2)` (Nullável)
- `CountryName`: `Varchar(100)` (Nullável)
- `CityName`: `Varchar(100)` (Nullável)
- `OperatingSystem`: `Varchar(50)` (Nullável)
- `DeviceType`: `Varchar(30)` (Nullável)
- `ReferrerDomain`: `Varchar(150)` (Nullável)
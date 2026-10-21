# Planejamento Técnico: Fase 06

## Estrutura de Diretórios (VSA)
/src/NanoLink.Api
  /Features
    /GetAnalytics
      GetAnalyticsEndpoint.cs
      GetAnalyticsResponse.cs
      GetAnalyticsHandler.cs

## Agregações no PostgreSQL
- As agregações devem usar `GroupBy` com `.Select()` diretamente traduzidos para SQL:
  - Total geral: `.CountAsync()`
  - Agrupamentos: `GroupBy(...) -> Count()` ordenados por contagem decrescente com `.Take(10)`.
  - Agrupamento temporal: filtrado por `ClickedAt >= DateTimeOffset.UtcNow.AddDays(-7)`.

## Configuração do Rate Limiter
- Utilizar `Microsoft.AspNetCore.RateLimiting` nativo do ASP.NET Core (.NET 8/9).
- Configuração de particionador baseado no IP da conexão (`RemoteIpAddress`).
- Resposta customizada com `OnRejected` definindo status 429 e `ProblemDetails`.
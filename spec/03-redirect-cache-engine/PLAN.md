# Planejamento Técnico: Fase 03

## Estrutura de Diretórios (VSA)
/src/NanoLink.Api
  /Features
    /RedirectUrl
      RedirectUrlEndpoint.cs
      RedirectUrlHandler.cs
  /Infrastructure
    /Cache
      ICacheService.cs
      RedisCacheService.cs

## Pacotes NuGet Necessários
- `StackExchange.Redis` (Driver de alta performance para Redis com multiplexação de conexões)

## Estratégia de Chaves e TTLs no Redis
- Prefixo de Link Ativo: `link:{code}` -> `string` (OriginalUrl) | TTL: 86400s + Jitter (0-1800s)
- Prefixo de Link Inexistente: `link:{code}` -> `__NOT_FOUND__` | TTL: 60s

## Decisões de Design de Performance
- Injeção de `IConnectionMultiplexer` como Singleton para evitar esgotamento de sockets.
- Armazenamento em strings brutas no Redis (`StringSetAsync`/`StringGetAsync`), sem overhead de serialização/deserialização JSON no caminho crítico do redirecionamento.
- Consultas EF Core usando `.AsNoTracking()` e `.Select(l => l.OriginalUrl)` para evitar tracking de entidades em memória.
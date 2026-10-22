# 0006. Testes de integração com Testcontainers

- **Status:** Aceita
- **Data:** 2026-10-21
- **Decisores:** Time NanoLink

## Contexto

O NanoLink depende de três comportamentos que não podem ser simulados com fidelidade: as consultas SQL do EF Core traduzidas para PostgreSQL, o comportamento do Redis com TTL e a semântica do pipeline completo, do redirect ao flush do worker. Um erro de tradução de `GroupBy` ou de tipo de coluna só aparece com o banco real.

Além disso, os testes precisam ser determinísticos. Um teste que depende de um PostgreSQL instalado na máquina do desenvolvedor, ou de um Redis com estado residual de outra execução, falha de forma intermitente e não roda no CI.

Durante a implementação, um bug real só apareceu contra o banco de verdade: a tradução de `DateTimeOffset.UtcDateTime.Date` em `GroupBy` do analytics, que quebrava a materialização do resultado com `No coercion operator defined`. Um teste com mocks ou um banco em memória teria aprovado essa consulta.

## Opções consideradas

1. **Banco em memória** (EF Core InMemory ou SQLite em memória).
2. **Mocks** de `DbContext` e `IConnectionMultiplexer`.
3. **Instâncias reais de PostgreSQL e Redis** em contêineres efêmeros, via Testcontainers, com `WebApplicationFactory<Program>`.

## Decisão

Os testes de integração usam **Testcontainers** para subir PostgreSQL 16 (`postgres:16-alpine`) e Redis 7 (`redis:7-alpine`) como contêineres efêmeros, a cada execução. A fixture `NanoLinkApiFactory` herda de `WebApplicationFactory<Program>` e implementa `IAsyncLifetime`:

- `InitializeAsync` inicia os dois contêineres antes de qualquer teste.
- `ConfigureWebHost` substitui as connection strings `ConnectionStrings:Postgres` e `ConnectionStrings:Redis` pelos endereços gerados dinamicamente para cada contêiner.
- As migrações são aplicadas pelo próprio `Program` no startup do host de teste.
- `DisposeAsync` encerra os contêineres.

A CONSTITUTION da fase 07 proíbe explicitamente mocks ou bancos em memória para infraestrutura nos testes de integração.

## Consequências

### Positivas

- **Fidelidade:** o SQL gerado, os tipos de coluna, os índices únicos e o comportamento de TTL do Redis são os de produção.
- **Determinismo:** cada execução parte de contêineres limpos. Não há dependência de estado da máquina do desenvolvedor.
- **Ausência de instalação local:** basta ter Docker. O CI roda da mesma forma.
- **Detecção de bugs reais:** os dois erros citados acima foram encontrados por esta suíte.

### Negativas

- **Dependência do Docker:** sem Docker em execução, os testes falham. O erro é claro, mas exige o ambiente.
- **Tempo de execução:** subir contêineres e aplicar migrações adiciona segundos à suíte. A fixture compartilhada, com `IClassFixture`, limita esse custo a uma subida por classe de testes.
- **Compatibilidade de versão do Docker:** a versão de `Testcontainers` 4.16 negocia a API Docker 1.44. Em um Docker Engine 20.10 local, a conexão falha com uma mensagem genérica. A mitigação está documentada no [ARCHITECTURE.md](../../ARCHITECTURE.md#diagnósticos-trade-offs-e-lições-aprendidas).
- **Estado compartilhado na fixture:** todos os testes de uma classe usam o mesmo banco. A suíte compensa isso com dados únicos por teste, mas o isolamento é uma convenção a ser mantida.
- **Limite de rate limit compartilhado:** como todos os testes usam a mesma partição `anonymous` do rate limiter, o número de POSTs da suíte precisa ficar abaixo do limite.

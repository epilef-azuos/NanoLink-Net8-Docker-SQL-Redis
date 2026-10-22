# NanoLink 🔗⚡

[![CI](https://github.com/epilef-azuos/NanoLink-Net8-Docker-SQL-Redis/actions/workflows/ci.yml/badge.svg)](https://github.com/epilef-azuos/NanoLink-Net8-Docker-SQL-Redis/actions/workflows/ci.yml)
![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)
![PostgreSQL 16](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Redis 7](https://img.shields.io/badge/Redis-7-DC382D?logo=redis&logoColor=white)
![Scalar](https://img.shields.io/badge/API%20Docs-Scalar-7C3AED)

Encurtador de URLs resiliente, desenhado para **latência inferior a 5 ms no caminho crítico** de redirecionamento e para **ingestão analítica assíncrona e desacoplada**: o clique nunca bloqueia o usuário, e a gravação em banco acontece em lote, em segundo plano.

---

## Visão Geral do Produto

O NanoLink recebe uma URL longa, devolve um código Base62 de 7 caracteres e redireciona quem acessa esse código com `302 Found`. Para cada clique, registra metadados (IP, User-Agent, Referer) e os enriquece **offline** com país, cidade, sistema operacional, tipo de dispositivo e domínio de origem. Um endpoint de analytics agrega essas métricas por link.

## Destaques Arquiteturais

- **Vertical Slice Architecture (VSA):** cada funcionalidade vive em `Features/<Nome>/` com seus próprios request, response, validator, handler e endpoint. Não existem pastas genéricas `Services/` ou `Controllers/` ([ADR 0001](docs/adr/0001-vertical-slice-architecture.md)).
- **Cache Redis com Read-Through, Jitter anti-stampede e Negative Caching anti-penetração:** links válidos ficam 24 h mais um jitter de até 30 min; códigos inexistentes são gravados com a sentinela `__NOT_FOUND__` por 60 s ([ADR 0004](docs/adr/0004-redis-negative-caching-and-jitter.md)).
- **Pipeline de analytics via Write-Behind Pattern:** `System.Threading.Channels` com capacidade de 50.000 posições e descarte `DropOldest`, consumido por um `BackgroundService` que grava em lotes de 100 itens ou a cada 5 segundos ([ADR 0003](docs/adr/0003-write-behind-pattern-via-channels.md)).
- **Geolocalização e metadados 100% offline:** leitor MaxMind MMDB em memória para país e cidade, e UAParser para SO, dispositivo e Referer. Nenhuma chamada HTTP a terceiros ([ADR 0005](docs/adr/0005-offline-metadata-and-geolocation-enrichment.md)).
- **Identificadores criptograficamente seguros:** códigos Base62 gerados com `RandomNumberGenerator`, sem previsibilidade sequencial ([ADR 0002](docs/adr/0002-base62-cryptographic-encoding.md)).
- **Rate Limiting nativo do .NET 9:** `System.Threading.RateLimiting` com janela fixa de 10 criações por minuto por IP, respondendo `429` com `Retry-After`.
- **Documentação interativa moderna:** OpenAPI nativo do ASP.NET Core com a UI do Scalar em `/scalar/v1`, em ambiente de desenvolvimento ([ADR 0007](docs/adr/0007-scalar-modern-api-reference.md)).
- **Testes de integração determinísticos:** Testcontainers sobe PostgreSQL e Redis reais, efêmeros, a cada execução ([ADR 0006](docs/adr/0006-testcontainers-for-integration-testing.md)).

## Diagrama de Fluxo

Dois caminhos saem da mesma requisição. O **Fast Path** responde ao usuário e termina. O **Async Path** roda depois, em segundo plano, sem que o usuário espere por ele.

```mermaid
flowchart LR
    C([Cliente]) -->|GET /{code}| API[ASP.NET Core API]

    subgraph FAST["Fast Path (caminho crítico)"]
        direction LR
        API -->|1. StringGet| R[(Redis)]
        R -->|hit: URL bruta| API
        R -.->|miss| PG[(PostgreSQL)]
        PG -.->|URL + grava cache| R
        API -->|2. 302 Found| C
    end

    subgraph ASYNC["Async Path (segundo plano)"]
        direction LR
        API -->|3. TryWrite, não bloqueante| CH{{Bounded Channel<br/>50.000 · DropOldest}}
        CH -->|lote de 100 ou 5 s| W[Batch Worker]
        W -->|enriquece offline| GEO[(MaxMind MMDB)]
        W -->|AddRangeAsync + SaveChanges| PG2[(PostgreSQL<br/>ClickEvents)]
    end
```

## Quickstart

### Pré-requisitos

- Docker Desktop (com Docker Compose)
- .NET SDK 9.0

### 1. Subida completa via Docker Compose

```bash
docker compose up --build -d
```

Sobe três serviços: a API em `http://localhost:8080`, o PostgreSQL 16 publicado na porta **5433** do host e o Redis 7 na porta **6379**. As migrações do EF Core são aplicadas automaticamente no startup da API.

> No container a API roda em `Production`, então a documentação Scalar não é exposta ali. Para ver a UI, use a execução local abaixo.

### 2. Execução local com hot reload

Suba apenas a infraestrutura e rode a API no host com `dotnet watch`:

```bash
docker compose up -d postgres redis
dotnet watch --project src/NanoLink.Api
```

A API sobe em `http://localhost:5286` no ambiente `Development`. Acesse a documentação interativa em **`http://localhost:5286/scalar/v1`**. A connection string de desenvolvimento aponta para `localhost:5433` (PostgreSQL) e `localhost:6379` (Redis).

### 3. Testes de integração

```bash
dotnet test
```

Os testes sobem PostgreSQL e Redis em contêineres efêmeros via Testcontainers. Não é preciso ter banco ou Redis instalados na máquina. É necessário Docker em execução.

> Se o seu Docker Engine for mais antigo que a API 1.44 (ex.: Docker 20.10), defina `DOCKER_API_VERSION=1.41` antes de rodar os testes. Veja [ARCHITECTURE.md](ARCHITECTURE.md#diagnósticos-trade-offs-e-lições-aprendidas).

## Endpoints

| Método | Rota | Descrição | Payload de exemplo | Respostas |
|---|---|---|---|---|
| `POST` | `/api/links` | Cria um link encurtado | `{ "originalUrl": "https://www.exemplo.com/artigo" }` | `201 Created` com `Location: /api/links/{code}`; `400 Bad Request` (validação, ProblemDetails); `429 Too Many Requests` com `Retry-After` |
| `GET` | `/{code}` | Redireciona para a URL original | — | `302 Found` com `Location`; `400 Bad Request` (código inválido); `404 Not Found` |
| `GET` | `/api/links/{code}/analytics` | Métricas agregadas do link | — | `200 OK`; `404 Not Found` |
| `GET` | `/` | Status da API | — | `200 OK` |
| `GET` | `/scalar/v1` | Documentação interativa (Development) | — | `200 OK` |
| `GET` | `/openapi/v1.json` | Documento OpenAPI (Development) | — | `200 OK` |

### Exemplos de payload

**Criação (201):**

```json
{
  "code": "7F3bWcG",
  "shortUrl": "http://localhost:8080/7F3bWcG",
  "originalUrl": "https://www.exemplo.com/artigo",
  "createdAt": "2026-10-21T22:04:31.867026+00:00"
}
```

**Validação (400):**

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "OriginalUrl": ["O campo 'originalUrl' deve ser uma URL absoluta com esquema http ou https."]
  }
}
```

**Analytics (200), resumido:**

```json
{
  "code": "7F3bWcG",
  "originalUrl": "https://www.exemplo.com/artigo",
  "createdAt": "2026-10-21T22:04:31.867026+00:00",
  "totalClicks": 12,
  "byCountry": [{ "key": "Unknown", "label": "Unknown", "count": 5 }],
  "byDevice": [{ "key": "Desktop", "label": "Desktop", "count": 8 }],
  "byOperatingSystem": [{ "key": "Windows", "label": "Windows", "count": 3 }],
  "byReferrer": [{ "key": "t.co", "label": "t.co", "count": 2 }],
  "clicksOverTime": [{ "date": "2026-10-21", "count": 12 }]
}
```

Um link sem cliques retorna `totalClicks: 0` e listas vazias, sem erro.

## Documentação adicional

- [ARCHITECTURE.md](ARCHITECTURE.md): deep-dive de arquitetura, caminho crítico, worker, diagnósticos e trade-offs.
- [docs/adr/](docs/adr/): Architectural Decision Records no formato MADR.

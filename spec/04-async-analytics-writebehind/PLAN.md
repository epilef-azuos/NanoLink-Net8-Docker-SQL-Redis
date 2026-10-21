# Planejamento Técnico: Fase 04

## Estrutura de Diretórios (VSA)
/src/NanoLink.Api
  /Data
    /Entities
      ClickEvent.cs
  /Infrastructure
    /Analytics
      ClickEventChannel.cs
      AnalyticsBackgroundQueue.cs
      AnalyticsBatchWorker.cs

## Modelação de Dados (PostgreSQL)
Tabela: `ClickEvents`
- `Id`: `Guid` (PK)
- `Code`: `Varchar(10)` (Indexado para agregação rápida)
- `ClickedAt`: `Timestamp with time zone` (UTC)
- `UserAgent`: `Text` (Nullável)
- `Referer`: `Text` (Nullável)
- `IpAddress`: `Varchar(45)` (IPv4 ou IPv6, Nullável)

## Arquitetura do Write-Behind com Channels
- `ClickEventChannel`: Singleton encapsulando `Channel<ClickEvent>`.
  - Configuração: `Channel.CreateBounded<ClickEvent>(new BoundedChannelOptions(50_000) { FullMode = BoundedChannelFullMode.DropOldest })`.
- `AnalyticsBatchWorker`: `BackgroundService` consumidor.
  - Consome mensagens com `channel.Reader.WaitToReadAsync()`.
  - Agrupa até 100 itens ou dispara flush a cada 5 segundos.
  - Usa `IServiceScopeFactory` para criar escopos e obter instâncias de `AppDbContext` pontuais por lote de escrita.
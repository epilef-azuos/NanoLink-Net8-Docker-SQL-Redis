# 0003. Write-Behind via Bounded Channels com flush em lote

- **Status:** Aceita
- **Data:** 2026-10-21
- **Decisores:** Time NanoLink

## Contexto

Cada redirecionamento gera um clique que precisa ser registrado para analytics. Gravar esse clique no PostgreSQL de forma síncrona, dentro do caminho do `GET /{code}`, adicionaria uma escrita de banco à latência de cada redirect e acoplaria a disponibilidade do redirecionamento à do banco de analytics. Um pico de cliques multiplicaria as escritas, e cada escrita individual é cara comparada ao custo de uma inserção em lote.

O requisito é latência inferior a 5 ms no caminho crítico, sem perder o registro dos cliques em uso normal.

## Opções consideradas

1. **Escrita síncrona** no próprio endpoint de redirect.
2. **Fila externa** com broker (Kafka, RabbitMQ).
3. **Write-behind em memória**: um `Channel<T>` limitado, consumido por um `BackgroundService` que grava em lotes.

## Decisão

Adotamos **write-behind em memória** com `System.Threading.Channels`:

- `ClickEventChannel` é um singleton que encapsula um `Channel<ClickEvent>` com `BoundedChannelOptions(50_000)`, `FullMode = DropOldest`, `SingleWriter = false` e `SingleReader = true`.
- O endpoint de redirect chama `TryWrite`, que não bloqueia.
- `AnalyticsBatchWorker`, um `BackgroundService`, consome o canal e grava em lote quando o buffer atinge **100 itens** ou passaram **5 segundos** desde o primeiro item.
- Cada lote é gravado em um escopo novo, com `AddRangeAsync` e `SaveChangesAsync`.

Escolhemos `DropOldest` sobre a alternativa de bloquear o produtor porque o caminho do redirect não pode esperar. Dar prioridade ao presente em um pico também faz sentido para analytics: o dado recente é o mais valioso.

## Consequências

### Positivas

- **Latência desacoplada:** o redirect nunca espera pelo banco de analytics. A escrita enfileirada custa uma operação de memória.
- **Memória limitada:** a capacidade fixa impede crescimento ilimitado sob ataque. Uma fila sem limite causaria `OutOfMemoryException`.
- **Eficiência no banco:** inserções em lote reduzem round-trips. Um lote de 100 faz uma ida ao banco em vez de 100.
- **Sem infraestrutura adicional:** nenhum broker a operar, monitorar ou pagar na v1.
- **Desligamento gracioso:** `Complete()` no canal permite drenar o buffer antes do encerramento.

### Negativas

- **Perda possível:** há semântica *at-most-once*. Cliques descartados por `DropOldest` se perdem, assim como o lote de um flush que falha, e um crash do processo perde o que está em memória.
- **Descarte silencioso:** `TryWrite` retorna `true` mesmo quando um item antigo foi descartado. Hoje não há contador de descartes, o que torna a perda invisível. Esse é um item prioritário de observabilidade.
- **Estado local:** cada instância tem seu próprio canal. Escalar horizontalmente distribui os cliques, mas não há consolidação antes da gravação, o que é aceitável porque o banco é a fonte única.
- **Limite de tempo no desligamento:** drenar um canal cheio pode exceder o timeout padrão de desligamento do host, de 30 s.
- **Migração futura:** se a perda precisar ser zero, a troca por um broker exige reescrever o produtor e o consumidor. A interface `ClickEventChannel` isola esse custo.

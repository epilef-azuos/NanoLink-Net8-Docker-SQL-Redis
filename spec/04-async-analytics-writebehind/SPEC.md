# Especificação de Negócio: Fase 04 - Pipeline de Analytics Assíncrono (Write-Behind)

## Objetivo
Capturar eventos de clique em URLs encurtadas sem impactar a latência do redirecionamento HTTP, utilizando filas em memória com descarregamento assíncrono em lote (batching) no PostgreSQL.

## Requisitos Funcionais
1. **Disparo do Evento**: Ao resolver o redirecionamento com sucesso no `GET /{code}`, disparar imediatamente a publicação assíncrona do evento de clique.
2. **Buffer em Memória**:
   - Uso de `System.Threading.Channels` com capacidade delimitada (*Bounded Channel*).
   - Política de descarte em sobrecarga: `DropOldest` (garante que picos extremos de tráfego priorizem disponibilidade da API e não esgotem a memória RAM).
3. **Coleta Inicial de Metadados**:
   - `Code` e `LinkId` associados.
   - `ClickedAt` (UTC).
   - `UserAgent` (do cabeçalho `User-Agent`).
   - `Referer` (do cabeçalho `Referer`).
   - `IpAddress` (IP do cliente extraído da requisição).
4. **Worker de Descarregamento (Write-Behind Worker)**:
   - Executar continuamente em segundo plano via `BackgroundService`.
   - Inserir no PostgreSQL em lote quando:
     - O buffer atingir 100 itens **OU**
     - O tempo de espera atingir 5 segundos (*time-based flush*).
5. **Critérios de Tolerância a Falhas**:
   - Tratamento de exceções no processador de lotes para evitar que o worker quebre e encerre o processo da API.

## Critérios de Aceitação
- [ ] O endpoint `GET /{code}` não deve ter sua latência alterada pela gravação analítica (< 5ms).
- [ ] Os cliques devem aparecer persistidos na tabela `ClickEvents` em lotes de forma consistente.
- [ ] A aplicação deve realizar *Graceful Shutdown*, esvaziando o buffer antes do encerramento do processo.
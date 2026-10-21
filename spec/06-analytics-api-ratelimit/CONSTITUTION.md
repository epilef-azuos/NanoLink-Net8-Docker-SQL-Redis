# Regras Invioláveis de Engenharia - Fase 06

1. **Zero Avaliação em Memória (No In-Memory Aggregations)**: Consultas de analytics NUNCA devem trazer listas de entidades brutas com `.ToList()` para fazer agregação no C#. As queries DEVEM ser compostas com LINQ e executadas diretamente no PostgreSQL com `.AsNoTracking()`.
2. **Uso Exclusivo do Rate Limiter Nativo**: Proibido o uso de pacotes legados de rate limit (ex: AspNetCoreRateLimit). Usar exclusivamente as APIs do namespace `System.Threading.RateLimiting`.
3. **Tratamento Seguro de IP**: O particionador de Rate Limiting deve prever cenários com IP nulo ou localhost, agrupando com chave de contingência padrão sem lançar exceções.
4. **Governança TASK.md**: Atualizar obrigatoriamente cada item com `[x]` à medida que as tarefas forem concluídas.
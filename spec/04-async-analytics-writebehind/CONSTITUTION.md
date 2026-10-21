# Regras Invioláveis de Engenharia - Fase 04

1. **Isolamento Absoluto do Caminho Crítico**: O endpoint de redirecionamento NUNCA deve chamar o banco de dados diretamente para registrar analytics. A única ação permitida é enfileirar o evento no `Channel`.
2. **Uso Exclusivo de Bounded Channels**: É proibido utilizar `Channel.CreateUnbounded`. Uma fila infinita sob ataque de tráfego resultará em OutOfMemoryException (OOM).
3. **Escopo Seguro no BackgroundService**: O `AppDbContext` é um serviço Scoped e não pode ser injetado diretamente no construtor de um Singleton/BackgroundService. O uso de `IServiceScopeFactory` é obrigatório para cada lote.
4. **Inserção em Lote Eficiente**: As inserções devem ser efetuadas via `AddRangeAsync`, minimizando roundtrips com o PostgreSQL.
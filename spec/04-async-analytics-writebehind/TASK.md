# Checklist de Tarefas Atômicas - Fase 04

- [x] **T1**: Criar a entidade `ClickEvent.cs` e mapeá-la no `AppDbContext.cs` com índices em `Code` e `ClickedAt`.
- [x] **T2**: Implementar o `ClickEventChannel.cs` configurado como Bounded Channel (50.000 posições).
- [x] **T3**: Implementar o `AnalyticsBatchWorker.cs` herdando de `BackgroundService`, com lógica de agregação por lote e tempo via `IServiceScopeFactory`.
- [x] **T4**: Injetar o `ClickEventChannel` no `RedirectUrlHandler` e publicar o evento de clique de forma assíncrona/não bloqueante antes do retorno HTTP 302.
- [x] **T5**: Registrar o Channel e o BackgroundService no `Program.cs`.
- [x] **T6**: Atualizar migrações do EF Core / garantir criação da tabela `ClickEvents`.
- [x] **T7**: Testar localmente com múltiplos acessos e verificar o log do worker inserindo em lote no PostgreSQL.
- [x] **T8**: Marcar todos os checks do TASK.md.
- [x] **T9**: Commit semântico: `feat: implementa pipeline assincrono de analytics com write-behind pattern`.
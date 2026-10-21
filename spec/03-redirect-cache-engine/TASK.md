# Checklist de Tarefas Atômicas - Fase 03

- [x] **T1**: Instalar pacote NuGet `StackExchange.Redis`.
- [x] **T2**: Registrar o `IConnectionMultiplexer` como Singleton no `Program.cs` usando a connection string configurada.
- [x] **T3**: Implementar o serviço de cache otimizado com TTL + Jitter e suporte a Negative Caching.
- [x] **T4**: Criar a Vertical Slice `Features/RedirectUrl` (Handler e Endpoint com `Results.Redirect(originalUrl, permanent: false)`).
- [x] **T5**: Mapear o endpoint `app.MapGet("/{code}", ...)` em `Program.cs`.
- [x] **T6**: Atualizar `requests.http` com cenários de redirecionamento (cache hit, cache miss e 404).
- [x] **T7**: Validar localmente e realizar commit semântico: `feat: implementa motor de redirecionamento com redis e negative caching`.
# Checklist de Tarefas Atômicas - Fase 06

- [x] **T1**: Configurar a política de Rate Limiting nativa (`FixedWindow`) no `Program.cs`.
- [x] **T2**: Aplicar o atributo/método `.RequireRateLimiting("create-link-policy")` no endpoint `POST /api/links`.
- [x] **T3**: Implementar o modelo de resposta `GetAnalyticsResponse.cs` com os records de agregação.
- [x] **T4**: Implementar o `GetAnalyticsHandler.cs` com projeções otimizadas no EF Core (`GroupBy`, `Count`, `Take`).
- [x] **T5**: Implementar e mapear o endpoint `GET /api/links/{code}/analytics` em `GetAnalyticsEndpoint.cs`.
- [x] **T6**: Atualizar o arquivo `requests.http` com testes de consulta de analytics e testes de estouro de rate limit (HTTP 429).
- [x] **T7**: Reconstruir o contêiner com `docker compose up -d --build api` e validar os testes.
- [x] **T8**: Marcar individualmente cada item deste `TASK.md` com `[x]`.
- [x] **T9**: Realizar commit semântico: `feat: implementa api analitica agregada e rate limiting nativo`.
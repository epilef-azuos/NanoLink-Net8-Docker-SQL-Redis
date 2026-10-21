# Checklist de Tarefas Atómicas - Fase 02

- [x] **T1**: Instalar pacotes NuGet (`Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`, `FluentValidation.DependencyInjectionExtensions`).
- [x] **T2**: Implementar `Link.cs` e `AppDbContext.cs` com mapeamento explícito do índice único em `Code`.
- [x] **T3**: Implementar o gerador estático `Base62Generator.cs`.
- [x] **T4**: Implementar a fatia `Features/ShortenUrl` (Request, Response, Validator, Handler e Endpoint).
- [x] **T5**: Configurar a injeção de dependências no `Program.cs`, aplicar migrações automáticas no arranque (`db.Database.MigrateAsync()`) e mapear o endpoint.
- [x] **T6**: Criar ficheiro de teste `NanoLink.http` na raiz para validação manual.
- [x] **T7**: Commit semântico: `feat: implementa criacao de urls curtas com vsa e ef core`.
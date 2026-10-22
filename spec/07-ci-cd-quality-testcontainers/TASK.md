# Checklist de Tarefas Atômicas - Fase 07

- [x] **T1**: Criar o projeto de testes `tests/NanoLink.IntegrationTests` e adicioná-lo à Solution `NanoLink.sln`.
- [x] **T2**: Instalar os pacotes NuGet de teste (`Microsoft.AspNetCore.Mvc.Testing`, `Testcontainers.PostgreSql`, `Testcontainers.Redis`, `FluentAssertions`).
- [x] **T3**: Criar a `NanoLinkApiFactory.cs` gerenciando o ciclo de vida dos contêineres PostgreSQL e Redis e aplicando as migrações automáticas.
- [x] **T4**: Implementar a classe de testes `LinkIntegrationTests.cs` cobrindo cenários de sucesso (201/302), validação (400) e negative cache (404).
- [x] **T5**: Executar os testes localmente com `dotnet test` e garantir 100% de aprovação.
- [x] **T6**: Criar o arquivo de workflow `.github/workflows/ci.yml` configurado para lint, build e test.
- [x] **T7**: Executar `dotnet format` na solução para normalizar a indentação e estilo.
- [x] **T8**: Marcar individualmente cada item deste `TASK.md` com `[x]`.
- [x] **T9**: Realizar commit semântico: `test: implementa testes de integracao com testcontainers e pipeline ci no github actions`.
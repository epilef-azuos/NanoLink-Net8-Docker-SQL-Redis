# Planejamento Técnico: Fase 07

## Estrutura de Diretórios
/
  .github
    /workflows
      ci.yml
  /src
    /NanoLink.Api
  /tests
    /NanoLink.IntegrationTests
      NanoLinkApiFactory.cs
      LinkIntegrationTests.cs
      NanoLink.IntegrationTests.csproj
  NanoLink.sln

## Pacotes NuGet do Projeto de Testes
- `Microsoft.AspNetCore.Mvc.Testing` (v9.*)
- `Testcontainers.PostgreSql` (v3.* ou v4.*)
- `Testcontainers.Redis` (v3.* ou v4.*)
- `FluentAssertions`
- `xunit` e `xunit.runner.visualstudio`
- `Microsoft.NET.Test.Sdk`

## Decisões Técnicas de Testcontainers
- Implementação de `IAsyncLifetime` na fixture customizada `NanoLinkApiFactory` para gerenciar a inicialização (`StartAsync`) e término (`DisposeAsync`) dos contêineres do PostgreSQL e Redis.
- Sobrescrita dinâmica das configurações de conexão no `ConfigureWebHost` apontando para as connection strings fornecidas pelas instâncias do Testcontainers (`GetConnectionString()`).
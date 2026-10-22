using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace NanoLink.IntegrationTests;

public sealed class NanoLinkApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine").Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine").Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgresContainer.StartAsync(), _redisContainer.StartAsync());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // As conexões apontam para os contêineres efêmeros, nunca para instâncias locais.
        // As migrações são aplicadas pelo próprio Program ao iniciar o host de teste.
        builder.UseSetting("ConnectionStrings:Postgres", _postgresContainer.GetConnectionString());
        builder.UseSetting("ConnectionStrings:Redis", _redisContainer.GetConnectionString());
    }

    // WebApplicationFactory já expõe DisposeAsync (ValueTask); o IAsyncLifetime usa Task, então é implementado explicitamente
    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
    }
}

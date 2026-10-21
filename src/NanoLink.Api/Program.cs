using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NanoLink.Api.Data;
using NanoLink.Api.Features.GetAnalytics;
using NanoLink.Api.Features.RedirectUrl;
using NanoLink.Api.Features.ShortenUrl;
using NanoLink.Api.Infrastructure.Analytics;
using NanoLink.Api.Infrastructure.Enrichment;
using NanoLink.Api.Infrastructure.Cache;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Um único multiplexer compartilhado evita esgotar sockets
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Write-behind: cliques são enfileirados no canal e gravados em lote pelo worker
builder.Services.AddSingleton<ClickEventChannel>();
builder.Services.AddHostedService<AnalyticsBatchWorker>();

// Enriquecimento offline: leitor MMDB em memória (singleton) e enricher por escopo do lote
builder.Services.AddSingleton<IGeoLocationResolver, MaxMindGeoLocationResolver>();
builder.Services.AddScoped<IAnalyticsEnricher, AnalyticsEnricher>();

builder.Services.AddValidatorsFromAssemblyContaining<ShortenUrlValidator>();
builder.Services.AddScoped<ShortenUrlHandler>();
builder.Services.AddScoped<RedirectUrlHandler>();
builder.Services.AddScoped<GetAnalyticsHandler>();
builder.Services.AddProblemDetails();

// Rate limiting nativo: até 10 criações de link por minuto por IP
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("create-link", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Rate limit exceeded. Try again later." },
            cancellationToken);
    };
});

var app = builder.Build();

app.UseRateLimiter();

app.MapGet("/", () => "NanoLink API is running on Docker! 🚀");
app.MapShortenUrlEndpoint();
app.MapRedirectUrlEndpoint();
app.MapGetAnalyticsEndpoint();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

// Torna o ponto de entrada visível para o WebApplicationFactory dos testes de integração
public partial class Program { }

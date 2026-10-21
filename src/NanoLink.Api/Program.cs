using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NanoLink.Api.Data;
using NanoLink.Api.Features.RedirectUrl;
using NanoLink.Api.Features.ShortenUrl;
using NanoLink.Api.Infrastructure.Cache;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Um único multiplexer compartilhado evita esgotar sockets
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

builder.Services.AddValidatorsFromAssemblyContaining<ShortenUrlValidator>();
builder.Services.AddScoped<ShortenUrlHandler>();
builder.Services.AddScoped<RedirectUrlHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.MapGet("/", () => "NanoLink API is running on Docker! 🚀");
app.MapShortenUrlEndpoint();
app.MapRedirectUrlEndpoint();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

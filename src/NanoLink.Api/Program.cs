using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NanoLink.Api.Data;
using NanoLink.Api.Features.ShortenUrl;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddValidatorsFromAssemblyContaining<ShortenUrlValidator>();
builder.Services.AddScoped<ShortenUrlHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.MapGet("/", () => "NanoLink API is running on Docker! 🚀");
app.MapShortenUrlEndpoint();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NanoLink.Api.Common;
using NanoLink.Api.Data;
using NanoLink.Api.Features.GetAnalytics;
using NanoLink.Api.Features.ShortenUrl;
using NanoLink.Api.Infrastructure.Cache;
using StackExchange.Redis;
using Xunit;

namespace NanoLink.IntegrationTests;

public class LinkIntegrationTests(NanoLinkApiFactory factory) : IClassFixture<NanoLinkApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private readonly HttpClient _noRedirectClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    [Fact]
    public async Task CreateLink_WithValidUrl_Returns201CreatedAndPersists()
    {
        var originalUrl = $"https://www.exemplo.com/integracao/{Guid.NewGuid()}";

        var response = await _client.PostAsJsonAsync("/api/links", new ShortenUrlRequest(originalUrl));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ShortenUrlResponse>();
        body.Should().NotBeNull();
        body!.Code.Should().MatchRegex("^[0-9a-zA-Z]{7}$");
        body.OriginalUrl.Should().Be(originalUrl);
        body.ShortUrl.Should().EndWith($"/{body.Code}");
        body.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        response.Headers.Location!.OriginalString.Should().Be($"/api/links/{body.Code}");

        // Confirma no banco efêmero que o registro foi realmente persistido
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var persisted = await db.Links.AsNoTracking().SingleOrDefaultAsync(l => l.Code == body.Code);

        persisted.Should().NotBeNull();
        persisted!.OriginalUrl.Should().Be(originalUrl);
    }

    [Fact]
    public async Task CreateLink_WithInvalidUrl_Returns400BadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/links", new ShortenUrlRequest("ftp://arquivo-invalido.com/x"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("errors").TryGetProperty("OriginalUrl", out _).Should().BeTrue();
    }

    [Fact]
    public async Task RedirectUrl_WhenExists_Returns302WithCorrectLocation()
    {
        var originalUrl = $"https://www.exemplo.com/redirect/{Guid.NewGuid()}";
        var code = await CreateLinkAsync(originalUrl);

        var response = await _noRedirectClient.GetAsync($"/{code}");

        response.StatusCode.Should().Be(HttpStatusCode.Found);
        response.Headers.Location!.ToString().Should().Be(originalUrl);

        // O resultado deve ter sido armazenado no Redis do contêiner
        var cached = await GetRedisDatabase().StringGetAsync(CacheKey(code));
        cached.ToString().Should().Be(originalUrl);
    }

    [Fact]
    public async Task RedirectUrl_WhenDoesNotExist_Returns404NotFound()
    {
        // Código gerado aleatoriamente: não existe no banco efêmero deste teste
        var code = Base62Generator.Generate();

        var response = await _noRedirectClient.GetAsync($"/{code}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Negative caching: o marcador sentinela deve estar no Redis
        var sentinel = await GetRedisDatabase().StringGetAsync(CacheKey(code));
        sentinel.ToString().Should().Be(ICacheService.NotFoundMarker);
    }

    [Fact]
    public async Task Analytics_AfterRedirects_ReturnsAggregatedMetricsOnceBatchIsFlushed()
    {
        var code = await CreateLinkAsync($"https://www.exemplo.com/analytics/{Guid.NewGuid()}");

        await RedirectWithHeadersAsync(code, "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) Mobile", "https://t.co/abc");
        await RedirectWithHeadersAsync(code, "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) Mobile", "https://t.co/def");
        await RedirectWithHeadersAsync(code, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0", "https://www.google.com/");

        // O worker grava em lote com flush de até 5 segundos: aguarda os 3 cliques aparecerem
        var analytics = await WaitForClicksAsync(code, expected: 3);

        analytics.Code.Should().Be(code);
        analytics.TotalClicks.Should().Be(3);
        analytics.ByOperatingSystem.Should().Contain(m => m.Key == "iOS" && m.Count == 2);
        analytics.ByReferrer.Should().Contain(m => m.Key == "t.co" && m.Count == 2);
        analytics.ByReferrer.Should().Contain(m => m.Key == "google.com" && m.Count == 1);
        analytics.ClicksOverTime.Should().HaveCount(7);
        analytics.ClicksOverTime.Sum(d => d.Count).Should().Be(3);
    }

    [Fact]
    public async Task Analytics_WithoutClicks_ReturnsZeroAndEmptyLists()
    {
        var code = await CreateLinkAsync($"https://www.exemplo.com/sem-cliques/{Guid.NewGuid()}");

        var analytics = await _client.GetFromJsonAsync<GetAnalyticsResponse>($"/api/links/{code}/analytics");

        analytics.Should().NotBeNull();
        analytics!.TotalClicks.Should().Be(0);
        analytics.ByCountry.Should().BeEmpty();
        analytics.ByDevice.Should().BeEmpty();
        analytics.ByOperatingSystem.Should().BeEmpty();
        analytics.ByReferrer.Should().BeEmpty();
        analytics.ClicksOverTime.Should().HaveCount(7);
        analytics.ClicksOverTime.Sum(d => d.Count).Should().Be(0);
    }

    private async Task<string> CreateLinkAsync(string originalUrl)
    {
        var response = await _client.PostAsJsonAsync("/api/links", new ShortenUrlRequest(originalUrl));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<ShortenUrlResponse>();
        return body!.Code;
    }

    private async Task RedirectWithHeadersAsync(string code, string userAgent, string referer)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/{code}");
        request.Headers.UserAgent.ParseAdd(userAgent);
        request.Headers.Referrer = new Uri(referer);

        using var response = await _noRedirectClient.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Found);
    }

    private async Task<GetAnalyticsResponse> WaitForClicksAsync(string code, int expected)
    {
        var deadline = DateTime.UtcNow.AddSeconds(20);

        while (true)
        {
            var analytics = await _client.GetFromJsonAsync<GetAnalyticsResponse>($"/api/links/{code}/analytics");

            if (analytics!.TotalClicks >= expected || DateTime.UtcNow > deadline)
            {
                return analytics;
            }

            await Task.Delay(500);
        }
    }

    private IDatabase GetRedisDatabase() =>
        factory.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();

    private static string CacheKey(string code) => $"link:{code}";
}

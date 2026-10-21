using StackExchange.Redis;

namespace NanoLink.Api.Infrastructure.Cache;

public sealed class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private static readonly TimeSpan ActiveTtl = TimeSpan.FromHours(24);
    private static readonly TimeSpan NegativeTtl = TimeSpan.FromSeconds(60);

    // Jitter de até 30 minutos para espalhar as expirações e evitar cache stampede
    private const int MaxJitterSeconds = 1800;

    private readonly IDatabase _database = redis.GetDatabase();

    public async Task<string?> GetUrlAsync(string code, CancellationToken ct = default)
    {
        RedisValue value = await _database.StringGetAsync(BuildKey(code));

        return value.HasValue ? value.ToString() : null;
    }

    public Task SetUrlAsync(string code, string originalUrl, CancellationToken ct = default)
    {
        var jitter = TimeSpan.FromSeconds(Random.Shared.Next(0, MaxJitterSeconds + 1));

        return _database.StringSetAsync(BuildKey(code), originalUrl, ActiveTtl + jitter);
    }

    public Task SetNegativeCacheAsync(string code, CancellationToken ct = default)
    {
        return _database.StringSetAsync(BuildKey(code), ICacheService.NotFoundMarker, NegativeTtl);
    }

    private static string BuildKey(string code) => $"link:{code}";
}

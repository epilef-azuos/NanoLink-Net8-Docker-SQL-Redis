namespace NanoLink.Api.Infrastructure.Cache;

public interface ICacheService
{
    // Valor sentinela gravado para códigos inexistentes (Negative Caching)
    public const string NotFoundMarker = "__NOT_FOUND__";

    Task<string?> GetUrlAsync(string code, CancellationToken ct = default);

    Task SetUrlAsync(string code, string originalUrl, CancellationToken ct = default);

    Task SetNegativeCacheAsync(string code, CancellationToken ct = default);
}

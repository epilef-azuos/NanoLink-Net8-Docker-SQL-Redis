using Microsoft.EntityFrameworkCore;
using NanoLink.Api.Data;
using NanoLink.Api.Infrastructure.Cache;

namespace NanoLink.Api.Features.RedirectUrl;

public sealed class RedirectUrlHandler(ICacheService cache, AppDbContext db)
{
    // Retorna a URL de destino, ou null quando o código não existe
    public async Task<string?> HandleAsync(string code, CancellationToken ct)
    {
        var cachedUrl = await cache.GetUrlAsync(code, ct);
        if (cachedUrl is not null)
        {
            return cachedUrl == ICacheService.NotFoundMarker ? null : cachedUrl;
        }

        // Cache miss: busca apenas a URL, sem tracking
        var originalUrl = await db.Links
            .AsNoTracking()
            .Where(link => link.Code == code)
            .Select(link => link.OriginalUrl)
            .FirstOrDefaultAsync(ct);

        if (originalUrl is null)
        {
            await cache.SetNegativeCacheAsync(code, ct);
            return null;
        }

        await cache.SetUrlAsync(code, originalUrl, ct);
        return originalUrl;
    }
}

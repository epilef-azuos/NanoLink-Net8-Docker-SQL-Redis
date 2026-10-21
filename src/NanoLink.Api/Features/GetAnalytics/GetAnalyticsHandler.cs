using Microsoft.EntityFrameworkCore;
using NanoLink.Api.Data;

namespace NanoLink.Api.Features.GetAnalytics;

public sealed class GetAnalyticsHandler(AppDbContext db)
{
    private const int TopN = 10;
    private const int WindowDays = 7;
    private const string Unknown = "Unknown";

    public async Task<GetAnalyticsResponse?> HandleAsync(string code, CancellationToken ct)
    {
        var link = await db.Links
            .AsNoTracking()
            .Where(l => l.Code == code)
            .Select(l => new { l.Code, l.OriginalUrl, l.CreatedAt })
            .FirstOrDefaultAsync(ct);

        if (link is null)
        {
            return null;
        }

        // Todas as agregações são traduzidas para SQL: nenhuma linha bruta vai para a memória
        var clicks = db.ClickEvents.AsNoTracking().Where(c => c.Code == code);

        var totalClicks = await clicks.CountAsync(ct);

        var byCountry = await clicks
            .GroupBy(c => new { c.CountryCode, c.CountryName })
            .Select(g => new { g.Key.CountryCode, g.Key.CountryName, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(TopN)
            .ToListAsync(ct);

        var byDevice = await clicks
            .GroupBy(c => c.DeviceType)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(TopN)
            .ToListAsync(ct);

        var byOperatingSystem = await clicks
            .GroupBy(c => c.OperatingSystem)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(TopN)
            .ToListAsync(ct);

        var byReferrer = await clicks
            .GroupBy(c => c.ReferrerDomain)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(TopN)
            .ToListAsync(ct);

        // Janela dos últimos 7 dias corridos, incluindo hoje (UTC)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = today.AddDays(-(WindowDays - 1));
        var windowStart = new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        // Agrupa por componentes inteiros (ano/mês/dia): o Npgsql traduz para SQL e evita
        // a materialização de DateTime a partir de coluna timestamptz
        var dailyCounts = await clicks
            .Where(c => c.ClickedAt >= windowStart)
            .GroupBy(c => new { c.ClickedAt.Year, c.ClickedAt.Month, c.ClickedAt.Day })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Count = g.Count() })
            .ToListAsync(ct);

        // Preenche os dias sem cliques com zero para a série ficar contínua
        var clicksOverTime = Enumerable.Range(0, WindowDays)
            .Select(offset => startDate.AddDays(offset))
            .Select(date => new TimeSeriesMetric(
                date,
                dailyCounts.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month && d.Day == date.Day)?.Count ?? 0))
            .ToList();

        return new GetAnalyticsResponse(
            link.Code,
            link.OriginalUrl,
            link.CreatedAt,
            totalClicks,
            byCountry.Select(x => new GroupMetric(x.CountryCode ?? x.CountryName ?? Unknown, x.CountryName ?? Unknown, x.Count)).ToList(),
            byDevice.Select(x => ToGroupMetric(x.Key, x.Count)).ToList(),
            byOperatingSystem.Select(x => ToGroupMetric(x.Key, x.Count)).ToList(),
            byReferrer.Select(x => ToGroupMetric(x.Key, x.Count)).ToList(),
            clicksOverTime);
    }

    private static GroupMetric ToGroupMetric(string? key, int count) =>
        new(key ?? Unknown, key ?? Unknown, count);
}

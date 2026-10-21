namespace NanoLink.Api.Features.GetAnalytics;

public record GetAnalyticsResponse(
    string Code,
    string OriginalUrl,
    DateTimeOffset CreatedAt,
    int TotalClicks,
    List<GroupMetric> ByCountry,
    List<GroupMetric> ByDevice,
    List<GroupMetric> ByOperatingSystem,
    List<GroupMetric> ByReferrer,
    List<TimeSeriesMetric> ClicksOverTime);

public record GroupMetric(string Key, string Label, int Count);

public record TimeSeriesMetric(DateOnly Date, int Count);

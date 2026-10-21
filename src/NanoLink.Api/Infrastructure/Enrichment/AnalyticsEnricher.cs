using NanoLink.Api.Data.Entities;
using UAParser;

namespace NanoLink.Api.Infrastructure.Enrichment;

public sealed class AnalyticsEnricher(IGeoLocationResolver geoLocationResolver) : IAnalyticsEnricher
{
    private const string Unknown = "Unknown";
    private const string Direct = "Direct";
    private const int MaxReferrerDomainLength = 150;

    // Parser é pesado de construir e thread-safe: criado uma única vez
    private static readonly Parser UserAgentParser = Parser.GetDefault(new ParserOptions());

    public void EnrichEvent(ClickEvent evt)
    {
        var geo = geoLocationResolver.Resolve(evt.IpAddress);
        var (operatingSystem, deviceType) = ParseUserAgent(evt.UserAgent);

        evt.Enrich(
            geo.CountryCode,
            geo.CountryName,
            geo.CityName,
            operatingSystem,
            deviceType,
            ExtractReferrerDomain(evt.Referer));
    }

    private static (string OperatingSystem, string DeviceType) ParseUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return (Unknown, Unknown);
        }

        var client = UserAgentParser.Parse(userAgent);
        var family = client.OS.Family;

        return (MapOperatingSystem(family), ClassifyDevice(client, userAgent));
    }

    private static string MapOperatingSystem(string? family) => family switch
    {
        null => "Outro",
        _ when family.StartsWith("Windows", StringComparison.Ordinal) => "Windows",
        "iOS" => "iOS",
        "Android" => "Android",
        "Mac OS X" => "macOS",
        "Linux" or "Ubuntu" or "Fedora" or "Debian" => "Linux",
        _ => "Outro"
    };

    private static string ClassifyDevice(ClientInfo client, string userAgent)
    {
        if (client.Device.IsSpider)
        {
            return "Bot";
        }

        var deviceFamily = client.Device.Family ?? string.Empty;
        var os = client.OS.Family ?? string.Empty;

        // Android sem o token "Mobile" é tablet; iPad e famílias com "Tablet" também
        if (deviceFamily.Contains("iPad", StringComparison.OrdinalIgnoreCase)
            || deviceFamily.Contains("Tablet", StringComparison.OrdinalIgnoreCase)
            || (os == "Android" && !userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase)))
        {
            return "Tablet";
        }

        if (os is "iOS" or "Android" or "Windows Phone"
            || deviceFamily.Contains("Smartphone", StringComparison.OrdinalIgnoreCase)
            || deviceFamily.Contains("iPhone", StringComparison.OrdinalIgnoreCase))
        {
            return "Mobile";
        }

        return "Desktop";
    }

    private static string ExtractReferrerDomain(string? referer)
    {
        if (string.IsNullOrWhiteSpace(referer))
        {
            return Direct;
        }

        if (!Uri.TryCreate(referer, UriKind.Absolute, out var uri))
        {
            return Unknown;
        }

        var host = uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? uri.Host[4..] : uri.Host;

        return host.Length > MaxReferrerDomainLength ? host[..MaxReferrerDomainLength] : host;
    }
}

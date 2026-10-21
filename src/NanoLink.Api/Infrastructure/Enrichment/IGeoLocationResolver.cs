namespace NanoLink.Api.Infrastructure.Enrichment;

public interface IGeoLocationResolver
{
    GeoLocation Resolve(string? ipAddress);
}

public sealed record GeoLocation(string? CountryCode, string? CountryName, string? CityName);

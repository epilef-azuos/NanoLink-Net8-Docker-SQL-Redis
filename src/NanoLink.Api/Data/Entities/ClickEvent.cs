namespace NanoLink.Api.Data.Entities;

public class ClickEvent
{
    // Construtor exigido pelo EF Core para materializar as entidades
    protected ClickEvent()
    {
    }

    public ClickEvent(string code, string? ipAddress, string? userAgent, string? referer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        Id = Guid.NewGuid();
        Code = code;
        ClickedAt = DateTimeOffset.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Referer = referer;
    }

    public Guid Id { get; private init; }

    public string Code { get; private init; } = null!;

    public DateTimeOffset ClickedAt { get; private init; }

    public string? UserAgent { get; private init; }

    public string? Referer { get; private init; }

    public string? IpAddress { get; private init; }

    public string? CountryCode { get; internal set; }

    public string? CountryName { get; internal set; }

    public string? CityName { get; internal set; }

    public string? OperatingSystem { get; internal set; }

    public string? DeviceType { get; internal set; }

    public string? ReferrerDomain { get; internal set; }

    // Preenchido pelo worker em segundo plano, nunca no caminho do redirecionamento
    public void Enrich(
        string? countryCode,
        string? countryName,
        string? cityName,
        string? os,
        string? deviceType,
        string? referrerDomain)
    {
        CountryCode = countryCode;
        CountryName = countryName;
        CityName = cityName;
        OperatingSystem = os;
        DeviceType = deviceType;
        ReferrerDomain = referrerDomain;
    }
}

using System.Net;
using System.Net.Sockets;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;

namespace NanoLink.Api.Infrastructure.Enrichment;

public sealed class MaxMindGeoLocationResolver : IGeoLocationResolver, IDisposable
{
    public const string LocalNetwork = "Local Network";
    public const string Unknown = "Unknown";

    private readonly DatabaseReader? _reader;
    private readonly ILogger<MaxMindGeoLocationResolver> _logger;

    public MaxMindGeoLocationResolver(
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<MaxMindGeoLocationResolver> logger)
    {
        _logger = logger;

        var relativePath = configuration["GeoIp:DatabasePath"] ?? "Resources/GeoLite2-City.mmdb";
        var databasePath = Path.Combine(environment.ContentRootPath, relativePath);

        // Sem o arquivo a API continua de pé: a geolocalização vira "Unknown"
        _reader = File.Exists(databasePath) ? new DatabaseReader(databasePath) : null;

        if (_reader is null)
        {
            _logger.LogWarning("Base MMDB não encontrada em {DatabasePath}; a geolocalização ficará como Unknown", databasePath);
        }
    }

    public GeoLocation Resolve(string? ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
        {
            return new GeoLocation(null, Unknown, null);
        }

        if (IsLocal(ip))
        {
            return new GeoLocation(null, LocalNetwork, null);
        }

        if (_reader is null)
        {
            return new GeoLocation(null, Unknown, null);
        }

        try
        {
            var city = _reader.City(ip);

            return new GeoLocation(
                city.Country.IsoCode,
                city.Country.Name ?? Unknown,
                city.City.Name);
        }
        catch (AddressNotFoundException)
        {
            return new GeoLocation(null, Unknown, null);
        }
        catch (GeoIP2Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao consultar a base MMDB para o IP {IpAddress}", ipAddress);
            return new GeoLocation(null, Unknown, null);
        }
    }

    public void Dispose() => _reader?.Dispose();

    private static bool IsLocal(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip))
        {
            return true;
        }

        var address = ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip;
        var bytes = address.GetAddressBytes();

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            return bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168)
                || (bytes[0] == 169 && bytes[1] == 254);
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // fc00::/7 (unique local) e fe80::/10 (link-local)
            return (bytes[0] & 0xFE) == 0xFC
                || (bytes[0] == 0xFE && (bytes[1] & 0xC0) == 0x80);
        }

        return false;
    }
}

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
}

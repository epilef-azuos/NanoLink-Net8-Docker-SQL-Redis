namespace NanoLink.Api.Data.Entities;

public class Link
{
    // Construtor exigido pelo EF Core para materializar as entidades
    protected Link()
    {
    }

    public Link(string code, string originalUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalUrl);

        Id = Guid.NewGuid();
        Code = code;
        OriginalUrl = originalUrl;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private init; }

    public string Code { get; private init; } = null!;

    public string OriginalUrl { get; private init; } = null!;

    public DateTimeOffset CreatedAt { get; private init; }
}

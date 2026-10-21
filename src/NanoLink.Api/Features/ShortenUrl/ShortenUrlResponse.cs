namespace NanoLink.Api.Features.ShortenUrl;

public record ShortenUrlResponse(string Code, string ShortUrl, string OriginalUrl, DateTimeOffset CreatedAt);

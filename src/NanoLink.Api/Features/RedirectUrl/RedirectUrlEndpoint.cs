namespace NanoLink.Api.Features.RedirectUrl;

public static class RedirectUrlEndpoint
{
    private const int MaxCodeLength = 10;

    public static IEndpointRouteBuilder MapRedirectUrlEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{code}", HandleAsync)
            .WithName("RedirectUrl");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        string? code,
        RedirectUrlHandler handler,
        CancellationToken cancellationToken)
    {
        // Validação fail-fast antes de qualquer acesso ao cache ou ao banco
        if (string.IsNullOrEmpty(code) || code.Length > MaxCodeLength || !code.All(char.IsAsciiLetterOrDigit))
        {
            return Results.BadRequest();
        }

        var originalUrl = await handler.HandleAsync(code, cancellationToken);

        if (originalUrl is null)
        {
            return Results.NotFound(new { message = "Link not found or expired" });
        }

        return Results.Redirect(originalUrl, permanent: false);
    }
}

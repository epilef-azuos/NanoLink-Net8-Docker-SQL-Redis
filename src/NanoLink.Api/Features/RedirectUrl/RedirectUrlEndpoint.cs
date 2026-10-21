using System.Net;
using NanoLink.Api.Data.Entities;
using NanoLink.Api.Infrastructure.Analytics;

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
        HttpContext context,
        RedirectUrlHandler handler,
        ClickEventChannel clickChannel,
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

        // Publicação não bloqueante: o 302 não espera pela gravação analítica
        clickChannel.TryWrite(new ClickEvent(
            code,
            ResolveClientIp(context),
            NullIfEmpty(context.Request.Headers.UserAgent),
            NullIfEmpty(context.Request.Headers.Referer)));

        return Results.Redirect(originalUrl, permanent: false);
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

    private static string? ResolveClientIp(HttpContext context)
    {
        // X-Forwarded-For: primeiro IP da lista, para testes manuais com IPs reais via .http
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrEmpty(forwardedFor)
            && IPAddress.TryParse(forwardedFor.Split(',')[0].Trim(), out var forwardedIp))
        {
            return forwardedIp.ToString();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}

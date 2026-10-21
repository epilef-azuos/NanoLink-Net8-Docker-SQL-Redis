namespace NanoLink.Api.Features.ShortenUrl;

public static class ShortenUrlEndpoint
{
    public static IEndpointRouteBuilder MapShortenUrlEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/links", HandleAsync)
            .WithName("ShortenUrl")
            .Produces<ShortenUrlResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ShortenUrlRequest request,
        HttpContext httpContext,
        ShortenUrlHandler handler,
        CancellationToken cancellationToken)
    {
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

        var (validation, response) = await handler.HandleAsync(request, baseUrl, cancellationToken);

        if (!validation.IsValid || response is null)
        {
            return TypedResults.ValidationProblem(validation.ToDictionary());
        }

        return TypedResults.Created($"/api/links/{response.Code}", response);
    }
}

namespace NanoLink.Api.Features.GetAnalytics;

public static class GetAnalyticsEndpoint
{
    public static IEndpointRouteBuilder MapGetAnalyticsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/links/{code}/analytics", HandleAsync)
            .WithName("GetLinkAnalytics")
            .Produces<GetAnalyticsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        string code,
        GetAnalyticsHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(code, cancellationToken);

        if (response is null)
        {
            return Results.NotFound(new { message = "Link not found" });
        }

        return Results.Ok(response);
    }
}

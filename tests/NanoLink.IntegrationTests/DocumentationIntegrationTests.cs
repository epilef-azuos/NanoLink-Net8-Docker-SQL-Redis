using System.Net;
using FluentAssertions;
using Xunit;

namespace NanoLink.IntegrationTests;

public class DocumentationIntegrationTests(NanoLinkApiFactory factory) : IClassFixture<NanoLinkApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task OpenApiDocument_IsExposedInDevelopment_AndDescribesLinkEndpoints()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var document = await response.Content.ReadAsStringAsync();
        document.Should().Contain("/api/links");
        document.Should().Contain("/api/links/{code}/analytics");
    }

    [Fact]
    public async Task ScalarUi_IsExposedInDevelopment()
    {
        var response = await _client.GetAsync("/scalar/v1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

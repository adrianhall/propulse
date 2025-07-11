using Propulse.Web.Tests.Helpers;

namespace Propulse.Web.Tests;

[Collection(WebServiceCollection.Name)]
public class SmokeTests(WebServiceFixture fixture) : IClassFixture<WebServiceFixture>
{
    [Fact]
    public async Task GET_Alive_Works()
    {
        // Arrange
        using var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/alive");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GET_Health_Works()
    {
        // Arrange
        using var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}

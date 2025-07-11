using Propulse.Web.Tests.Helpers;

namespace Propulse.Web.Tests;

[Collection(SerializedCollection.Name)]
public class SmokeTests(WebServiceFixture fixture)
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

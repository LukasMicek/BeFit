using System.Net;

namespace BeFit.IntegrationTests;

public class HttpIntegrationTests : IClassFixture<BeFitWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HttpIntegrationTests(BeFitWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task TrainingSessions_Index_ReturnsOk_WhenAuthenticated()
    {
        // Act
        var response = await _client.GetAsync("/TrainingSessions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Home_Index_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

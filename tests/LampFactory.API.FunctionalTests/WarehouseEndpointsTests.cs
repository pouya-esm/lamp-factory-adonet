using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace LampFactory.API.FunctionalTests;

public class WarehouseEndpointsTests : IClassFixture<LampFactoryApiFactory>
{
    private readonly HttpClient _client;

    public WarehouseEndpointsTests(LampFactoryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllWarehouses_WhenAnonymous_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/warehouse");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWarehouseById_WhenAnonymous_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/warehouse/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UnknownRoute_WhenRequested_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

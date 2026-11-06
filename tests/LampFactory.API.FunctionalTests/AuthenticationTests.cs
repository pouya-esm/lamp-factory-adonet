using LampFactory.Application.Features.IdentityAccess.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace LampFactory.API.FunctionalTests;

public class AuthenticationTests : IClassFixture<LampFactoryApiFactory>
{
    private readonly HttpClient _client;

    public AuthenticationTests(LampFactoryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreMissing_ReturnsUnauthorizedOrBadRequest()
    {
        var request = new LoginDTO { Username = "nonexistent", Password = "wrong" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // The endpoint never reveals which of the two was wrong.
        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest });
    }

    [Fact]
    public async Task Login_WhenBodyIsEmpty_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SecuredEndpoint_WhenAnonymous_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/warehouse");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

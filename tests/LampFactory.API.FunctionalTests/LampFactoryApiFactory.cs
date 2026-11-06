using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LampFactory.API.FunctionalTests;

/// <summary>
/// Shared WebApplicationFactory for the functional tests. The values below are
/// TEST-ONLY placeholders; real secret values (Jwt:Key) are never committed and
/// are supplied locally via <c>dotnet user-secrets</c> / environment variables.
/// </summary>
public sealed class LampFactoryApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", "functional-test-signing-key-0123456789abcdef-0123456789");
        builder.UseSetting("Jwt:Issuer", "LampFactoryAPI");
        builder.UseSetting("Jwt:Audience", "LampFactoryClients");
    }
}

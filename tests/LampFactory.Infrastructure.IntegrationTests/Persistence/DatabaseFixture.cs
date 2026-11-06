using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LampFactory.Infrastructure.IntegrationTests.Persistence;

// Shares one connection check across the integration tests so the suite does
// not probe the database once per test.
public sealed class DatabaseFixture : IDisposable
{
    public IConfiguration Configuration { get; }
    public bool IsAvailable { get; }

    public DatabaseFixture()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(cs))
        {
            IsAvailable = false;
            return;
        }

        try
        {
            using var conn = new SqlConnection(cs);
            conn.Open();
            IsAvailable = true;
        }
        catch
        {
            // No reachable database; tests that need one will skip themselves.
            IsAvailable = false;
        }
    }

    public void Dispose() { }
}

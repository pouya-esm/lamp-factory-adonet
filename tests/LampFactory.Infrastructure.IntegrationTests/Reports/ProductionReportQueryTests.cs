using LampFactory.Infrastructure.IntegrationTests.Persistence;
using LampFactory.Infrastructure.Persistence;
using LampFactory.Infrastructure.Reports;
using Microsoft.Extensions.Configuration;

namespace LampFactory.Infrastructure.IntegrationTests.Reports;

public class ProductionReportQueryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ProductionReportQueryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetProductionTraceability_WhenDbUnavailable_IsSkipped()
    {
        if (!_fixture.IsAvailable) return;

        var context = new ApplicationDbContext(_fixture.Configuration);
        var queries = new ProductionTraceabilityReportQuery(context);

        // An empty filter set is the widest query; it should at minimum not throw.
        var results = await queries.GetProductionTraceabilityAsync(null, string.Empty);

        Assert.NotNull(results);
    }

    [Fact]
    public async Task GetMaterialConsumptionVariance_WhenDbUnavailable_IsSkipped()
    {
        if (!_fixture.IsAvailable) return;

        var context = new ApplicationDbContext(_fixture.Configuration);
        var queries = new MaterialConsumptionVarianceReportQuery(context);

        var results = await queries.GetMaterialConsumptionVarianceAsync(null, string.Empty);

        Assert.NotNull(results);
    }
}

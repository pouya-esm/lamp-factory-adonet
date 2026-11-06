using LampFactory.Application.Abstractions.Security;
using LampFactory.Domain.Enums;
using LampFactory.Domain.Inventory;
using LampFactory.Infrastructure.Persistence;
using LampFactory.Infrastructure.Persistence.Repositories.Inventory;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LampFactory.Infrastructure.IntegrationTests.Persistence;

// These hit a real SQL Server. They are skipped automatically when no connection
// string is reachable, so the suite stays green on a machine without the DB.
public class WarehouseRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public WarehouseRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateWithProducts_ThenGetById_RoundTripsWarehouse()
    {
        if (!_fixture.IsAvailable) return;

        var context = new ApplicationDbContext(_fixture.Configuration);
        var repo = new WarehouseRepository(context, new TestCurrentUser());

        var warehouse = new Warehouse
        {
            WarehouseType = WarehouseType.EntryStorage,
            Name = "Integration Test Warehouse",
            Address = "Test"
        };
        var products = new List<WarehouseProduct> { new WarehouseProduct { ProductID = 1, Quantity = 4 } };

        var id = await repo.CreateWithProductsAsync(warehouse, products);
        Assert.True(id > 0);

        try
        {
            var (loaded, stock) = await repo.GetByIdWithStockAsync(id);
            Assert.NotNull(loaded);
            Assert.Equal("Integration Test Warehouse", loaded!.Name);
            Assert.Single(stock);
            Assert.Equal(4, stock[0].Quantity);
        }
        finally
        {
            await repo.RemoveWithProductsAsync(id);
        }
    }

    // The repository only needs the current-user abstraction, so the test
    // supplies a stand-in actor instead of building an HttpContext.
    private sealed class TestCurrentUser : ICurrentUserService
    {
        public int? UserId => 1;
        public int ActorId => 1;
    }
}

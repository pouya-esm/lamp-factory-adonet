using LampFactory.Domain.Enums;
using LampFactory.Domain.Inventory;

namespace LampFactory.Domain.Tests.Inventory;

public class WarehouseTests
{
    [Fact]
    public void Warehouse_WhenCreated_HasNoNameOrAddress()
    {
        var warehouse = new Warehouse();

        Assert.Null(warehouse.Name);
        Assert.Null(warehouse.Address);
        Assert.Equal(default, warehouse.WarehouseType);
    }

    [Fact]
    public void Warehouse_WhenAssignedProperties_RetainsThem()
    {
        var warehouse = new Warehouse
        {
            WarehouseID = 7,
            WarehouseType = WarehouseType.EntryStorage,
            Name = "Main",
            Address = "Tehran"
        };

        Assert.Equal(7, warehouse.WarehouseID);
        Assert.Equal(WarehouseType.EntryStorage, warehouse.WarehouseType);
        Assert.Equal("Main", warehouse.Name);
        Assert.Equal("Tehran", warehouse.Address);
    }

    [Fact]
    public void AuditableEntity_WhenInherited_ExposeAuditColumns()
    {
        var warehouse = new Warehouse { CreatedBy = 3, CreatedAt = new DateTime(2026, 1, 1) };

        Assert.Equal(3, warehouse.CreatedBy);
        Assert.Equal(new DateTime(2026, 1, 1), warehouse.CreatedAt);
        Assert.Null(warehouse.ModifiedBy);
    }
}

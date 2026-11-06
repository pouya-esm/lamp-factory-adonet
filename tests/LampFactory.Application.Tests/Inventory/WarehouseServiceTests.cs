using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Inventory.Warehouses;
using LampFactory.Application.Abstractions.Persistence.Inventory.Warehouses;
using LampFactory.Application.Features.Inventory.Warehouses.Validators;
using LampFactory.Domain.Enums;
using LampFactory.Domain.Inventory;
using LampFactory.Infrastructure.Services.Inventory.Warehouses;
using NSubstitute;

namespace LampFactory.Application.Tests.Inventory;

// Warehouse service behaviour against the application result model. HTTP
// status decisions are asserted separately in the API mapping tests.
public class WarehouseServiceTests
{
    private readonly IWarehouseRepository _repository = Substitute.For<IWarehouseRepository>();
    private readonly WarehouseService _service;

    public WarehouseServiceTests()
    {
        // The real validator exercises the warehouse/stock rules used by the
        // create and update paths under test.
        _service = new WarehouseService(_repository, new WarehouseDetailsDTOValidator());
    }

    [Fact]
    public async Task GetById_WhenIdIsNotPositive_ReturnsValidationError()
    {
        var result = await _service.GetWarehouseByIdAsync(0);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Failure!.Type);
    }

    [Fact]
    public async Task GetById_WhenWarehouseMissing_ReturnsNotFound()
    {
        _repository.GetByIdWithStockAsync(42)
                   .Returns<(Warehouse?, List<WarehouseProduct>)>((null, new List<WarehouseProduct>()));

        var result = await _service.GetWarehouseByIdAsync(42);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Failure!.Type);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetById_WhenWarehouseExists_ReturnsItWithStock()
    {
        var warehouse = new Warehouse { WarehouseID = 5, Name = "Main" };
        var stock = new List<WarehouseProduct> { new WarehouseProduct { ProductID = 1, Quantity = 10 } };
        _repository.GetByIdWithStockAsync(5).Returns<(Warehouse?, List<WarehouseProduct>)>((warehouse, stock));

        var result = await _service.GetWarehouseByIdAsync(5);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Failure);
        Assert.NotNull(result.Value);
        Assert.Equal(5, result.Value!.Warehouse!.WarehouseID);
    }

    [Fact]
    public async Task Create_WhenValidatorFails_ReturnsValidationErrors()
    {
        var dto = new WarehouseDetailsDTO
        {
            Warehouse = new Warehouse { WarehouseID = 1, Name = new string('a', 40) }
        };

        var result = await _service.CreateWarehouseAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Failure!.Type);
        Assert.Contains(result.Failure.ValidationErrors, e => e.Contains("30 characters"));
    }

    [Fact]
    public async Task Create_WhenInsertReportsNoId_ReturnsValidationFailure()
    {
        var dto = new WarehouseDetailsDTO
        {
            Warehouse = new Warehouse { WarehouseType = WarehouseType.EntryStorage, Name = "Main" }
        };
        _repository.CreateWithProductsAsync(Arg.Any<Warehouse>(), Arg.Any<List<WarehouseProduct>>()).Returns(0);

        var result = await _service.CreateWarehouseAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Failure!.Type);
    }

    [Fact]
    public async Task Create_WhenSucceeds_ReturnsTheReloadedWarehouse()
    {
        var dto = new WarehouseDetailsDTO
        {
            Warehouse = new Warehouse { WarehouseType = WarehouseType.EntryStorage, Name = "Main" }
        };
        _repository.CreateWithProductsAsync(Arg.Any<Warehouse>(), Arg.Any<List<WarehouseProduct>>()).Returns(9);
        _repository.GetByIdWithStockAsync(9)
                   .Returns<(Warehouse?, List<WarehouseProduct>)>((dto.Warehouse, new List<WarehouseProduct>()));

        var result = await _service.CreateWarehouseAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Failure);
        await _repository.Received().CreateWithProductsAsync(Arg.Any<Warehouse>(), Arg.Any<List<WarehouseProduct>>());
        await _repository.Received().GetByIdWithStockAsync(9);
    }

    [Fact]
    public async Task Delete_WhenWarehouseMissing_ReturnsNotFound()
    {
        _repository.GetByIdWithStockAsync(3)
                   .Returns<(Warehouse?, List<WarehouseProduct>)>((null, new List<WarehouseProduct>()));

        var result = await _service.DeleteWarehouseAsync(3);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Failure!.Type);
        await _repository.DidNotReceive().RemoveWithProductsAsync(3);
    }

    [Fact]
    public async Task Delete_WhenWarehouseExists_RemovesIt()
    {
        var warehouse = new Warehouse { WarehouseID = 3, Name = "Main" };
        _repository.GetByIdWithStockAsync(3).Returns<(Warehouse?, List<WarehouseProduct>)>((warehouse, new List<WarehouseProduct>()));
        _repository.RemoveWithProductsAsync(3).Returns(true);

        var result = await _service.DeleteWarehouseAsync(3);

        Assert.True(result.IsSuccess);
        await _repository.Received().RemoveWithProductsAsync(3);
    }
}

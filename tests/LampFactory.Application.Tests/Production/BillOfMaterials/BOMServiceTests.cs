using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.BillOfMaterials;
using LampFactory.Application.Abstractions.Persistence.Production.BillOfMaterials;
using LampFactory.Application.Features.Production.BillOfMaterials.Validators;
using LampFactory.Domain.Production;
using LampFactory.Infrastructure.Services.Production.BillOfMaterials;
using NSubstitute;

namespace LampFactory.Application.Tests.Production.BillOfMaterials;

// End-to-end application workflow coverage for the BOM feature: mapping,
// validation gating and read-through of persisted state.
public class BOMServiceTests
{
    private readonly IBOMRepository _repository = Substitute.For<IBOMRepository>();
    private readonly BOMService _service;

    public BOMServiceTests()
    {
        _service = new BOMService(_repository, new BOMDTOValidator());
    }

    private static (BOM, List<BOMComponent>) Aggregate(int id) => (
        new BOM { BOM_ID = id, ProductID = 7 },
        new List<BOMComponent> { new BOMComponent { BOM_ID = id, ProductID = 2, ConsumptionAmount = 1.5 } });

    [Fact]
    public async Task GetAll_ReturnsStronglyTypedList()
    {
        var (bom, components) = Aggregate(1);
        _repository.GetAllWithComponentsAsync()
                   .Returns(new List<(BOM, List<BOMComponent>)> { (bom, components) });

        var result = await _service.GetAllBOMsAsync();

        Assert.True(result.IsSuccess);
        var payload = Assert.Single(result.Value!);
        Assert.Equal(1, payload.BOM.BomId);
        Assert.Equal(2, Assert.Single(payload.Components).ProductID);
    }

    [Fact]
    public async Task GetById_WhenIdIsNotPositive_FailsValidationBeforeLookup()
    {
        var result = await _service.GetBOMByIdAsync(0);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Failure!.Type);
        await _repository.DidNotReceive().GetByIdWithComponentsAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task GetById_WhenBomMissing_ReturnsNotFound()
    {
        _repository.GetByIdWithComponentsAsync(9)
                   .Returns<(BOM?, List<BOMComponent>)>((null, new List<BOMComponent>()));

        var result = await _service.GetBOMByIdAsync(9);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Failure!.Type);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetById_WhenBomExists_ReturnsTypedAggregateWithoutDomainEntities()
    {
        var (bom, components) = Aggregate(4);
        _repository.GetByIdWithComponentsAsync(4)
                   .Returns<(BOM?, List<BOMComponent>)>((bom, components));

        var result = await _service.GetBOMByIdAsync(4);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.BOM.BomId);
        Assert.Equal(7, result.Value.BOM.ProductID);
        Assert.IsType<BOMComponentResponseDTO>(Assert.Single(result.Value.Components));
    }

    [Fact]
    public async Task Create_WhenValidationFails_NeverTouchesRepository()
    {
        var dto = new BOMDTO
        {
            BOM = new BOM { ProductID = 0 },
            Components = new List<BOMComponent> { new BOMComponent { ProductID = 1 } }
        };

        var result = await _service.CreateBOMAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Failure!.Type);
        Assert.Contains(result.Failure.ValidationErrors, e => e.Contains("ProductID"));
        await _repository.DidNotReceive().CreateWithComponentsAsync(Arg.Any<BOM>(), Arg.Any<List<BOMComponent>>());
    }

    [Fact]
    public async Task Create_WhenSucceeds_ReturnsReloadedAggregate()
    {
        var dto = new BOMDTO
        {
            BOM = new BOM { ProductID = 7 },
            Components = new List<BOMComponent> { new BOMComponent { ProductID = 2, ConsumptionAmount = 2 } }
        };
        var (created, createdComponents) = Aggregate(33);
        _repository.CreateWithComponentsAsync(Arg.Any<BOM>(), Arg.Any<List<BOMComponent>>()).Returns(33);
        _repository.GetByIdWithComponentsAsync(33)
                   .Returns<(BOM?, List<BOMComponent>)>((created, createdComponents));

        var result = await _service.CreateBOMAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(33, result.Value!.BOM.BomId);
        Assert.Equal(33, result.Value.Components.Single().BomId);
        await _repository.Received(1).GetByIdWithComponentsAsync(33);
    }

    [Fact]
    public async Task Update_WhenRouteIdDisagreesWithDto_FailsValidationWithoutDbChange()
    {
        var dto = new BOMDTO
        {
            BOM = new BOM { BOM_ID = 99, ProductID = 7 },
            Components = new List<BOMComponent> { new BOMComponent { ProductID = 2 } }
        };

        var result = await _service.UpdateBOMAsync(5, dto);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Failure!.Type);
        Assert.Contains(result.Failure.ValidationErrors, e => e.Contains("BOM ID mismatch"));
        await _repository.DidNotReceive().UpdateWithComponentsAsync(Arg.Any<BOM>(), Arg.Any<List<BOMComponent>>());
    }

    [Fact]
    public async Task Update_WhenBomMissing_ReturnsNotFound()
    {
        var dto = new BOMDTO
        {
            BOM = new BOM { BOM_ID = 5, ProductID = 7 },
            Components = new List<BOMComponent> { new BOMComponent { ProductID = 2 } }
        };
        _repository.GetByIdWithComponentsAsync(5)
                   .Returns<(BOM?, List<BOMComponent>)>((null, new List<BOMComponent>()));

        var result = await _service.UpdateBOMAsync(5, dto);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Failure!.Type);
        await _repository.DidNotReceive().UpdateWithComponentsAsync(Arg.Any<BOM>(), Arg.Any<List<BOMComponent>>());
    }

    [Fact]
    public async Task Update_WhenValid_ReturnsPersistedStateInsteadOfRequestDto()
    {
        var dto = new BOMDTO
        {
            BOM = new BOM { BOM_ID = 5, ProductID = 7 },
            Components = new List<BOMComponent> { new BOMComponent { BOM_ID = 5, ProductID = 2 } }
        };
        // First probe (existence) has no audit stamp; the read-through after
        // the update carries database-written ModifiedAt/ModifiedBy.
        var probes = 0;
        _repository.GetByIdWithComponentsAsync(5)
                   .Returns<(BOM?, List<BOMComponent>)>(_ =>
                   {
                       return probes++ == 0
                           ? (new BOM { BOM_ID = 5, ProductID = 7 }, new List<BOMComponent>())
                           : (new BOM { BOM_ID = 5, ProductID = 7, ModifiedAt = new DateTime(2026, 1, 2), ModifiedBy = 11 },
                              new List<BOMComponent> { new BOMComponent { BOM_ID = 5, ProductID = 2 } });
                   });
        _repository.UpdateWithComponentsAsync(Arg.Any<BOM>(), Arg.Any<List<BOMComponent>>()).Returns(true);

        var result = await _service.UpdateBOMAsync(5, dto);

        Assert.True(result.IsSuccess);
        // The response follows the persisted database state, not the request
        // payload: server-written stamps are part of the returned DTO.
        Assert.Equal(7, result.Value!.BOM.ProductID);
        Assert.Equal(2, result.Value.Components.Single().ProductID);
        Assert.Equal(new DateTime(2026, 1, 2), result.Value.BOM.ModifiedAt);
        Assert.Equal(11, result.Value.BOM.ModifiedBy);
        await _repository.Received().UpdateWithComponentsAsync(
            Arg.Is<BOM>(b => b.BOM_ID == 5), Arg.Any<List<BOMComponent>>());
    }

    [Fact]
    public async Task Delete_WhenIdIsNotPositive_FailsValidation()
    {
        var result = await _service.DeleteBOMAsync(0);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Failure!.Type);
        await _repository.DidNotReceive().RemoveWithComponentsAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Delete_WhenBomMissing_ReturnsNotFound()
    {
        _repository.GetByIdWithComponentsAsync(3)
                   .Returns<(BOM?, List<BOMComponent>)>((null, new List<BOMComponent>()));

        var result = await _service.DeleteBOMAsync(3);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Failure!.Type);
        await _repository.DidNotReceive().RemoveWithComponentsAsync(3);
    }

    [Fact]
    public async Task Delete_WhenSucceeds_ReturnsPayloadFreeSuccess()
    {
        _repository.GetByIdWithComponentsAsync(3)
                   .Returns<(BOM?, List<BOMComponent>)>((new BOM { BOM_ID = 3 }, new List<BOMComponent>()));
        _repository.RemoveWithComponentsAsync(3).Returns(true);

        var result = await _service.DeleteBOMAsync(3);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Failure);
        await _repository.Received().RemoveWithComponentsAsync(3);
    }

    [Fact]
    public async Task UnexpectedRepositoryException_IsNotSwallowed()
    {
        _repository.GetAllWithComponentsAsync()
                   .Returns<Task<List<(BOM, List<BOMComponent>)>>>(call => throw new InvalidOperationException("db down"));

        // Unexpected exceptions propagate to the centralized handler instead of
        // being masked as an InternalServerError result.
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetAllBOMsAsync());
    }
}

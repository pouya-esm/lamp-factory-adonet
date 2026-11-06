using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.ProductionPlans;
using LampFactory.Application.Abstractions.Persistence.Production.ProductionPlans;
using LampFactory.Application.Features.Production.ProductionPlans.Validators;
using LampFactory.Domain.Production;
using LampFactory.Infrastructure.Services.Production.ProductionPlans;
using NSubstitute;

namespace LampFactory.Application.Tests.Production;

// ProductionPlan service behaviour against the application result model.
// HTTP status decisions are asserted separately in the API mapping tests.
public class ProductionPlanServiceTests
{
    private readonly IProductionPlanRepository _repository = Substitute.For<IProductionPlanRepository>();
    private readonly ProductionPlanService _service;

    public ProductionPlanServiceTests()
    {
        _service = new ProductionPlanService(_repository, new ProductionPlanDTOValidator());
    }

    [Fact]
    public async Task GetAll_WhenNoneExist_ReturnsEmptyList()
    {
        _repository.GetAllWithProductsAsync()
                   .Returns(new List<(ProductionPlan Plan, List<ProductionPlanProduct> Products)>());

        var result = await _service.GetAllProductionPlansAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task Create_WhenNoProductsAttached_FailsValidation()
    {
        var dto = new ProductionPlanDTO
        {
            ProductionPlan = new ProductionPlan { Priority = 1 }
        };

        var result = await _service.CreateProductionPlanAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Failure!.Type);
    }

    [Fact]
    public async Task Create_WhenValid_PersistsPlanAndProducts()
    {
        var dto = new ProductionPlanDTO
        {
            ProductionPlan = new ProductionPlan { Priority = 1 },
            Products = new List<ProductionPlanProduct> { new ProductionPlanProduct { ProductID = 2, MinQuantity = 5, MaxQuantity = 5 } }
        };
        _repository.CreateWithProductsAsync(Arg.Any<ProductionPlan>(), Arg.Any<List<ProductionPlanProduct>>()).Returns(11);
        // The service re-reads the aggregate after inserting it so the response
        // carries the server-written audit columns, so stub the read-through.
        _repository.GetByIdWithProductsAsync(11)
                   .Returns<(ProductionPlan?, List<ProductionPlanProduct>)>((new ProductionPlan { ProductionPlanId = 11 }, new List<ProductionPlanProduct>()));

        var result = await _service.CreateProductionPlanAsync(dto);

        Assert.True(result.IsSuccess);
        await _repository.Received().CreateWithProductsAsync(
            Arg.Any<ProductionPlan>(), Arg.Any<List<ProductionPlanProduct>>());
    }

    [Fact]
    public async Task GetById_WhenPlanMissing_ReturnsNotFound()
    {
        // A missing aggregate is signalled by a null root inside the tuple,
        // never by a null tuple itself.
        _repository.GetByIdWithProductsAsync(9)
                   .Returns<(ProductionPlan?, List<ProductionPlanProduct>)>((null, new List<ProductionPlanProduct>()));

        var result = await _service.GetProductionPlanByIdAsync(9);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Failure!.Type);
        Assert.Null(result.Value);
    }
}

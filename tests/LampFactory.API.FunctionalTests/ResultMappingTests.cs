using System.Net;
using System.Text.Json;
using LampFactory.API.Common;
using LampFactory.API.Common.Models;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.BillOfMaterials;
using Microsoft.AspNetCore.Mvc;

namespace LampFactory.API.FunctionalTests;

// Asserts the single ErrorType -> HTTP mapping table plus the envelope and
// status-decision helpers. Pure unit exercises; the mapping is the same code
// every controller calls.
public class ResultMappingTests
{
    // One cached instance per test class: creating serializer options per call
    // is expensive (CA1869).
    private static readonly JsonSerializerOptions WireSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private sealed class NoRoutesController : ControllerBase { }

    [Theory]
    [InlineData(ErrorType.Validation, HttpStatusCode.BadRequest)]
    [InlineData(ErrorType.NotFound, HttpStatusCode.NotFound)]
    [InlineData(ErrorType.Conflict, HttpStatusCode.Conflict)]
    [InlineData(ErrorType.Unauthorized, HttpStatusCode.Unauthorized)]
    [InlineData(ErrorType.Forbidden, HttpStatusCode.Forbidden)]
    public void ToActionResult_Failure_MapsCategoryToHttpStatus(ErrorType type, HttpStatusCode expected)
    {
        var error = type switch
        {
            ErrorType.Validation => AppError.Validation("invalid"),
            ErrorType.NotFound => AppError.NotFound("missing"),
            ErrorType.Conflict => AppError.Conflict("busy"),
            ErrorType.Unauthorized => AppError.Unauthorized(),
            _ => AppError.Forbidden()
        };

        var action = Result<string>.Fail(error).ToActionResult();

        var objectResult = (ObjectResult)action.Result!;
        Assert.Equal((int)expected, objectResult.StatusCode);
        var envelope = (APIResponse<string>)objectResult.Value!;
        Assert.False(envelope.IsSuccess);
        Assert.Equal(expected, envelope.StatusCode);
        Assert.Contains(envelope.ErrorMessage!, e => e.Length > 0);
        Assert.Null(envelope.Result);
    }

    [Fact]
    public void ToActionResult_Success_Returns200WithTypedPayload()
    {
        var payload = new BOMWithComponentsDTO();

        var action = Result<BOMWithComponentsDTO>.Success(payload).ToActionResult();

        var ok = Assert.IsType<OkObjectResult>(action.Result, exactMatch: false);
        Assert.Equal(200, ok.StatusCode);
        var envelope = (APIResponse<BOMWithComponentsDTO>)ok.Value!;
        Assert.True(envelope.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, envelope.StatusCode);
        Assert.Same(payload, envelope.Result);
    }

    [Fact]
    public void ToActionResult_ValidationErrors_ReturnDetailsNotMessage()
    {
        var action = Result<string>.BadRequest("Validation failed", new List<string> { "a", "b" }).ToActionResult();

        var envelope = (APIResponse<string>)((ObjectResult)action.Result!).Value!;
        Assert.Equal(new List<string> { "a", "b" }, envelope.ErrorMessage);
    }

    [Fact]
    public void NonGenericToActionResult_Success_Returns200WithoutPayload()
    {
        var action = Result.Success().ToActionResult();

        var ok = Assert.IsType<OkObjectResult>(action.Result, exactMatch: false);
        var envelope = (APIResponse)ok.Value!;
        Assert.True(envelope.IsSuccess);
        Assert.Null(envelope.Result);
    }

    [Fact]
    public void ToNoContentActionResult_Success_Returns204WithoutBody()
    {
        var action = Result.Success().ToNoContentActionResult();

        var noContent = Assert.IsType<NoContentResult>(action.Result, exactMatch: false);
        Assert.Equal(204, noContent.StatusCode);
    }

    [Fact]
    public void ToNoContentActionResult_Failure_MapsToErrorStatus()
    {
        var action = Result.NotFound("BOM not found for deletion.").ToNoContentActionResult();

        var objectResult = (ObjectResult)action.Result!;
        Assert.Equal(404, objectResult.StatusCode);
        var envelope = (APIResponse<object>)objectResult.Value!;
        Assert.False(envelope.IsSuccess);
        Assert.Contains(envelope.ErrorMessage!, e => e == "BOM not found for deletion.");
    }

    [Fact]
    public void ToCreatedAtActionResult_WithRouteValues_Returns201WithLocation()
    {
        var payload = new BOMResponseDTO { BomId = 12 };

        var action = Result<BOMResponseDTO>.Success(payload)
            .ToCreatedAtActionResult(new NoRoutesController(), nameof(FakeBomEndpoints.Get), v => new { id = v!.BomId });

        var created = (CreatedAtActionResult)action.Result!;
        Assert.Equal(201, created.StatusCode);
        Assert.Equal("Get", created.ActionName);
        Assert.Equal(12, created.RouteValues!["id"]);
        var envelope = (APIResponse<BOMResponseDTO>)created.Value!;
        Assert.True(envelope.IsSuccess);
        Assert.Same(payload, envelope.Result);
    }

    [Fact]
    public void ToCreatedAtActionResult_WithoutRouteValues_ReturnsPlain201WithoutFabricatedLocation()
    {
        var action = Result<object>.Success(new { Voucher = 1 })
            .ToCreatedAtActionResult(new NoRoutesController());

        var objectResult = (ObjectResult)action.Result!;
        Assert.Equal(201, objectResult.StatusCode);
        Assert.True(((APIResponse<object>)objectResult.Value!).IsSuccess);
    }

    // The wire contract camelCase/field shape that lamp-factory-ui relies on.
    [Fact]
    public void ResponseDtos_SerializeWithTheHistoricalWireShape()
    {
        var payload = new BOMWithComponentsDTO
        {
            BOM = new BOMResponseDTO { BomId = 1, ProductID = 2 },
            Components = new List<BOMComponentResponseDTO> { new BOMComponentResponseDTO { BomId = 1, ProductID = 3, ConsumptionAmount = 2.5 } }
        };

        var json = JsonSerializer.Serialize(payload, WireSerializerOptions);

        Assert.Contains("\"bom\":{", json);
        Assert.Contains("\"boM_ID\":1", json);
        Assert.Contains("\"productID\":2", json);
        Assert.Contains("\"components\":[{", json);
        Assert.Contains("\"consumptionAmount\":2.5", json);
    }
}

// Provides an action named Get for nameof(...) resolution in the tests above.
internal static class FakeBomEndpoints
{
    public interface IFakeBomEndpointsService;

    public static object Get(int id) => id;
}

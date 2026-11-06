using LampFactory.Application.Common.Results;

namespace LampFactory.Application.Tests.Common.Results;

// Core contract of the application result model.
public class ResultTests
{
    [Fact]
    public void Success_CarriesValueAndNoFailure()
    {
        var result = Result<string>.Success("payload");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Failure);
        Assert.Equal("payload", result.Value);
    }

    [Fact]
    public void NotFound_FailureCarriesTypeAndValueStaysDefault()
    {
        var result = Result<object>.NotFound("BOM not found.");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Failure!.Type);
        Assert.Equal("BOM not found.", result.Failure.Message);
        Assert.Null(result.Value);
    }

    [Fact]
    public void BadRequest_IsValidationCategoryWithSuppliedErrors()
    {
        var errors = new List<string> { "Valid ProductID is required for BOM." };

        var result = Result<int>.BadRequest("Validation failed", errors);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Failure!.Type);
        Assert.Equal(errors, result.Failure.ValidationErrors);
    }

    [Fact]
    public void Conflict_IsConflictCategory()
    {
        var result = Result.Conflict("BOM already referenced.");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Failure!.Type);
    }

    [Fact]
    public void PlainSuccess_HasNoPayloadAndNoFailure()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Failure);
    }

    [Fact]
    public void Unauthorized_And_Forbidden_MapToTheirCategories()
    {
        Assert.Equal(ErrorType.Unauthorized, Result.Unauthorized().Failure!.Type);
        Assert.Equal(ErrorType.Forbidden, Result.Forbidden().Failure!.Type);
    }

    [Fact]
    public void Categories_AreStableIdentifiers()
    {
        // The API layer maps these to status codes; the set must not drift.
        Assert.Equal(0, (int)ErrorType.None);
        Assert.Equal(1, (int)ErrorType.Validation);
        Assert.Equal(2, (int)ErrorType.NotFound);
        Assert.Equal(3, (int)ErrorType.Conflict);
        Assert.Equal(4, (int)ErrorType.Unauthorized);
        Assert.Equal(5, (int)ErrorType.Forbidden);
    }
}

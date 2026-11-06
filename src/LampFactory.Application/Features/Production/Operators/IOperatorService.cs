using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.Operators;
using LampFactory.Domain.Operators;

namespace LampFactory.Application.Features.Production.Operators;

public interface IOperatorService
{
    Task<Result<List<Operator>>> GetAllOperatorsAsync();
    Task<Result<Operator>> GetOperatorByIdAsync(int id);
    Task<Result<Operator>> CreateOperatorAsync(OperatorDTO dto);
    Task<Result<Operator>> UpdateOperatorAsync(int id, OperatorDTO dto);
    Task<Result> DeleteOperatorAsync(int id);
}
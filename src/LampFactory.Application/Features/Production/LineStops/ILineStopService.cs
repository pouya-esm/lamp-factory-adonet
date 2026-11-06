using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.LineStops;
using LampFactory.Domain.Production;

namespace LampFactory.Application.Features.Production.LineStops;

public interface ILineStopService
{
    Task<Result<List<LineStop>>> GetAllAsync();
    Task<Result<LineStop>> GetByIdAsync(int id);
    Task<Result<LineStop>> CreateAsync(LineStopDTO dto);
    Task<Result<LineStop>> UpdateAsync(int id, LineStopDTO dto);
    Task<Result> DeleteAsync(int id);
}
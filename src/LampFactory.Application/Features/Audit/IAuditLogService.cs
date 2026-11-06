using LampFactory.Application.Common.Results;

namespace LampFactory.Application.Features.Audit;

public interface IAuditLogService
{
    Task<Result<object>> GetAllAsync(string? entityName, int? page, int? pageSize);
    Task<Result<List<string>>> GetEntityNamesAsync();
}
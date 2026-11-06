
using LampFactory.Application.Features.Audit;

namespace LampFactory.Application.Abstractions.Persistence.Audit;

public interface IAuditLogRepository
{
    Task<List<AuditLogDTO>> GetAllAsync(string? entityName, int? page, int? pageSize);
    Task<int> CountAsync(string? entityName);
    Task<List<string>> GetEntityNamesAsync();
}

using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Audit;
using LampFactory.Application.Abstractions.Persistence.Audit;

namespace LampFactory.Infrastructure.Services.Audit
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<Result<object>> GetAllAsync(string? entityName, int? page, int? pageSize)
        {
            var logs = await _auditLogRepository.GetAllAsync(entityName, page, pageSize);
            var total = await _auditLogRepository.CountAsync(entityName);

            int currentPage = (page.HasValue && page.Value > 0) ? page.Value : 1;
            int currentPageSize = (pageSize.HasValue && pageSize.Value > 0) ? pageSize.Value : 50;

            return Result<object>.Success(new
            {
                Items = logs,
                TotalCount = total,
                Page = currentPage,
                PageSize = currentPageSize,
                TotalPages = (int)Math.Ceiling((double)total / currentPageSize)
            });
        }

        public async Task<Result<List<string>>> GetEntityNamesAsync()
        {

            var names = await _auditLogRepository.GetEntityNamesAsync();
            return Result<List<string>>.Success(names);
        }
    }
}
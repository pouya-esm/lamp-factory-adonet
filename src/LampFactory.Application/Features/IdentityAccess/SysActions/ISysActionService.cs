using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.SysActions;
using LampFactory.Domain.IdentityAccess;

namespace LampFactory.Application.Features.IdentityAccess.SysActions;

public interface ISysActionService
{
    Task<Result<List<SysAction>>> GetAllActionsAsync();
    Task<Result<SysAction>> GetActionByIdAsync(int id);
    Task<Result<SysAction>> CreateActionAsync(SysActionDTO dto);
    Task<Result<SysAction>> UpdateActionAsync(int id, SysActionDTO dto);
    Task<Result> DeleteActionAsync(int id);
}
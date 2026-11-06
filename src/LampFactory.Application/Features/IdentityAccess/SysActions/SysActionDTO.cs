using LampFactory.Application.Features.IdentityAccess.SysActions;

namespace LampFactory.Application.Features.IdentityAccess.SysActions
{
    // Write-side contract for the SysAction aggregate. The API binds this, not the
    // Domain entity, so the presentation layer never shapes aggregate state.
    public class SysActionDTO
    {
        public SysAction SysAction { get; set; } = new();
    }
}

using LampFactory.Application.Features.IdentityAccess.Resources;

namespace LampFactory.Application.Features.IdentityAccess.Resources
{
    // Write-side contract for the Resource aggregate. The API binds this, not the
    // Domain entity, so the presentation layer never shapes aggregate state.
    // The flat ResourceDTO is a separate read projection for the sidebar.
    public class ResourceDetailsDTO
    {
        public Resource Resource { get; set; } = new();
    }
}

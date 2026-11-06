using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.Resources;
using LampFactory.Domain.IdentityAccess;

namespace LampFactory.Application.Features.IdentityAccess.Resources;

public interface IResourceService
{
    Task<Result<List<ResourceDTO>>> GetAccessibleResourcesAsync();
    Task<Result<List<Resource>>> GetAllResourcesAsync();
    Task<Result<Resource>> GetResourceByIdAsync(int id);
    Task<Result<Resource>> CreateResourceAsync(ResourceDetailsDTO dto);
    Task<Result<Resource>> UpdateResourceAsync(int id, ResourceDetailsDTO dto);
    Task<Result> DeleteResourceAsync(int id);
}
using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Abstractions.Security;
using LampFactory.Application.Features.IdentityAccess.Permissions;
using LampFactory.Application.Features.IdentityAccess.Resources;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Permissions;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Resources;
using LampFactory.Domain.IdentityAccess;

namespace LampFactory.Infrastructure.Services.IdentityAccess.Resources
{
    public class ResourceService : IResourceService
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<ResourceDetailsDTO> _validator;

        public ResourceService(IResourceRepository resourceRepository, IPermissionRepository permissionRepository, ICurrentUserService currentUserService, IValidator<ResourceDetailsDTO> validator)
        {
            _resourceRepository = resourceRepository;
            _permissionRepository = permissionRepository;
            _currentUserService = currentUserService;
            _validator = validator;
        }

        public async Task<Result<List<ResourceDTO>>> GetAccessibleResourcesAsync()
        {

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return Result<List<ResourceDTO>>.Unauthorized("User is not authenticated.");
            }

            var resources = await _permissionRepository.GetAccessibleResourcesAsync(userId.Value);
            var resultList = resources.Select(r => new ResourceDTO
            {
                ResourceID = r.ResourceID,
                Name = r.Name
            }).ToList();

            return Result<List<ResourceDTO>>.Success(resultList);
        }

        public async Task<Result<List<Resource>>> GetAllResourcesAsync()
        {

            var resources = await _resourceRepository.GetAllAsync();
            return Result<List<Resource>>.Success(resources);
        }

        public async Task<Result<Resource>> GetResourceByIdAsync(int id)
        {

            if (id <= 0) return Result<Resource>.BadRequest("ID mismatch.");

            var resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null) return Result<Resource>.NotFound("Resource not found.");

            return Result<Resource>.Success(resource);
        }

        public async Task<Result<Resource>> CreateResourceAsync(ResourceDetailsDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<Resource>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newResourceId = await _resourceRepository.CreateAsync(dto.Resource);
            if (newResourceId <= 0) return Result<Resource>.BadRequest("Error creating resource.");

            var created = await _resourceRepository.GetByIdAsync(newResourceId);
            if (created == null) return Result<Resource>.NotFound("Resource could not be loaded after creation.");

            return Result<Resource>.Success(created);
        }

        public async Task<Result<Resource>> UpdateResourceAsync(int id, ResourceDetailsDTO dto)
        {

            if (dto == null || dto.Resource == null || id != dto.Resource.ResourceID)
                return Result<Resource>.BadRequest("ID mismatch.");

            var existing = await _resourceRepository.GetByIdAsync(id);
            if (existing == null) return Result<Resource>.NotFound("Resource not found.");

            var success = await _resourceRepository.UpdateAsync(dto.Resource);
            if (!success) return Result<Resource>.BadRequest("Error editing resource.");

            var updated = await _resourceRepository.GetByIdAsync(id);
            if (updated == null) return Result<Resource>.NotFound("Resource could not be loaded after update.");

            return Result<Resource>.Success(updated);
        }

        public async Task<Result> DeleteResourceAsync(int id)
        {

            var existing = await _resourceRepository.GetByIdAsync(id);
            if (existing == null) return Result.NotFound("Resource not found.");

            var success = await _resourceRepository.RemoveAsync(id);
            if (!success) return Result.BadRequest("Error removing resource.");

            return Result.Success();
        }
    }
}
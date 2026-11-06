using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.Resources;
using LampFactory.Application.Abstractions.Security;
using LampFactory.Application.Features.IdentityAccess.Permissions;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Permissions;

namespace LampFactory.Infrastructure.Services.IdentityAccess.Permissions
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<PermissionCreateDTO> _validator;

        public PermissionService(IPermissionRepository permissionRepository, ICurrentUserService currentUserService, IValidator<PermissionCreateDTO> validator)
        {
            _permissionRepository = permissionRepository;
            _currentUserService = currentUserService;
            _validator = validator;
        }

        public async Task<bool> HasUserPermissionAsync(int userId, string resourceName, string actionName)
        {
            return await _permissionRepository.CheckUserPermissionAsync(userId, resourceName, actionName);
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

        public async Task<Result<List<PermissionDTO>>> GetAllPermissionsAsync()
        {

            var permissions = await _permissionRepository.GetAllAsync();
            var resultList = permissions.Select(p => new PermissionDTO
            {
                PermissionID = p.PermissionID,
                ResourceID = p.ResourceID,
                ResourceName = p.Resource?.Name ?? string.Empty,
                ActionID = p.ActionID,
                ActionName = p.Action?.Name ?? string.Empty,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy,
                ModifiedAt = p.ModifiedAt,
                ModifiedBy = p.ModifiedBy
            }).ToList();

            return Result<List<PermissionDTO>>.Success(resultList);
        }

        public async Task<Result<PermissionDTO>> GetPermissionByIdAsync(int id)
        {

            if (id <= 0)
                return Result<PermissionDTO>.BadRequest("Invalid ID.");

            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null)
                return Result<PermissionDTO>.NotFound("Permission not found.");

            var dto = new PermissionDTO
            {
                PermissionID = permission.PermissionID,
                ResourceID = permission.ResourceID,
                ResourceName = permission.Resource?.Name ?? string.Empty,
                ActionID = permission.ActionID,
                ActionName = permission.Action?.Name ?? string.Empty
            };

            return Result<PermissionDTO>.Success(dto);
        }

        public async Task<Result<PermissionDTO>> CreatePermissionAsync(int resourceId, int actionId)
        {

            var dto = new PermissionCreateDTO { ResourceID = resourceId, ActionID = actionId };
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<PermissionDTO>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var permission = new Permission
            {
                ResourceID = resourceId,
                ActionID = actionId
            };

            var newPermissionId = await _permissionRepository.CreateAsync(permission);
            if (newPermissionId <= 0)
                return Result<PermissionDTO>.BadRequest("Failed to create permission.");

            var created = await _permissionRepository.GetByIdAsync(newPermissionId);
            if (created == null)
                return Result<PermissionDTO>.NotFound("Permission could not be loaded after creation.");

            return Result<PermissionDTO>.Success(new PermissionDTO
            {
                PermissionID = created.PermissionID,
                ResourceID = created.ResourceID,
                ResourceName = created.Resource?.Name ?? string.Empty,
                ActionID = created.ActionID,
                ActionName = created.Action?.Name ?? string.Empty
            });
        }

        public async Task<Result<string>> UpdatePermissionAsync(int id, int resourceId, int actionId)
        {

            if (id <= 0)
                return Result<string>.BadRequest("Invalid ID.");

            var dto = new PermissionCreateDTO { ResourceID = resourceId, ActionID = actionId };
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<string>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var permission = new Permission
            {
                PermissionID = id,
                ResourceID = resourceId,
                ActionID = actionId
            };

            var success = await _permissionRepository.UpdateAsync(permission);
            if (!success)
                return Result<string>.NotFound("Permission not found or failed to update.");

            return Result<string>.Success("Permission updated successfully.");
        }

        public async Task<Result> DeletePermissionAsync(int id)
        {

            var success = await _permissionRepository.RemoveAsync(id);
            if (!success)
                return Result.NotFound("Permission not found.");

            return Result.Success();
        }
    }
}
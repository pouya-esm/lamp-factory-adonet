using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.Roles;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Roles;

namespace LampFactory.Infrastructure.Services.IdentityAccess.Roles
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IValidator<RoleCreateDTO> _createValidator;
        private readonly IValidator<RoleUpdateDTO> _updateValidator;

        public RoleService(IRoleRepository roleRepository, IValidator<RoleCreateDTO> createValidator, IValidator<RoleUpdateDTO> updateValidator)
        {
            _roleRepository = roleRepository;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<Result<List<RoleDetailDTO>>> GetAllRolesAsync()
        {

            var roles = await _roleRepository.GetAllAsync();
            var resultList = new List<RoleDetailDTO>();

            foreach (var role in roles)
            {
                var permissionIds = await _roleRepository.GetPermissionIdsForRoleAsync(role.RoleID);
                resultList.Add(new RoleDetailDTO
                {
                    RoleID = role.RoleID,
                    RoleName = role.RoleName,
                    Description = role.Description,
                    PermissionIds = permissionIds,
                    CreatedAt = role.CreatedAt,
                    CreatedBy = role.CreatedBy,
                    ModifiedAt = role.ModifiedAt,
                    ModifiedBy = role.ModifiedBy
                });
            }

            return Result<List<RoleDetailDTO>>.Success(resultList);
        }

        public async Task<Result<RoleDetailDTO>> CreateRoleAsync(string roleName, string? description, List<int> permissionIds)
        {

            var dto = new RoleCreateDTO { RoleName = roleName, Description = description, PermissionIds = permissionIds };
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<RoleDetailDTO>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newRoleId = await _roleRepository.CreateWithPermissionsAsync(roleName, description, permissionIds);
            if (newRoleId <= 0) return Result<RoleDetailDTO>.BadRequest("خطا در ایجاد نقش.");

            var createdPermissionIds = await _roleRepository.GetPermissionIdsForRoleAsync(newRoleId);
            return Result<RoleDetailDTO>.Success(new RoleDetailDTO { RoleID = newRoleId, RoleName = roleName, Description = description, PermissionIds = createdPermissionIds });
        }

        public async Task<Result<string>> UpdateRoleAsync(int id, string roleName, string? description, List<int> permissionIds)
        {

            var dto = new RoleUpdateDTO { RoleName = roleName, Description = description, PermissionIds = permissionIds };
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<string>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var success = await _roleRepository.UpdateWithPermissionsAsync(id, roleName, description, permissionIds);
            if (!success) return Result<string>.BadRequest("خطا در ویرایش نقش.");

            return Result<string>.Success("نقش با موفقیت ویرایش شد.");
        }

        public async Task<Result> DeleteRoleAsync(int id)
        {

            var success = await _roleRepository.RemoveAsync(id);
            if (!success) return Result.NotFound("نقش یافت نشد.");

            return Result.Success();
        }
    }
}
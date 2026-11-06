using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.Users;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Users;

namespace LampFactory.Infrastructure.Services.IdentityAccess.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IValidator<UserRolesDTO> _validator;

        public UserService(IUserRepository userRepository, IValidator<UserRolesDTO> validator)
        {
            _userRepository = userRepository;
            _validator = validator;
        }

        public async Task<Result<List<UserRolesDTO>>> GetAllUsersAsync()
        {

            var users = await _userRepository.GetAllAsync();
            var userRolesList = new List<UserRolesDTO>();

            foreach (var user in users)
            {
                var roleIds = await _userRepository.GetRoleIdsForUserAsync(user.UserID);
                userRolesList.Add(new UserRolesDTO
                {
                    User = user,
                    RoleIds = roleIds
                });
            }

            return Result<List<UserRolesDTO>>.Success(userRolesList);
        }

        public async Task<Result<UserRolesDTO>> GetUserByIdAsync(int id)
        {

            if (id <= 0) return Result<UserRolesDTO>.BadRequest("Invalid ID.");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return Result<UserRolesDTO>.NotFound("User not found.");

            var roleIds = await _userRepository.GetRoleIdsForUserAsync(user.UserID);
            var result = new UserRolesDTO { User = user, RoleIds = roleIds };

            return Result<UserRolesDTO>.Success(result);
        }

        public async Task<Result<UserRolesDTO>> CreateUserAsync(UserRolesDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<UserRolesDTO>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newUserId = await _userRepository.CreateWithRolesAsync(dto.User, dto.RoleIds);
            if (newUserId <= 0) return Result<UserRolesDTO>.BadRequest("Error creating user.");

            var createdUser = await _userRepository.GetByIdAsync(newUserId);
            if (createdUser == null) return Result<UserRolesDTO>.NotFound("User could not be loaded after creation.");

            var createdRoleIds = await _userRepository.GetRoleIdsForUserAsync(newUserId);
            return Result<UserRolesDTO>.Success(new UserRolesDTO { User = createdUser, RoleIds = createdRoleIds });
        }

        public async Task<Result<UserRolesDTO>> UpdateUserAsync(int id, UserRolesDTO dto)
        {

            var context = new ValidationContext<UserRolesDTO>(dto);
            context.RootContextData["RouteId"] = id;

            var validationResult = await _validator.ValidateAsync(context);
            if (!validationResult.IsValid)
            {
                return Result<UserRolesDTO>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null) return Result<UserRolesDTO>.NotFound("User not found.");

            var success = await _userRepository.UpdateWithRolesAsync(dto.User, dto.RoleIds);
            if (!success) return Result<UserRolesDTO>.BadRequest("Error editing user.");

            return Result<UserRolesDTO>.Success(dto);
        }

        public async Task<Result> DeleteUserAsync(int id)
        {

            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null) return Result.NotFound("User not found.");

            var success = await _userRepository.RemoveAsync(id);
            if (!success) return Result.BadRequest("Error deleting user.");

            return Result.Success();
        }
    }
}
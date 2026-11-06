using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.Users;

namespace LampFactory.Application.Features.IdentityAccess.Users;

public interface IUserService
{
    Task<Result<List<UserRolesDTO>>> GetAllUsersAsync();
    Task<Result<UserRolesDTO>> GetUserByIdAsync(int id);
    Task<Result<UserRolesDTO>> CreateUserAsync(UserRolesDTO dto);
    Task<Result<UserRolesDTO>> UpdateUserAsync(int id, UserRolesDTO dto);
    Task<Result> DeleteUserAsync(int id);
}
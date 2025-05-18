
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_service.Interface;

public interface IUserService
{
    Task<User?> GetUserByEmailAsync(string email);

    Task<User?> GetUserByUsernameAsync(string username);

    Task<User?> AuthenicateUserAsync(string email, string password);

    Task<string> GenerateJwttokenAsync(string email, int roleId);

    string ExtractEmailFromToken(string token);

    Task<ResetPasswordResult> ResetPasswordAsync(string token, string newPassword, string confirmPassword);

    Task<string?> GeneratePasswordResetTokenAsync(string email);

    Task<ProfileViewModel?> GetUserProfileAsync(string email);

    Task<bool> UpdateUserProfileAsync(string email, ProfileViewModel model);

    Task<string?> ChangePasswordAsync(string email, ChangePasswordViewModel model);

    Task<PagedResult<UserTableViewModel>> GetUsersAsync(UserPaginationViewModel model);

    Task<bool> DeleteUserAsync(int id);

    Task<List<Role>> GetRolesAsync();

    Task<bool> AddUserAsync(AddUserViewModel model);

    Task<EditUserViewModel> GetUserForEditAsync(int id);

    Task<bool> EditUserAsync(int id, EditUserViewModel model);

    Task<List<RolePermissionViewModel>> GetPermissionsByRoleAsync(string roleName);

    Task<bool> UpdateRolePermissionsAsync(List<RolePermissionViewModel> permissions);

    Task<DashboardViewModel> GetDashboardDataAsync(string filter, DateTime? customStartDate = null, DateTime? customEndDate = null);

}

using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_repository.Interface;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);

    Task <User?> GetUserByUsernameAsync(string username);

    Task<User?> GetUserByResetTokenAsync(string token);

    Task<bool> UpdateUserAsync(User user);

    Task<string?> GetUserRoleAsync(int roleId);

    Task<int> GetUserIdByEmailAsync(string Email);

    Task<User?> GetUserByEmailAndRoleAsync(string email);

    Task<IQueryable<User>> GetAllUsersWithRolesAsync();
    Task<User?> GetUserByIdAsync(int id);

    Task SoftDeleteUserAsync(User user);

    Task<List<Role>> GetRolesAsync();

    Task<Role> GetRoleByIdAsync(int id);

    Task AddUserAsync(User user);

    Task<User?> GetUserByIdAndRoleAsync(int id);

    Task<Role?> GetRoleByNameAsync(string roleName);

    Task<List<Roleandpermission>> GetRolePermissionsByRoleIdAsync(int roleId);

    Task<Roleandpermission?> GetRolePermissionByRoleAndPermissionAsync(string? roleName, int? permissionId);

    Task UpdateRolePermissionAsync(Roleandpermission rolePermission);

    Task<List<Order>> GetOrdersInRangeAsync(DateTime start, DateTime end);

    Task<List<OrderItemsMapping>> GetServedItemsAsync(DateTime start, DateTime end);

    Task<List<(DateTime Date, int Count)>> GetDailyCustomerCountsAsync(DateTime start, DateTime end);

    Task<List<TopItem>> GetTopItemsAsync(DateTime start, DateTime end);

    Task<List<TopItem>> GetLeastItemsAsync(DateTime start, DateTime end);

    Task<List<(DateTime Createdat, DateTime? Servingtime)>> GetServedOrdersAsync(DateTime start, DateTime end);

    Task<int> GetWaitingCountAsync(DateTime start, DateTime end);

    Task<int> GetNewCustomerCountAsync(DateTime start, DateTime end);

}



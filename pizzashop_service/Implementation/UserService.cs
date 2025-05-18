using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop_service.Implementation;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IUserRepository userRepository, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;

    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving the user by email.", ex);
        }
    }


    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        try
        {
            return await _userRepository.GetUserByUsernameAsync(username);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving the user by username.", ex);
        }
    }

    public async Task<User?> AuthenicateUserAsync(string email, string password)
    {
        try
        {
            User? user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            if (!user.Password.StartsWith("$2a$") && !user.Password.StartsWith("$2b$") && !user.Password.StartsWith("$2y$"))
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.Password = hashedPassword;
                await _userRepository.UpdateUserAsync(user);
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return null;
            }

            return user;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while authenticating the user.", ex);
        }
    }

    public async Task<string> GenerateJwttokenAsync(string email, int roleId)
    {
        try
        {
            return await _jwtService.GenerateJwtTokenAsync(email, roleId);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while generating the JWT token.", ex);
        }
    }


    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        try
        {
            User? user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return null;

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.Resettokenexpiry = DateTime.UtcNow.AddHours(1);

            await _userRepository.UpdateUserAsync(user);

            return user.PasswordResetToken;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while generating the password reset token.", ex);
        }
    }



    public async Task<ResetPasswordResult> ResetPasswordAsync(string token, string newPassword, string confirmPassword)
    {
        var result = new ResetPasswordResult();

        try
        {
            if (newPassword != confirmPassword)
            {
                result.Success = false;
                result.Message = "The new password and confirmation password do not match.";
                return result;
            }

            User? user = await _userRepository.GetUserByResetTokenAsync(token);

            if (user == null || user.Resettokenexpiry < DateTime.UtcNow)
            {
                result.Success = false;
                result.Message = "Invalid or expired reset token.";
                return result;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.Resettokenexpiry = null;

            await _userRepository.UpdateUserAsync(user);

            result.Success = true;
            result.Message = "Password has been successfully updated.";
            return result;
        }
        catch (Exception)
        {
            result.Success = false;
            result.Message = "An error occurred while resetting the password.";
            return result;
        }
    }

    public string ExtractEmailFromToken(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                return string.Empty;

            var handler = new JwtSecurityTokenHandler();
            var authToken = handler.ReadJwtToken(token);
            return authToken.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Email)?.Value ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<ProfileViewModel?> GetUserProfileAsync(string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
                return null;

            User? user = await _userRepository.GetUserByEmailAndRoleAsync(email);
            if (user == null)
                return null;

            return new ProfileViewModel
            {
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Username = user.Username,
                Email = user.Email,
                Rolename = user.Role.Rolename,
                CountryId = user.Countryid,
                StateId = user.Stateid,
                CityId = user.Cityid,
                Phone = user.Phone,
                Address = user.Address,
                Zipcode = user.Zipcode,
                ProfileImagePath = user.Profileimagepath
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateUserProfileAsync(string email, ProfileViewModel model)
    {
        try
        {
            User? user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return false;

            user.Firstname = model.Firstname;
            user.Lastname = model.Lastname;
            user.Username = model.Username;
            user.Phone = model.Phone;
            user.Countryid = model.CountryId;
            user.Stateid = model.StateId;
            user.Cityid = model.CityId;
            user.Address = model.Address;
            user.Zipcode = model.Zipcode;

            if (model.ProfileImage != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/users");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (FileStream? fileStream = new(filePath, FileMode.Create))
                {
                    model.ProfileImage.CopyTo(fileStream);
                }

                if (!string.IsNullOrEmpty(user.Profileimagepath))
                {
                    string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Profileimagepath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                user.Profileimagepath = "/images/users/" + uniqueFileName;
            }

            model.ProfileImagePath = user.Profileimagepath;

            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var username = model.Username ?? string.Empty;
                var profilePath = model.ProfileImagePath ?? string.Empty;

                context.Response.Cookies.Append("Username", username);
                context.Response.Cookies.Append("ProfileImgPath", profilePath);
            }
            return await _userRepository.UpdateUserAsync(user);
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> ChangePasswordAsync(string email, ChangePasswordViewModel model)
    {
        try
        {
            User? user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null)
            {
                return "UserNotFound";
            }

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
            {
                return "IncorrectPassword";
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _userRepository.UpdateUserAsync(user);

            return "Success";
        }
        catch (Exception ex)
        {
            throw new Exception("change password error occurs", ex);
        }
    }


    public async Task<PagedResult<UserTableViewModel>> GetUsersAsync(UserPaginationViewModel model)
    {
        try
        {
            var query = await _userRepository.GetAllUsersWithRolesAsync();

            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                query = query.Where(i =>
                    i.Firstname.ToLower().Contains(model.SearchTerm.ToLower()) ||
                    i.Lastname.ToLower().Contains(model.SearchTerm.ToLower()));
            }

            query = model.SortBy switch
            {
                "Name" => model.SortOrder == "asc"
                    ? query.OrderBy(u => u.Firstname)
                    : query.OrderByDescending(u => u.Firstname),

                "Role" => model.SortOrder == "asc"
                    ? query.OrderBy(u => u.Role.Rolename)
                    : query.OrderByDescending(u => u.Role.Rolename),

                _ => query.OrderBy(u => u.Id)
            };

            int totalCount = await query.CountAsync();


            List<UserTableViewModel> users = await query
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .Select(t => new UserTableViewModel
                {
                    Id = t.Id,
                    Firstname = t.Firstname,
                    Lastname = t.Lastname,
                    Email = t.Email,
                    Phone = t.Phone,
                    Rolename = t.Role.Rolename,
                    Status = t.Status,
                    ProfileImagePath = t.Profileimagepath
                })
                .ToListAsync();

            return new PagedResult<UserTableViewModel>(users, model.PageNumber, model.PageSize, totalCount);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to fetch paginated users", ex);
        }
    }


    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            User? user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return false;
            }
            user.Isdeleted = true;
            await _userRepository.SoftDeleteUserAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error occurred while deleting user with ID: {id}", ex);
        }
    }


    public async Task<List<Role>> GetRolesAsync()
    {
        try
        {
            return await _userRepository.GetRolesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error occurred while retrieving roles.", ex);
        }
    }


    public async Task<bool> AddUserAsync(AddUserViewModel model)
    {
        try
        {
            Role? role = await _userRepository.GetRoleByIdAsync(model.RoleId);
            if (role == null)
            {
                return false;
            }

            User? user = new()
            {
                Firstname = model.Firstname,
                Lastname = model.Lastname,
                Email = model.Email,
                Username = model.Username,
                Phone = model.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Roleid = model.RoleId,
                Profileimagepath = model.ProfileImagePath,
                Countryid = model.CountryId,
                Stateid = model.StateId,
                Cityid = model.CityId,
                Address = model.Address,
                Zipcode = model.Zipcode,
                Createdby = role.Rolename
            };

            if (model.ProfileImage != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/users");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (FileStream? fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(fileStream);
                }

                user.Profileimagepath = "/images/users/" + uniqueFileName;
            }

            await _userRepository.AddUserAsync(user);

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Error occurred while adding a user.", ex);
        }
    }

    public async Task<EditUserViewModel> GetUserForEditAsync(int id)
    {
        try
        {
            User? user = await _userRepository.GetUserByIdAsync(id);

            if (user == null) return null;

            return new EditUserViewModel
            {
                id = user.Id,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Email = user.Email,
                Username = user.Username,
                Phone = user.Phone,
                Status = user.Status,
                RoleId = user.Roleid,
                CountryId = user.Countryid,
                StateId = user.Stateid,
                CityId = user.Cityid,
                Address = user.Address,
                Zipcode = user.Zipcode,
                ProfileImagePath = user.Profileimagepath
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Error occurred while fetching user for edit.", ex);
        }
    }


    public async Task<bool> EditUserAsync(int id, EditUserViewModel model)
    {
        try
        {
            User? user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return false;

            bool hasChanges =
                user.Firstname != model.Firstname ||
                user.Lastname != model.Lastname ||
                user.Email != model.Email ||
                user.Username != model.Username ||
                user.Phone != model.Phone ||
                user.Status != model.Status ||
                user.Roleid != model.RoleId ||
                user.Countryid != model.CountryId ||
                user.Stateid != model.StateId ||
                user.Cityid != model.CityId ||
                user.Address != model.Address ||
                user.Zipcode != model.Zipcode ||
                (model.ProfileImage != null && model.ProfileImage.Length > 0) ||
                (model.RemoveProfileImg == "true");

            if (!hasChanges)
            {
                return false;
            }

            user.Firstname = model.Firstname;
            user.Lastname = model.Lastname;
            user.Email = model.Email;
            user.Username = model.Username;
            user.Phone = model.Phone;
            user.Status = model.Status;
            user.Roleid = model.RoleId;
            user.Countryid = model.CountryId;
            user.Stateid = model.StateId;
            user.Cityid = model.CityId;
            user.Address = model.Address;
            user.Zipcode = model.Zipcode;

            if (model.RemoveProfileImg == "true")
            {
                if (!string.IsNullOrEmpty(user.Profileimagepath))
                {
                    string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Profileimagepath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                    user.Profileimagepath = null;
                }
            }

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/users");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (FileStream? fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(fileStream);
                }

                if (!string.IsNullOrEmpty(user.Profileimagepath))
                {
                    string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.Profileimagepath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                user.Profileimagepath = "/images/users/" + uniqueFileName;
            }

            await _userRepository.UpdateUserAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Error occurred while editing the user profile.", ex);
        }
    }


    public async Task<List<RolePermissionViewModel>> GetPermissionsByRoleAsync(string roleName)
    {
        try
        {
            var role = await _userRepository.GetRoleByNameAsync(roleName) ?? throw new Exception($"Role with name {roleName} not found.");

            var rolePermissions = await _userRepository.GetRolePermissionsByRoleIdAsync(role.Id);

            var permissions = rolePermissions.Select(rp => new RolePermissionViewModel
            {
                Permissionid = rp.Permissionid,
                PermissionName = rp.Permission?.Permissiomname ??"Unknown",
                Canview = rp.Canview,
                CanaddEdit = rp.CanaddEdit,
                Candelete = rp.Candelete
            }).ToList();

            return permissions;
        }
        catch
        {
            throw;
        }
    }

    public async Task<bool> UpdateRolePermissionsAsync(List<RolePermissionViewModel> permissions)
    {
        try
        {
            foreach (var permission in permissions)
            {
                var rolePermission = await _userRepository.GetRolePermissionByRoleAndPermissionAsync(permission.Roleid, permission.Permissionid);

                if (rolePermission != null)
                {
                    rolePermission.Canview = permission.Canview;
                    rolePermission.CanaddEdit = permission.CanaddEdit;
                    rolePermission.Candelete = permission.Candelete;

                    await _userRepository.UpdateRolePermissionAsync(rolePermission);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while updating role permissions.", ex);
        }
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync(string filter, DateTime? customStartDate = null, DateTime? customEndDate = null)
    {
        DateTime startDate, endDate;
        var today = DateTime.Today;

        if (filter == "Custom" && customStartDate.HasValue && customEndDate.HasValue)
        {
            startDate = customStartDate.Value.Date;
            endDate = customEndDate.Value.Date.AddDays(1);
        }
        else
        {
            switch (filter)
            {
                case "Today":
                    startDate = today;
                    endDate = today.AddDays(1);
                    break;
                case "Last 7 Days":
                    startDate = today.AddDays(-6);
                    endDate = today.AddDays(1);
                    break;
                case "Last 30 Days":
                    startDate = today.AddDays(-29);
                    endDate = today.AddDays(1);
                    break;
                case "Current Month":
                default:
                    startDate = new DateTime(today.Year, today.Month, 1);
                    endDate = startDate.AddMonths(1);
                    break;
            }
        }

        var ordersInRange = await _userRepository.GetOrdersInRangeAsync(startDate, endDate);
        var servedItems = await _userRepository.GetServedItemsAsync(startDate, endDate);
        var dailyCustomerCounts = await _userRepository.GetDailyCustomerCountsAsync(startDate, endDate);
        var topItems = await _userRepository.GetTopItemsAsync(startDate, endDate);
        var leastItems = await _userRepository.GetLeastItemsAsync(startDate, endDate);
        var servedOrders = await _userRepository.GetServedOrdersAsync(startDate, endDate);
        var waitingCount = await _userRepository.GetWaitingCountAsync(startDate, endDate);
        var newCustomer = await _userRepository.GetNewCustomerCountAsync(startDate, endDate);

        // Aggregations
        var totalSales = ordersInRange.Where(o => o.Status != "Cancalled").Sum(o => o.Totalamount);
        var totalOrders = ordersInRange.Count;
        var avgOrderValue = Math.Round(totalOrders > 0 ? totalSales / totalOrders : 0, 2);

        var revenueChart = ordersInRange
            .GroupBy(o => o.Createdat.Date)
            .Select(g => new ChartDataPoint
            {
                Label = g.Key.ToString("MMM dd"),
                Value = g.Sum(o => o.Totalamount)
            })
            .OrderBy(g => g.Label)
            .ToList();

        var cumulativeList = new List<ChartDataPoint>();
        int runningTotal = 0;
        foreach (var (Date, Count) in dailyCustomerCounts)
        {
            runningTotal += Count;
            cumulativeList.Add(new ChartDataPoint
            {
                Label = Date.ToString("MMM dd"),
                Value = runningTotal
            });
        }

        var waitTimeMinutes = servedOrders
            .Select(o => (o.Servingtime.Value - o.Createdat).TotalMinutes)
            .ToList();

        var avgWaitTime = waitTimeMinutes.Any() ? Math.Round(waitTimeMinutes.Average(), 1) : 0;

        return new DashboardViewModel
        {
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            AverageOrderValue = avgOrderValue,
            RevenueChartData = revenueChart,
            CustomerGrowthData = cumulativeList,
            TopSellingItems = topItems,
            LeastSellingItems = leastItems,
            AverageWaitingTime = avgWaitTime,
            WaitingListCount = waitingCount,
            NewCustomer = newCustomer
        };
    }


}

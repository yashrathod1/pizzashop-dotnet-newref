using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IUserService _useService;

    public DashboardController(IUserService userService)
    {
        _useService = userService;
    }

    public async Task<IActionResult> Index(string filter, DateTime? startDate, DateTime? endDate)
    {   
        ViewBag.ActiveNav = "Dashboard";
        var dashboardData = await _useService.GetDashboardDataAsync(filter, startDate, endDate);
        return View(dashboardData);
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardData(string filter, DateTime? startDate, DateTime? endDate)
    {
        var dashboardData = await _useService.GetDashboardDataAsync(filter, startDate, endDate);
        return Json(dashboardData);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        try
        {   
            ViewBag.ActiveNav = "Dashboard";
            string? token = Request.Cookies["AuthToken"];
            string? email =  _useService.ExtractEmailFromToken(token);

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Login");
            }

            ProfileViewModel? model = await _useService.GetUserProfileAsync(email);

            if (model == null)
            {
                return NotFound("User Not Found");
            }

            ViewBag.Email = email;
            return View(model);
        }
        catch
        {
            TempData["error"] = "An error occurred while fetching the profile.";
            return RedirectToAction("Login", "Login");
        }
    }


    [HttpPost]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        try
        {
            string? token = Request.Cookies["AuthToken"];
            string? email = _useService.ExtractEmailFromToken(token);

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Login");
            }

            bool success = await _useService.UpdateUserProfileAsync(email, model);

            CookieOptions? coockieopt = new()

            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Append("Username", model.Username, coockieopt);
            Response.Cookies.Append("ProfileImgPath", string.IsNullOrEmpty(model.ProfileImagePath) ? "/images/Default_pfp.svg.png" : model.ProfileImagePath, coockieopt);

            if (!success)
            {
                return NotFound("User Not Found");
            }

            TempData["success"] = "Profile updated successfully.";
            return RedirectToAction("Profile", "Dashboard");
        }
        catch
        {
            TempData["error"] = "An error occurred while updating the profile.";
            return RedirectToAction("Profile", "Dashboard");

        }
    }


    [HttpGet]
    public IActionResult ChangePassword()
    {   
        ViewBag.ActiveNav = "Dashboard";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        try
        {
            string? token = Request.Cookies["AuthToken"];
            string? userEmail = _useService.ExtractEmailFromToken(token);

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Login");
            }

            string? result = await _useService.ChangePasswordAsync(userEmail, model);

            if (result == "UserNotFound")
            {
                TempData["error"] = "User not found.";
                return View(model);
            }

            if (result == "IncorrectPassword")
            {
                TempData["error"] = "Current password is incorrect.";
                return View(model);
            }

            TempData["success"] = "Password updated successfully.";
            return RedirectToAction("Login", "Auth");
        }
        catch (Exception)
        {
            TempData["error"] = "An unexpected error occurred.";
            return View(model);
        }
    }


    public IActionResult Logout()
    {
        if (Request.Cookies["UserEmail"] != null)
        {
            Response.Cookies.Delete("UserEmail");
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("Username");
            Response.Cookies.Delete("ProfileImgPath");

        }
        return RedirectToAction("Login", "Auth");
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop.Controllers;

[Authorize]
public class TaxesAndFeesController : Controller
{
    private readonly ITaxesAndFeesService _taxesAndFeesService;

    public TaxesAndFeesController(ITaxesAndFeesService taxesAndFeesService)
    {
        _taxesAndFeesService = taxesAndFeesService;
    }
    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveNav = "TaxsAndFees";
        RolePermissionViewModel? permission = await PermissionHelper.GetPermissionsAsync(HttpContext, "TaxAndFee");

        ViewBag.Permissions = permission;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetTaxesAndFees(PaginationViewModel model)
    {
        RolePermissionViewModel? permission = await PermissionHelper.GetPermissionsAsync(HttpContext, "TaxAndFee");
        ViewBag.Permissions = permission;

        PagedResult<TaxsAndFeesViewModel>? pagedItems = await _taxesAndFeesService.GetTaxesAndFeesAsync(model);
        return PartialView("_TaxesAndFeesPartial", pagedItems);
    }

    [HttpPost]
    public async Task<IActionResult> AddTax(TaxsAndFeesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data provided." });
        }

        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        int userId = int.Parse(userIdClaim ?? "0");

        bool result = await _taxesAndFeesService.AddTaxAsync(model, userId);
        if (result)
        {
            return Json(new { success = true, message = "Tax added successfully." });
        }
        else
        {
            return Json(new { success = false, message = "Failed to add Tax." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTaxById(int id)
    {
        TaxsAndFeesViewModel? tax = await _taxesAndFeesService.GetTaxByIdAsync(id);

        if (tax == null)
        {
            return NotFound();
        }

        return Json(tax);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTax([FromBody] ItemViewModel request)
    {
        bool result = await _taxesAndFeesService.SoftDeleteTaxAsync(request.Id);
        return result ? Ok(new { success = true }) : Json(new { success = false });
    }

    [HttpPost]
    public async Task<IActionResult> EditTax([FromBody] TaxsAndFeesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new
            {
                success = false,
                message = "Invalid data provided.",
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });
        }

        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        int userId = int.Parse(userIdClaim ?? "0");

        bool result = await _taxesAndFeesService.UpdateTaxAsync(model, userId);

        if (!result)
        {
            return Json(new { success = false, message = "An error occurred while updating the table." });
        }

        return Json(new { success = true, message = "Tax updated successfully!" });
    }
}

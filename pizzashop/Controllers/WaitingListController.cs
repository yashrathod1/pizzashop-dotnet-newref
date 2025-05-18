using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop.Controllers;

[Authorize]
public class WaitingListController : Controller
{
    private readonly IWaitingListService _waitingListService;

    public WaitingListController(IWaitingListService waitingListService)
    {
        _waitingListService = waitingListService;
    }

    public async Task<IActionResult> Index()
    {
        WaitingListViewModel? section = await _waitingListService.GetSectionAsync();
        ViewBag.ActiveNav = "WaitingList";
        return View(section);
    }

    [HttpGet]
    public async Task<IActionResult> GetSections()
    {
        List<OrderAppSectionViewModel>? sections = await _waitingListService.GetAllSectionsAsync();
        return Json(sections);
    }

    public async Task<IActionResult> GetWaitingListBySection(int? sectionId)
    {
        WaitingListViewModel? model = await _waitingListService.GetWaitingListAsync(sectionId);
        return PartialView("_WaitingListPartial", model.WaitingList);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWaitingToken(WaitingTokenViewModel waitingTokenVm)
    {
        try
        {
            if (waitingTokenVm == null)
            {
                return Json("Invalid data.");
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int userId = int.Parse(userIdClaim ?? "0");

            var (IsSuccess, Message) = await _waitingListService.AddWaitingTokenInWaitingListAsync(waitingTokenVm, userId);
            return Json(new { success = IsSuccess, message = Message });
        }
        catch (Exception ex)
        {
            throw new Exception("Error In Creating Waiting Token", ex);
        }

    }

    public async Task<IActionResult> GetCustomerByEmail(string email)
    {
        CustomerViewModel? customer = await _waitingListService.GetCustomerByEmail(email);
        return Json(customer);
    }

    [HttpGet]
    public async Task<IActionResult> GetTokenById(int id)
    {
        WaitingListItemViewModel? model = await _waitingListService.GetTokenByIdAsync(id);
        if (model != null)
        {
            return Json(model);
        }
        return Json(null);
    }


    [HttpPost]
    public async Task<IActionResult> EditWaitingToken(WaitingTokenViewModel model)
    {
        try
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int userId = int.Parse(userIdClaim ?? "0");

            var (success, message) = await _waitingListService.EditWaitingTokenAsync(model, userId);

            if (success)
            {
                return Json(new { success = true, message = message });
            }
            else
            {
                return Json(new { success = false, message = message });
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error in updating waiting token", ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SoftDeleteToken(int id)
    {
        try
        {
            bool result = await _waitingListService.SoftDeleteTokenAsync(id);
            if (result)
            {
                return Json(new { success = true, message = "Token soft deleted successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Token not found." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableSectionsAsync()
    {
        List<SectionsViewModal>? sections = await _waitingListService.GetSectionsWithAvailableTablesAsync();
        return Json(sections);
    }
    [HttpGet]
    public async Task<IActionResult> GetAvailableTablesAsync(int sectionId)
    {
        List<TableViewModel>? tables = await _waitingListService.GetAvailableTablesBySectionAsync(sectionId);
        return Json(tables);
    }

    [HttpPost]
    public async Task<IActionResult> AssignTables([FromBody] AssignTableInWaitingTokenViewModel model)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        int userId = int.Parse(userIdClaim ?? "0");

        AssignTableResultViewModel? result = await _waitingListService.AssignTablesToCustomerAsync(model, userId);

        return Json(new
        {
            success = result.IsSuccess,
            message = result.Message,
            orderId = result.OrderId
        });
    }



}
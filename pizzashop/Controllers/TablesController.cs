using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop.Controllers;

[Authorize]
public class TablesController : Controller
{
    private readonly ITableService _tableService;

    public TablesController(ITableService tableService)
    {
        _tableService = tableService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveNav = "Tables";
        TablesOrderAppViewModel? sectionandtable = await _tableService.GetSectionsWithTablesAsync();
        return View(sectionandtable);
    }

    [HttpGet]
    public async Task<IActionResult> GetSections()
    {
        List<OrderAppSectionViewModel>? sections = await _tableService.GetAllSectionsAsync();
        return Json(sections);
    }

    [HttpPost]
    public async Task<IActionResult> AddWaitingToken(WaitingTokenViewModel waitingTokenVm)
    {
        try
        {
            if (waitingTokenVm == null)
            {
                return Json("Invalid data.");
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int userId = int.Parse(userIdClaim ?? "0");

            var (IsSuccess, Message) = await _tableService.AddWaitingTokenAsync(waitingTokenVm, userId);
            return Json(new { success = IsSuccess, message = Message });
        }
        catch (Exception ex)
        {
            throw new Exception("Error In Creating Waiting Token", ex);
        }

    }

    [HttpGet]
    public async Task<IActionResult> GetWaitingTokenList(int sectionId)
    {
        List<WaitingTokenViewModel>? waitingTokens = await _tableService.GetWaitingTokens(sectionId);
        return PartialView("_TableWaitingListPartial", waitingTokens);
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomerDetailsByToken(int tokenId)
    {
        WaitingTokenViewModel? customerDetails = await _tableService.GetCustomerDetailsByToken(tokenId);
        return Json(customerDetails);
    }

    public async Task<IActionResult> GetCustomerByEmail(string email)
    {
        CustomerViewModel? customer = await _tableService.GetCustomerByEmail(email);
        return Json(customer);
    }

    [HttpPost]
    public async Task<IActionResult> AssignTables([FromBody] AssignTableRequestViewModel model)
    {
        try
        {       
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int userId = int.Parse(userIdClaim ?? "0");

            var (IsSuccess, Message) = await _tableService.AssignTablesAsync(model, userId);
            return Json(new { success = IsSuccess, message = Message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "An unexpected error occurred." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderIdByTable(int tableId)
    {
        int? orderId = await _tableService.GetOrderIdByTableIdAsync(tableId);
        return Json(orderId);
    }


}
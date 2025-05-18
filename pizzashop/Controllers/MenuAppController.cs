using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop.Controllers;

[Authorize]
public class MenuAppController : Controller
{
    private readonly IMenuAppService _menuAppService;

    public MenuAppController(IMenuAppService menuAppService)
    {
        _menuAppService = menuAppService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(int orderId)
    {
        ViewBag.orderId = orderId;
        ViewBag.ActiveNav = "Menu";
        MenuAppViewModel? orderdetails = await _menuAppService.GetCategoriesAsync();

        if (orderdetails == null || orderdetails.CategoryList.Count == 0)
        {
            return NotFound();
        }
        return View(orderdetails);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetItems(string type, int? categoryId = null, string? searchTerm = null)
    {
        List<MenuAppItemViewModel> items = (type?.ToLower()) switch
        {
            "all" => await _menuAppService.GetItemsAsync(searchTerm: searchTerm),
            "favorite" => await _menuAppService.GetItemsAsync(isFavourite: true, searchTerm: searchTerm),
            "category" => await _menuAppService.GetItemsAsync(categoryId: categoryId, searchTerm: searchTerm),
            _ => new List<MenuAppItemViewModel>(),
        };

        return PartialView("_ItemCardPartial", items);
    }



    [HttpPost]
    public async Task<IActionResult> ToggleIsFavourite(int id)
    {
        bool isFavourite = await _menuAppService.ToggleIsFavourite(id);
        return Json(isFavourite);
    }

    [HttpGet]
    public async Task<IActionResult> GetModifierInItemCard(int id)
    {
        MenuAppModifierDetailViewModel? modifier = await _menuAppService.GetModifierInItemCardAsync(id);
        return PartialView("_ModifierItemPartial", modifier);
    }

    [HttpGet]
    public async Task<IActionResult> GetTableDetailsByOrderId(int orderId)
    {
        try
        {
            MenuAppTableSectionViewModel? result = await _menuAppService.GetTableDetailsByOrderIdAsync(orderId);
            return Json(new
            {
                floorName = result.SectionName,
                assignedTables = string.Join(", ", result.TableName)
            });
        }
        catch (Exception ex)
        {
            return Json(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddOrderItemPartial(int ItemId, List<int> ModifierIds)
    {
        try
        {
            MenuAppAddOrderItemViewModel? model = await _menuAppService.AddItemInOrder(ItemId, ModifierIds);
            return PartialView("_MenuAppItemPartial", model);
        }
        catch (Exception ex)
        {
            return Json(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderDetails(int orderId)
    {
        MenuAppOrderDetailsViewModel? orderdetails = await _menuAppService.GetOrderDetailsAsync(orderId);
        return PartialView("_OrderDetailsPartial", orderdetails);
    }

    [HttpPost]
    public async Task<IActionResult> SaveOrder([FromBody] MenuAppOrderDetailsViewModel model)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        int userId = int.Parse(userIdClaim ?? "0");

        bool success = await _menuAppService.SaveOrder(model, userId);
        if (success)
            return Ok(new { success = true });
        else
            return StatusCode(500, "Order save failed.");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteOrderItem(int itemId)
    {
        try
        {
            var (success, message) = await _menuAppService.DeleteOrderItemAsync(itemId);

            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "An error occurred: " + ex.Message });
        }
    }

    [HttpGet]

    public async Task<IActionResult> GetCustomerDetails(int orderId)
    {
        try
        {
            MenuAppCustomerViewModel? customer = await _menuAppService.GetCustomerDetailsByOrderId(orderId);
            return Json(customer);
        }
        catch (Exception ex)
        {
            throw new Exception("Error in Getting Details", ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCustomerDetails(MenuAppCustomerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data." });
        }

        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        int userId = int.Parse(userIdClaim ?? "0");

        bool result = await _menuAppService.UpdateCustomerDetailsAsync(model, userId);

        if (result)
        {
            return Json(new { success = true, message = "customer Updated successfully." });
        }
        else
        {
            return Json(new { success = false, message = "Failed to Update Customer" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderCommentById(int orderId)
    {
        try
        {
            MenuAppOrderViewModel? order = await _menuAppService.GetOrderCommentById(orderId);
            return Json(order);
        }
        catch (Exception ex)
        {
            throw new Exception("Error In Getting OrderComment", ex);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetItemInstructionById(int orderItemId, int orderId)
    {
        try
        {
            MenuAppItemInstructionViewModel? orderItem = await _menuAppService.GetItemInstructionById(orderItemId, orderId);
            return Json(orderItem);
        }
        catch (Exception ex)
        {
            throw new Exception("Error In Getting ItemInstruction", ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateInstruction(MenuAppItemInstructionViewModel model)
    {
        try
        {
            bool result = await _menuAppService.UpdateInstruction(model);
            if (result)
            {
                return Json(new { success = true, message = "Item Instruction Added successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to Add Instruction" });
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error in Adding Instruction", ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateComment(MenuAppOrderViewModel model)
    {
        try
        {
            bool result = await _menuAppService.UpdateOrderComment(model);
            if (result)
            {
                return Json(new { success = true, message = "Order Comment Added successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to Add Comment" });
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error in Adding Comment", ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CompleteOrder(int orderId)
    {
        try
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int userId = int.Parse(userIdClaim ?? "0");

            var (success, message) = await _menuAppService.CompleteOrderAsync(orderId, userId);
            return Json(new { success, message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while completing the order." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderStatus(int orderId)
    {
        MenuAppOrderViewModel? result = await _menuAppService.GetOrderStatusAsync(orderId);
        return Json(result);
    }


    [HttpPost]
    public async Task<IActionResult> SaveReview([FromBody] MenuAppCustomerFeedbackViewModel model)
    {
        try
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int userId = int.Parse(userIdClaim ?? "0");

            bool isSaved = await _menuAppService.AddFeedbackAsync(model, userId);
            if (isSaved)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }
        catch (Exception ex)
        {
            throw new Exception("error in saving review", ex);
        }
    }

    public async Task<IActionResult> GenerateInvoicePdf(int id)
    {
        try
        {
            byte[] pdfBytes = await _menuAppService.GenerateInvoicePdfAsync(id);
            return File(pdfBytes, "application/pdf", $"Invoice_{id}.pdf");
        }
        catch (Exception ex)
        {
            return Json(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        try
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int userId = int.Parse(userIdClaim ?? "0");

            var (Success, Message) = await _menuAppService.CancelOrderAsync(orderId,userId);
            return Json(new { success = Success, message = Message });
        }
        catch
        {
            return StatusCode(500, new { success = false, message = "An unexpected error occurred." });
        }
    }

}

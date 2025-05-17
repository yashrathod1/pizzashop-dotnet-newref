using pizzashop_repository.ViewModels;

namespace pizzashop_service.Interface;

public interface IOredersService
{
    Task<PagedResult<OrdersTableViewModel>> GetOrdersAsync(OrderPaginationViewModel model);

    byte[] GenerateExcel(string status, string date, string searchText);

    Task<OrderDetailsViewModel?> GetOrderDetailsAsync(int orderId);

    Task<byte[]> GenerateInvoicePdfAsync(int orderId);

    Task<byte[]> GenerateOrderDetailsPdfAsync(int orderId);



}
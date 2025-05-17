using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_repository.Interface;

public interface IOrdersRepository
{
    Task<IQueryable<Order>> GetAllOrdersAsync();
    
    List<OrdersTableViewModel> GetOrders(string status, string date, string orderId);

    Task<Order?> GetOrderWithDetailsAsync(int orderId);
}

using Microsoft.EntityFrameworkCore;
using pizzashop_repository.Database;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_repository.Implementation;

public class OrdersRepository : IOrdersRepository
{
    private readonly PizzaShopDbContext _context;

    public OrdersRepository(PizzaShopDbContext context)
    {
        _context = context;
    }

    public async Task<IQueryable<Order>> GetAllOrdersAsync()
    {
        try
        {
            var query = _context.Orders
                .Include(o => o.Feedbacks)
                .Include(o => o.Customer)
                .Include(o => o.Payments)
                .AsQueryable();

            return await Task.FromResult(query);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get orders from database", ex);
        }
    }


    public List<OrdersTableViewModel> GetOrders(string status, string dateRange, string searchTerm)
    {
        IQueryable<OrdersTableViewModel>? query = _context.Orders
                            .Include(o => o.Customer)
                            .Include(o => o.Payment)
                            .Include(o => o.Feedbacks)
                            .Select(o => new OrdersTableViewModel
                            {
                                Id = o.Id,
                                OrderDate = o.Createdat,
                                CustomerName = o.Customer.Name,
                                Status = o.Status,
                                PaymentMethod = o.Payment != null ? o.Payment.PaymentMethod : "Pending",
                                Rating = o.Feedbacks.Select(f => f.Avgrating).FirstOrDefault(),
                                TotalAmount = o.Payment != null ? o.Payment.Amount : 0
                            });

        if (!string.IsNullOrEmpty(status) && status != "All")
        {
            query = query.Where(o => o.Status == status);
        }

        if (!string.IsNullOrEmpty(dateRange) && dateRange != "All time")
        {
            DateTime now = DateTime.Now;
            DateTime startDate = now;

            switch (dateRange)
            {
                case "Today":
                    startDate = now.AddDays(-1);
                    break;
                case "Last 7 days":
                    startDate = now.AddDays(-7);
                    break;
                case "Last 30 days":
                    startDate = now.AddDays(-30);
                    break;
                case "Current Month":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    break;
            }

            query = query.Where(o => o.OrderDate >= startDate);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(o => o.Id.ToString().Contains(searchTerm.ToString()) || o.CustomerName.ToLower().Contains(searchTerm.ToLower()));
        }

        return query.ToList();
    }

    public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Payment)
            .Include(o => o.OrderItemsMappings)
                .ThenInclude(o => o.OrderItemModifiers)
            .Include(o => o.OrderTaxesMappings)
                .ThenInclude(o => o.Tax)
            .Include(o => o.OrdersTableMappings)
                .ThenInclude(o => o.Table)
                .ThenInclude(o => o.Section)
            .Include(o => o.Invoices)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

}








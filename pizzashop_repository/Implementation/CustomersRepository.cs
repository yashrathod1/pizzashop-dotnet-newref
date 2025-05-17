using Microsoft.EntityFrameworkCore;
using pizzashop_repository.Database;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_repository.Implementation;

public class CustomersRepository : ICustomersRepository
{
    private readonly PizzaShopDbContext _context;

    public CustomersRepository(PizzaShopDbContext context)
    {
        _context = context;
    }

    public async Task<IQueryable<Customer>> GetAllCustomersAsync()
    {
        try
        {
            var query = _context.Customers
                .Include(c => c.Orders)
                .AsQueryable();

            return await Task.FromResult(query);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrieve customers from the database", ex);
        }
    }


    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return await _context.Customers.Include(c => c.Orders).ThenInclude(o => o.OrderItemsMappings)
                                        .Include(c => c.Orders).ThenInclude(o => o.Payment)
                                        .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<CustomerTableViewModel>> GetCustomerAsync(
    string searchTerm = "", string dateRange = "All time",
    DateTime? customStartDate = null, DateTime? customEndDate = null)
    {
        DateTime startDate = DateTime.MinValue;
        DateTime endDate = DateTime.MaxValue;

        switch (dateRange)
        {
            case "Today":
                startDate = DateTime.Now.Date.AddDays(-1);
                endDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
                break;
            case "Last 7 days":
                startDate = DateTime.Now.Date.AddDays(-7);
                endDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
                break;
            case "Last 30 days":
                startDate = DateTime.Now.Date.AddDays(-30);
                endDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
                break;
            case "Current Month":
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                endDate = startDate.AddMonths(1).AddTicks(-1);
                break;
            case "Custom Date":
                if (customStartDate.HasValue && customEndDate.HasValue)
                {
                    startDate = customStartDate.Value.Date;
                    endDate = customEndDate.Value.Date.AddDays(1).AddTicks(-1);
                }
                break;
        }

        var query = _context.Customers
                            .Include(c => c.Orders)
                                .ThenInclude(o => o.Payment)
                            .SelectMany(c => c.Orders
                                .Where(o => o.Createdat >= startDate && o.Createdat <= endDate),
                                (c, o) => new
                                {
                                    c.Id,
                                    c.Name,
                                    c.Email,
                                    c.PhoneNumber,
                                    OrderDate = o.Createdat.Date
                                });


        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(i => i.Name.ToLower().Contains(searchTerm.ToLower()));
        }

        List<CustomerTableViewModel>? groupedCustomers = await query
            .GroupBy(c => new { c.Id, c.Name, c.Email, c.PhoneNumber, c.OrderDate })
            .Select(g => new CustomerTableViewModel
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Email = g.Key.Email,
                PhoNo = g.Key.PhoneNumber,
                Date = g.Key.OrderDate,
                TotalOrder = g.Count(),

            })
            .ToListAsync();

        return groupedCustomers;
    }

}

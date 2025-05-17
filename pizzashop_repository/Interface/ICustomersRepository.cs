using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_repository.Interface;

public interface ICustomersRepository
{
    Task<IQueryable<Customer>> GetAllCustomersAsync();
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<List<CustomerTableViewModel>> GetCustomerAsync(string searchTerm = "", string dateRange = "All time", DateTime? customStartDate = null, DateTime? customEndDate = null);
}
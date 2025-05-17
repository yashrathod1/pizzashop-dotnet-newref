using pizzashop_repository.ViewModels;

namespace pizzashop_service.Interface;

public interface ICustomersService
{
    Task<PagedResult<CustomerTableViewModel>> GetCustomersAsync(CustomerPaginationViewModel model);

    Task<CustomerHistoryViewModel> GetCustomerHistoryAsync(int id);

    Task<byte[]> GenerateExcel(string searchTerm = "", string dateRange = "All time", DateTime? customStartDate = null, DateTime? customEndDate = null);
}

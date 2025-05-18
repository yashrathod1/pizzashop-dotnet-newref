using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_service.Interface;

public interface ITableService
{
    // Task<TablesOrderAppViewModel> GetSectionsAsync();

    Task<TablesOrderAppViewModel> GetSectionsWithTablesAsync();

    Task<List<OrderAppSectionViewModel>> GetAllSectionsAsync();

     Task<(bool IsSuccess, string Message)> AddWaitingTokenAsync(WaitingTokenViewModel waitingTokenVm, int UserId);

    Task<List<WaitingTokenViewModel>> GetWaitingTokens(int sectionId);

    Task<WaitingTokenViewModel> GetCustomerDetailsByToken(int tokenId);

    Task<CustomerViewModel?> GetCustomerByEmail(string email);

    Task<(bool IsSuccess, string Message)> AssignTablesAsync(AssignTableRequestViewModel model, int UserId);

    Task<int?> GetOrderIdByTableIdAsync(int tableId);


}

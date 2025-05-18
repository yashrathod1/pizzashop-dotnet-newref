using pizzashop_repository.ViewModels;

namespace pizzashop_service.Interface;

public interface IWaitingListService
{
    Task<WaitingListViewModel> GetSectionAsync();

    Task<List<OrderAppSectionViewModel>> GetAllSectionsAsync();

    Task<WaitingListViewModel> GetWaitingListAsync(int? sectionId);

    Task<(bool IsSuccess, string Message)> AddWaitingTokenInWaitingListAsync(WaitingTokenViewModel waitingTokenVm, int UserId);

    Task<CustomerViewModel?> GetCustomerByEmail(string email);

    Task<WaitingListItemViewModel?> GetTokenByIdAsync(int id);

    Task<(bool success, string message)> EditWaitingTokenAsync(WaitingTokenViewModel model, int UserId);

    Task<bool> SoftDeleteTokenAsync(int id);

    Task<List<SectionsViewModal>> GetSectionsWithAvailableTablesAsync();

    Task<AssignTableResultViewModel> AssignTablesToCustomerAsync(AssignTableInWaitingTokenViewModel model, int UserId);

    Task<List<TableViewModel>> GetAvailableTablesBySectionAsync(int sectionId);
}

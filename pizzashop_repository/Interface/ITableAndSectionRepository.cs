using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;


namespace pizzashop_repository.Interface;

public interface ITableAndSectionRepository
{
    Task<List<SectionsViewModal>> GetSectionsAsync();

    Task<IQueryable<Table>> GetTablesBySectionAsync(int sectionId);

    Task<Section> AddSectionAsync(Section section);

    Task<Section?> GetSectionByNameAsync(string name);

    Task<SectionsViewModal> GetSectionByIdAsync(int id);

    Task<bool> UpdateSectionAsync(SectionsViewModal section);

    Task<bool> SoftDeleteSectionAsync(int id);

    Task<bool> AddTableAsync(Table table);

    Task<TableViewModel> GetTableById(int id);

    Task<Table?> GetTableByNameAsync(string name);

    Task<bool> UpdateTableAsync(Table table);

    Task<Table?> GetTableByIdForEdit(int id);

    Task<bool> SoftDeleteTableAsync(int id);

    List<Table> GetTablesByIds(List<int> tableIds);

    void SoftDeleteTablesAsync(List<int> tableIds);
}

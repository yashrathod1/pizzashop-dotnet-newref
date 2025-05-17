using Microsoft.EntityFrameworkCore;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop_service.Implementation;

public class TableAndSectionService : ITableAndSectionService
{
    private readonly ITableAndSectionRepository _tableAndSectionRepository;

    public TableAndSectionService(ITableAndSectionRepository tableAndSectionRepository)
    {
        _tableAndSectionRepository = tableAndSectionRepository;
    }

    public async Task<List<SectionsViewModal>> GetSectionsAsync()
    {
        return await _tableAndSectionRepository.GetSectionsAsync();
    }

    public async Task<PagedResult<TableViewModel>> GetTableBySectionAsync(TablePaginationViewModel model)
    {
        try
        {
            var query = await _tableAndSectionRepository.GetTablesBySectionAsync(model.SectionId);

            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                query = query.Where(t => t.Name.ToLower().Contains(model.SearchTerm.ToLower()));
            }

            int totalCount = await query.CountAsync();

            var tableList = await query
                .OrderBy(t => t.Id)
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .Select(t => new TableViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Capacity = t.Capacity,
                    Status = t.Status,
                    SectionId = t.Sectionid,
                    Isdeleted = t.Isdeleted
                }).ToListAsync();

            return new PagedResult<TableViewModel>(tableList, model.PageNumber, model.PageSize, totalCount);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while fetching tables by section.", ex);
        }
    }


    public async Task<Section> AddSectionAsync(string name, string description)
    {

        Section? existingSection = await _tableAndSectionRepository.GetSectionByNameAsync(name.Trim());

        if (existingSection != null)
        {
            return null;
        }
        Section? section = new() { Name = name, Description = description };

        return await _tableAndSectionRepository.AddSectionAsync(section);
    }

    public async Task<SectionsViewModal?> GetSectionByNameAsync(string name)
    {
        Section? section = await _tableAndSectionRepository.GetSectionByNameAsync(name);
        return section != null ? new SectionsViewModal { Name = section.Name, Id = section.Id } : null;
    }

    public async Task<SectionsViewModal> GetSectionById(int id)
    {
        return await _tableAndSectionRepository.GetSectionByIdAsync(id);
    }

    public async Task<bool> UpdateSectionAsync(SectionsViewModal section)
    {
        return await _tableAndSectionRepository.UpdateSectionAsync(section);
    }

    public async Task<bool> SoftDeleteSectionAsync(int id)
    {
        return await _tableAndSectionRepository.SoftDeleteSectionAsync(id);
    }

    public async Task<bool> AddTableAsync(TableViewModel model)
    {
        Table? newTable = new()
        {
            Name = model.Name,
            Capacity = model.Capacity,
            Status = model.Status,
            Sectionid = model.SectionId,
            Isdeleted = model.Isdeleted

        };

        return await _tableAndSectionRepository.AddTableAsync(newTable);
    }

    public async Task<TableViewModel> GetTableById(int id)
    {
        return await _tableAndSectionRepository.GetTableById(id);
    }

    public async Task<TableViewModel?> GetTableByNameAsync(string name)
    {
        Table? table = await _tableAndSectionRepository.GetTableByNameAsync(name);
        return table != null ? new TableViewModel { Name = table.Name, Id = table.Id } : null;
    }

    public async Task<bool> UpdateTableAsync(TableViewModel model)
    {
        Table? table = await _tableAndSectionRepository.GetTableByIdForEdit(model.Id);
        if (table == null) return false;

        table.Name = model.Name;
        table.Sectionid = model.SectionId;
        table.Capacity = model.Capacity;
        table.Status = model.Status;

        return await _tableAndSectionRepository.UpdateTableAsync(table);
    }

    public async Task<bool> SoftDeleteTableAsync(int id)
    {
        return await _tableAndSectionRepository.SoftDeleteTableAsync(id);
    }

    public (bool Success, string? Message) SoftDeleteTables(List<int> tableIds)
    {
        var tables = _tableAndSectionRepository.GetTablesByIds(tableIds);

        var nonAvailableTables = tables
            .Where(t => !string.Equals(t.Status, "Available", StringComparison.OrdinalIgnoreCase))
            .Select(t => t.Name)
            .ToList();

        if (nonAvailableTables.Any())
        {
            string message = "Cannot delete tables that are not 'Available': " + string.Join(", ", nonAvailableTables);
            return (false, message);
        }

        _tableAndSectionRepository.SoftDeleteTablesAsync(tableIds);
        return (true, null);
    }

}
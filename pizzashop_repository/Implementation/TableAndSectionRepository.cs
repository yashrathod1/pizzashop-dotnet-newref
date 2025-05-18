using Microsoft.EntityFrameworkCore;
using pizzashop_repository.Database;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_repository.Implementation;

public class TableAndSectionRepository : ITableAndSectionRepository
{
    private readonly PizzaShopDbContext _context;

    public TableAndSectionRepository(PizzaShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<SectionsViewModal>> GetSectionsAsync()
    {
        return await _context.Sections.Where(c => !c.Isdeleted).Select(x => new SectionsViewModal
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description
        }).ToListAsync();
    }

    public async Task<IQueryable<Table>> GetTablesBySectionAsync(int sectionId)
    {
        try
        {
            var tables = _context.Tables
                .Where(t => t.Sectionid == sectionId && !t.Isdeleted)
                .AsQueryable();

            return await Task.FromResult(tables);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get tables from database", ex);
        }
    }


    public async Task<Section> AddSectionAsync(Section section)
    {
        _context.Sections.Add(section);
        await _context.SaveChangesAsync();
        return section;
    }

    public async Task<Section?> GetSectionByNameAsync(string name)
    {
        return await _context.Sections
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower().Trim());
    }


    public async Task<SectionsViewModal> GetSectionByIdAsync(int id)
    {
        Section? section = await _context.Sections
            .FirstOrDefaultAsync(s => s.Id == id);

        if (section == null) return null;

        return new SectionsViewModal
        {
            Id = section.Id,
            Name = section.Name,
            Description = section.Description,
        };
    }

    public async Task<bool> UpdateSectionAsync(SectionsViewModal section, int UserId)
    {
        Section? existingSection = await _context.Sections.FindAsync(section.Id);
        if (existingSection == null) return false;

        existingSection.Name = section.Name;
        existingSection.Description = section.Description;
        existingSection.Updatedby = UserId;

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> SoftDeleteSectionAsync(int id)
    {
        Section? section = await _context.Sections.FindAsync(id);
        if (section == null) return false;

        var tablesInSection = await _context.Tables
            .Where(t => t.Sectionid == id && !t.Isdeleted)
            .ToListAsync();

        if (tablesInSection.Count > 1 && tablesInSection.Any(t => t.Status != "Available"))
        {
            return false;
        }

        section.Isdeleted = true;
        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<bool> AddTableAsync(Table table)
    {
        _context.Tables.Add(table);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<TableViewModel> GetTableById(int id)
    {
        TableViewModel? table = await _context.Tables
            .Where(x => x.Id == id)
            .Select(x => new TableViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Capacity = x.Capacity,
                Status = x.Status,
                SectionId = x.Sectionid,

            }).FirstOrDefaultAsync();

        return table;
    }

    public async Task<Table?> GetTableByNameAsync(string name)
    {
        return await _context.Tables.FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<Table?> GetTableByIdForEdit(int id)
    {
        return await _context.Tables.FindAsync(id);
    }

    public async Task<bool> UpdateTableAsync(Table table)
    {
        _context.Tables.Update(table);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> SoftDeleteTableAsync(int id)
    {
        Table? table = await _context.Tables.FindAsync(id);
        if (table == null) return false;

        if (table.Status != "Available")
        {
            return false;
        }

        table.Isdeleted = true;
        return await _context.SaveChangesAsync() > 0;
    }


    public List<Table> GetTablesByIds(List<int> tableIds)
    {
        return _context.Tables.Where(t => tableIds.Contains(t.Id)).ToList();
    }

    public void SoftDeleteTablesAsync(List<int> tableIds)
    {
        var tables = _context.Tables.Where(x => tableIds.Contains(x.Id)).ToList();
        foreach (var table in tables)
        {
            table.Isdeleted = true;
        }
        _context.SaveChanges();
    }


}

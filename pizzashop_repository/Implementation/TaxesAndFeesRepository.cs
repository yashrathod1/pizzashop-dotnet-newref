using Microsoft.EntityFrameworkCore;
using pizzashop_repository.Database;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_repository.Implementation;

public class TaxesAndFeesRepository : ITaxesAndFeesRepository
{
    private readonly PizzaShopDbContext _context;

    public TaxesAndFeesRepository(PizzaShopDbContext context)
    {
        _context = context;
    }

    public async Task<IQueryable<Taxesandfee>> GetAllTaxesAndFeesAsync()
    {
        try
        {
            var query = _context.Taxesandfees
                .Where(i => !i.Isdeleted)
                .AsQueryable();

            return await Task.FromResult(query);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrieve taxes and fees from database.", ex);
        }
    }


    public async Task<bool> AddTaxAsync(Taxesandfee taxesandfee)
    {
        _context.Taxesandfees.Add(taxesandfee);
        return await _context.SaveChangesAsync() > 0;
    }
    public async Task<TaxsAndFeesViewModel> GetTaxByIdAsync(int id)
    {
        TaxsAndFeesViewModel? tax = await _context.Taxesandfees
            .Where(x => x.Id == id)
            .Select(x => new TaxsAndFeesViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                Value = x.Value,
                IsEnabled = x.Isenabled,
                IsDefault = x.Isdefault

            }).FirstOrDefaultAsync();

        return tax;
    }

    public async Task<bool> SoftDeleteTaxAsync(int id)
    {
        Taxesandfee? taxesandfee = await _context.Taxesandfees.FindAsync(id);
        if (taxesandfee == null) return false;

        taxesandfee.Isdeleted = true;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateTaxesAsync(Taxesandfee taxesandfee)
    {
        _context.Taxesandfees.Update(taxesandfee);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Taxesandfee?> GetTaxByIdForEdit(int id)
    {
        return await _context.Taxesandfees.FindAsync(id);
    }
}

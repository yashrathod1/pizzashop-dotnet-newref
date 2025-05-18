using Microsoft.EntityFrameworkCore;
using pizzashop_repository.Interface;
using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;
using pizzashop_service.Interface;

namespace pizzashop_service.Implementation;

public class TaxesAndFeesService : ITaxesAndFeesService
{

    private readonly ITaxesAndFeesRepository _taxesAndFeesRepository;

    public TaxesAndFeesService(ITaxesAndFeesRepository taxesAndFeesService)
    {
        _taxesAndFeesRepository = taxesAndFeesService;
    }
    public async Task<PagedResult<TaxsAndFeesViewModel>> GetTaxesAndFeesAsync(PaginationViewModel model)
    {
        try
        {
            var query = await _taxesAndFeesRepository.GetAllTaxesAndFeesAsync();

            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                query = query.Where(i => i.Name.ToLower().Contains(model.SearchTerm.ToLower()));
            }

            int totalCount = await query.CountAsync();

            var result = await query
                .OrderBy(i => i.Id)
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .Select(t => new TaxsAndFeesViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Type = t.Type,
                    IsEnabled = t.Isenabled,
                    IsDefault = t.Isdefault,
                    Value = t.Value
                }).ToListAsync();

            return new PagedResult<TaxsAndFeesViewModel>(result, model.PageNumber, model.PageSize, totalCount);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while fetching taxes and fees.", ex);
        }
    }


    public async Task<bool> AddTaxAsync(TaxsAndFeesViewModel model, int UserId)
    {

        Taxesandfee? tax = new()

        {
            Name = model.Name,
            Type = model.Type,
            Value = model.Value,
            Isenabled = model.IsEnabled,
            Isdefault = model.IsDefault,
            Createdby = UserId
        };

        return await _taxesAndFeesRepository.AddTaxAsync(tax);
    }

    public async Task<TaxsAndFeesViewModel> GetTaxByIdAsync(int id)
    {
        return await _taxesAndFeesRepository.GetTaxByIdAsync(id);
    }

    public async Task<bool> SoftDeleteTaxAsync(int id)
    {
        return await _taxesAndFeesRepository.SoftDeleteTaxAsync(id);
    }

    public async Task<bool> UpdateTaxAsync(TaxsAndFeesViewModel model, int UserId)
    {
        Taxesandfee? tax = await _taxesAndFeesRepository.GetTaxByIdForEdit(model.Id);
        if (tax == null) return false;

        tax.Name = model.Name;
        tax.Type = model.Type;
        tax.Value = model.Value;
        tax.Isenabled = model.IsEnabled;
        tax.Isdefault = model.IsDefault;
        tax.Updatedat = DateTime.Now;
        tax.Updatedby = UserId;

        return await _taxesAndFeesRepository.UpdateTaxesAsync(tax);
    }
}

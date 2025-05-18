using pizzashop_repository.Models;
using pizzashop_repository.ViewModels;

namespace pizzashop_repository.Interface;

public interface IMenuRepository
{
    Task<List<Category>> GetCategoriesAsync();

    Task<List<MenuItem>> GetItemsAsync();
    Task<Category> AddCategoryAsync(Category category);

    Task<Category?> GetCategoryByNameAsync(string name);

    Task<Category?> GetCategoryByIdAsync(int id);

    Task<bool> IsCategoryUsedInActiveOrdersAsync(int categoryId);

    Task<bool> UpdateCategoryAsync(Category category);

    Task<bool> SoftDeleteCategoryAsync(Category category);

    Task<bool> AddItemAsync(MenuItem item);

    Task<bool> AddItemModifiersAsync(List<MappingMenuItemWithModifier> mappings);

    Task<bool> UpdateItemAsync(MenuItem item);

    Task<bool> DeleteItemModifiersAsync(List<MappingMenuItemWithModifier> modifiersToRemove);

    MenuItem? GetItemsById(int id);

    Task<bool> SoftDeleteItemAsync(int id);

    void SoftDeleteItemsAsync(List<int> itemIds);

    List<MappingMenuItemWithModifier> GetItemModifiersByItemId(int itemId);

    Task<List<Modifiergroup>> GetModifierGroupsAsync();

    Task<IQueryable<Modifier>> GetModifiersByModifierGroupQueryAsync(int modifierGroupId);

    Task<List<Modifier>> GetModifiersAsync();

    Task<List<int>> GetModifierGroupIdsForModifierAsync(int modifierId);

    Task<bool> AddModifierGroup(Modifiergroup modifierGroup, List<int> modifierIds);

    Task<bool> ExistsModifierGroupByNameAsync(string name);

    Task<bool> SoftDeleteModifierGroupAsync(int id);

    Task<Modifiergroup?> GetModifierGroupByIdAsync(int id);

    Task<bool> UpdateModifierGroup(ModifierGroupViewModel model, int UserId);

    Task<bool> AddModifierAsync(Modifier modifier, List<int> modifierGroupIds);

    Task<Modifier?> GetModifierById(int id);

    Task<bool> UpdateModifierAsync(Modifier modifie);

    Task UpdateModifierGroupsAsync(int modifierId, List<int> modifierGroupIds);

    Task<Modifier?> GetModifierByIdAsync(int id);

    Task<bool> SoftDeleteModifierAsync(int id);

    Task<bool> SoftDeleteModifierFromGroupAsync(int modifierId, int groupId);

    Task<bool> SoftDeleteModifiersAsync(List<int> modifierIds, int currentGroupId);

    Task<IQueryable<MenuItem>> GetItemsByCategoryQueryAsync(int categoryId);

    Task<IQueryable<Modifier>> GetAllModifiersAsync();

    Task<ModifierGroupViewModel?> GetModifiersByGroupIdAsync(int modifierGroupId);

    Task<List<int>> GetModifierGroupIdsByModifierId(int modifierId);

    Task<ItemModifierGroupViewModel> GetModifierGroupByIdForItem(int groupId);

    Task<List<MappingMenuItemWithModifier>> GetItemWithModifiersByItemIdAsync(int id);

    Task<bool> UpdateItemModifiersAsync(List<MappingMenuItemWithModifier> modifiersToUpdate);
}

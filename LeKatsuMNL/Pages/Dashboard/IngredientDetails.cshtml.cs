using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using LeKatsuMNL.Helpers;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class IngredientDetailsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public IngredientDetailsModel(LeKatsuDb context)
        {
            _context = context;
        }

        [BindProperty]
        public CommissaryInventory Ingredient { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public List<Category> Categories { get; set; }
        public List<SubCategory> SubCategories { get; set; }
        public List<CommissaryInventory> AvailableItems { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return RedirectToPage("/Dashboard/Items");
            }

            Ingredient = await _context.CommissaryInventories
                .Include(i => i.Category)
                .Include(i => i.SubCategory)
                .Include(i => i.IngredientRecipes)
                    .ThenInclude(r => r.Material)
                .FirstOrDefaultAsync(m => m.ComId == id);

            if (Ingredient == null)
            {
                return NotFound();
            }

            Categories = await _context.Categories.Where(c => !c.IsArchived).ToListAsync();
            SubCategories = await _context.SubCategories.ToListAsync();
            
            // Available items: exclude self to prevent circular reference, and only those that are not repacks themselves (optional, but keep it simple for now as per user request to be like SKU)
            // User said "adding an ingredient to an ingredient", didn't specify depth, but SKU allows recursion.
            AvailableItems = await _context.CommissaryInventories
                .Where(i => i.ComId != id && i.SkuId == null && !i.IsArchived)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!PermissionHelper.HasPermission(User, "Items", 'U')) return Forbid();

            if (Ingredient == null || Ingredient.ComId == 0)
            {
                return Page();
            }

            var itemToUpdate = await _context.CommissaryInventories
                .Include(i => i.IngredientRecipes)
                .FirstOrDefaultAsync(i => i.ComId == Ingredient.ComId);

            if (itemToUpdate == null)
            {
                return NotFound();
            }

            // Update basic info (Yield logic handled in Items page usually, but we keep it sync here if needed)
            itemToUpdate.ItemName = Ingredient.ItemName;
            itemToUpdate.SubCategoryId = Ingredient.SubCategoryId;
            itemToUpdate.Uom = Ingredient.Uom;
            itemToUpdate.CostPrice = Ingredient.CostPrice;
            itemToUpdate.SellingPrice = Ingredient.SellingPrice;
            itemToUpdate.Stock = Ingredient.Stock;

            // Update recipes: Clear and re-add
            _context.IngredientRecipes.RemoveRange(itemToUpdate.IngredientRecipes);
            
            if (Ingredient.IngredientRecipes != null)
            {
                foreach (var recipe in Ingredient.IngredientRecipes)
                {
                    if (recipe.MaterialId <= 0) continue;
                    
                    var newRecipe = new IngredientRecipe
                    {
                        ParentId = itemToUpdate.ComId,
                        MaterialId = recipe.MaterialId,
                        QuantityNeeded = recipe.QuantityNeeded,
                        Uom = recipe.Uom
                    };

                    itemToUpdate.IngredientRecipes.Add(newRecipe);
                }
            }

            await _context.SaveChangesAsync();
            
            // Recalculate Unit Cost
            itemToUpdate.CostPrice = Math.Round(await CalculateTotalUnitCost(itemToUpdate.ComId), 2);

            await _context.SaveChangesAsync();

            StatusMessage = "Successfully recorded. The ingredient details have been updated.";
            return RedirectToPage(new { id = Ingredient.ComId });
        }

        private async Task<decimal> CalculateTotalUnitCost(int ingredientId, HashSet<int> visitedIds = null)
        {
            if (visitedIds == null) visitedIds = new HashSet<int>();
            if (visitedIds.Contains(ingredientId)) return 0; // Circular dependency check
            
            visitedIds.Add(ingredientId);

            var item = await _context.CommissaryInventories
                .Include(i => i.IngredientRecipes)
                    .ThenInclude(r => r.Material)
                .FirstOrDefaultAsync(i => i.ComId == ingredientId);

            if (item == null) return 0;

            // If it's not a repack, just return its cost (or 0 if calculating from scratch, but here we need its base cost if it's a leaf node)
            if (!item.IsRepack || item.IngredientRecipes == null || !item.IngredientRecipes.Any())
            {
                return item.CostPrice;
            }

            decimal totalCost = 0;
            foreach (var recipe in item.IngredientRecipes)
            {
                if (recipe.Material != null)
                {
                    decimal materialCost = recipe.Material.IsRepack 
                        ? await CalculateTotalUnitCost(recipe.MaterialId, new HashSet<int>(visitedIds))
                        : recipe.Material.CostPrice;

                    decimal convertedQty = Helpers.UomConverter.Convert(
                        recipe.QuantityNeeded, recipe.Uom, recipe.Material.Uom);
                    
                    totalCost += convertedQty * materialCost;
                }
            }

            return totalCost;
        }
    }
}

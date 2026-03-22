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

            // Update basic info
            itemToUpdate.ItemName = Ingredient.ItemName;
            itemToUpdate.CategoryId = Ingredient.CategoryId;
            itemToUpdate.SubCategoryId = Ingredient.SubCategoryId;
            itemToUpdate.Uom = Ingredient.Uom;
            itemToUpdate.SellingPrice = Ingredient.SellingPrice;
            itemToUpdate.Stock = Ingredient.Stock;

            // If it's a Recipe, CostPrice is calculated, not directly from form
            if (itemToUpdate.ItemType != "Recipe")
            {
                itemToUpdate.CostPrice = Ingredient.CostPrice;
            }

            // Update Yield from form if provided (new fields for Recipe)
            string yieldValue = Request.Form["YieldValue"];
            string yieldUnit = Request.Form["YieldUnit"];
            if (itemToUpdate.ItemType == "Recipe" && !string.IsNullOrEmpty(yieldValue))
            {
                itemToUpdate.Yield = $"{yieldValue}{yieldUnit}";
            }

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
            decimal totalMaterialsCost = await CalculateTotalMaterialsCost(itemToUpdate.ComId);
            decimal divisor = ParseYieldDivisor(itemToUpdate.Yield, itemToUpdate.Uom);
            
            itemToUpdate.CostPrice = Math.Round(totalMaterialsCost / (divisor > 0 ? divisor : 1), 4);

            await _context.SaveChangesAsync();

            StatusMessage = "Successfully recorded. The ingredient details have been updated.";
            return RedirectToPage(new { id = Ingredient.ComId });
        }

        public decimal ParseYieldDivisor(string yieldStr, string targetUom)
        {
            if (string.IsNullOrEmpty(yieldStr)) return 1;
            // Use UomConverter to normalize the yield string (e.g., "1kg") to the target UOM (e.g., "Grams")
            return Helpers.UomConverter.Convert(1, yieldStr, targetUom);
        }

        public decimal ExtractYieldValue(string yieldStr)
        {
            if (string.IsNullOrEmpty(yieldStr)) return 1;
            var match = System.Text.RegularExpressions.Regex.Match(yieldStr, @"(\d+(\.\d+)?)(?=[^\d/]*$)", System.Text.RegularExpressions.RegexOptions.RightToLeft);
            if (match.Success && decimal.TryParse(match.Value, out decimal val))
            {
                return val;
            }
            return 1;
        }

        public string ExtractYieldUnit(string yieldStr)
        {
            if (string.IsNullOrEmpty(yieldStr)) return "";
            var match = System.Text.RegularExpressions.Regex.Match(yieldStr, @"(\d+(\.\d+)?)(?=[^\d/]*$)", System.Text.RegularExpressions.RegexOptions.RightToLeft);
            if (match.Success)
            {
                return yieldStr.Substring(match.Index + match.Length).Trim();
            }
            return yieldStr;
        }

        private async Task<decimal> CalculateTotalMaterialsCost(int ingredientId, HashSet<int> visitedIds = null)
        {
            if (visitedIds == null) visitedIds = new HashSet<int>();
            if (visitedIds.Contains(ingredientId)) return 0; // Circular dependency check
            
            visitedIds.Add(ingredientId);

            var item = await _context.CommissaryInventories
                .Include(i => i.IngredientRecipes)
                    .ThenInclude(r => r.Material)
                .FirstOrDefaultAsync(i => i.ComId == ingredientId);

            if (item == null) return 0;

            // If it's a leaf node (not a recipe/repack), use its base cost
            if (item.IngredientRecipes == null || !item.IngredientRecipes.Any())
            {
                return item.CostPrice;
            }

            decimal totalCost = 0;
            foreach (var recipe in item.IngredientRecipes)
            {
                if (recipe.Material != null)
                {
                    decimal materialCost = (recipe.Material.IngredientRecipes != null && recipe.Material.IngredientRecipes.Any())
                        ? await CalculateTotalMaterialsCost(recipe.MaterialId, new HashSet<int>(visitedIds))
                        : recipe.Material.CostPrice;

                    // If the material cost we just got is a TOTAL cost (from sub-recipe), we might need to divide it by its yield too?
                    // Actually, CalculateTotalMaterialsCost should probably return the PRICE PER UNIT of the material.
                    // Wait, if it's a leaf node, item.CostPrice IS the price per unit.
                    // If it's a sub-recipe, we need its PRICE PER UNIT.
                    
                    if (recipe.Material.IngredientRecipes != null && recipe.Material.IngredientRecipes.Any())
                    {
                        decimal subYield = ParseYieldDivisor(recipe.Material.Yield, recipe.Material.Uom);
                        materialCost = materialCost / (subYield > 0 ? subYield : 1);
                    }

                    decimal convertedQty = Helpers.UomConverter.Convert(
                        recipe.QuantityNeeded, recipe.Uom, recipe.Material.Uom);
                    
                    totalCost += convertedQty * materialCost;
                }
            }

            return totalCost;
        }
    }
}

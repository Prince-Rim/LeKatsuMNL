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
    public class SkuDetailsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public SkuDetailsModel(LeKatsuDb context)
        {
            _context = context;
        }

        [BindProperty]
        public SkuHeader SkuHeader { get; set; }

        public List<Category> Categories { get; set; }
        public List<SubCategory> SubCategories { get; set; }
        public List<CommissaryInventory> AvailableItems { get; set; }
        public List<SkuHeader> AvailableSkus { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                // For demo purposes if id is not provided, load the first one or create dummy
                SkuHeader = await _context.SkuHeaders
                    .Include(s => s.Category)
                    .Include(s => s.SubCategory)
                    .Include(s => s.SkuRecipes)
                        .ThenInclude(r => r.CommissaryInventory)
                    .Include(s => s.SkuRecipes)
                        .ThenInclude(r => r.TargetSku)
                    .FirstOrDefaultAsync();

                if (SkuHeader == null)
                {
                    SkuHeader = new SkuHeader { ItemName = "Chicken Karaage" };
                }
            }
            else
            {
                SkuHeader = await _context.SkuHeaders
                    .Include(s => s.Category)
                    .Include(s => s.SubCategory)
                    .Include(s => s.SkuRecipes)
                        .ThenInclude(r => r.CommissaryInventory)
                    .Include(s => s.SkuRecipes)
                        .ThenInclude(r => r.TargetSku)
                    .FirstOrDefaultAsync(m => m.SkuId == id);

                if (SkuHeader == null)
                {
                    return NotFound();
                }
            }

            Categories = await _context.Categories.ToListAsync();
            SubCategories = await _context.SubCategories.ToListAsync();
            AvailableItems = await _context.CommissaryInventories.ToListAsync();
            AvailableSkus = await _context.SkuHeaders
                .Where(s => s.SkuId != id) // Prevent direct self-reference
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!PermissionHelper.HasPermission(User, "SKU", 'U')) return Forbid();

            if (SkuHeader == null || SkuHeader.SkuId == 0)
            {
                return Page();
            }

            var skuToUpdate = await _context.SkuHeaders
                .Include(s => s.SkuRecipes)
                .FirstOrDefaultAsync(s => s.SkuId == SkuHeader.SkuId);

            if (skuToUpdate == null)
            {
                return NotFound();
            }

            // Update basic info
            skuToUpdate.ItemName = SkuHeader.ItemName;
            skuToUpdate.SubCategoryId = SkuHeader.SubCategoryId;
            skuToUpdate.PackagingType = SkuHeader.PackagingType;
            skuToUpdate.PackagingUnit = SkuHeader.PackagingUnit;
            skuToUpdate.PackSize = SkuHeader.PackSize;
            skuToUpdate.Uom = SkuHeader.Uom;
            skuToUpdate.Supplier = SkuHeader.Supplier;
            skuToUpdate.IsSellingPriceEnabled = true;
            skuToUpdate.IsReorderLevelEnabled = true;
            skuToUpdate.SellingPrice = SkuHeader.SellingPrice;
            skuToUpdate.UnitCost = SkuHeader.UnitCost;

            // Update recipes: Clear and re-add for simplicity
            _context.SkuRecipes.RemoveRange(skuToUpdate.SkuRecipes);
            
            if (SkuHeader.SkuRecipes != null)
            {
                foreach (var recipe in SkuHeader.SkuRecipes)
                {
                    // Defensive check: Ignore if both are null or invalid
                    if (recipe.ComId <= 0 && recipe.TargetSkuId <= 0) continue;
                    
                    var newRecipe = new SkuRecipe
                    {
                        SkuId = skuToUpdate.SkuId,
                        QuantityNeeded = recipe.QuantityNeeded,
                        Uom = recipe.Uom
                    };

                    if (recipe.ComId > 0)
                    {
                        var inventoryItem = await _context.CommissaryInventories.FindAsync(recipe.ComId);
                        if (inventoryItem != null)
                        {
                            newRecipe.ComId = recipe.ComId;
                            newRecipe.CommissaryInventory = inventoryItem;
                        }
                    }
                    else if (recipe.TargetSkuId > 0)
                    {
                        var targetSku = await _context.SkuHeaders.FindAsync(recipe.TargetSkuId);
                        if (targetSku != null)
                        {
                            newRecipe.TargetSkuId = recipe.TargetSkuId;
                            newRecipe.TargetSku = targetSku;
                        }
                    }

                    skuToUpdate.SkuRecipes.Add(newRecipe);
                }
            }

            // Save changes first so that CalculateTotalUnitCost can see the new recipes in the database
            await _context.SaveChangesAsync();
            
            // Set the calculated unit cost
            skuToUpdate.UnitCost = Math.Round(await CalculateTotalUnitCost(skuToUpdate.SkuId), 2);

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = SkuHeader.SkuId });
        }
        private async Task<decimal> CalculateTotalUnitCost(int skuId, HashSet<int> visitedSkuIds = null)
        {
            if (visitedSkuIds == null) visitedSkuIds = new HashSet<int>();
            if (visitedSkuIds.Contains(skuId)) return 0; // Circular dependency check
            
            visitedSkuIds.Add(skuId);

            var sku = await _context.SkuHeaders
                .Include(s => s.SkuRecipes)
                    .ThenInclude(r => r.CommissaryInventory)
                .Include(s => s.SkuRecipes)
                    .ThenInclude(r => r.TargetSku)
                .FirstOrDefaultAsync(s => s.SkuId == skuId);

            if (sku == null) return 0;

            decimal totalCost = 0;
            foreach (var recipe in sku.SkuRecipes)
            {
                if (recipe.ComId.HasValue && recipe.CommissaryInventory != null)
                {
                    decimal convertedQty = Helpers.UomConverter.Convert(
                        recipe.QuantityNeeded, recipe.Uom, recipe.CommissaryInventory.Uom);
                    totalCost += convertedQty * recipe.CommissaryInventory.CostPrice;
                }
                else if (recipe.TargetSkuId.HasValue)
                {
                    decimal targetSkuUnitCost = await CalculateTotalUnitCost(recipe.TargetSkuId.Value, new HashSet<int>(visitedSkuIds));
                    // Assuming TargetSku unit of measurement and QuantityNeeded are compatible for now
                    // Usually SKUs are measured in Units (PCS, Packs, etc.)
                    totalCost += recipe.QuantityNeeded * targetSkuUnitCost;
                }
            }

            return totalCost;
        }
    }
}

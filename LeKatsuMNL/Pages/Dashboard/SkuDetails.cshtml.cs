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

        public List<CommissaryInventory> AvailableItems { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                // For demo purposes if id is not provided, load the first one or create dummy
                SkuHeader = await _context.SkuHeaders
                    .Include(s => s.Category)
                    .Include(s => s.SkuRecipes)
                        .ThenInclude(r => r.CommissaryInventory)
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
                    .Include(s => s.SkuRecipes)
                        .ThenInclude(r => r.CommissaryInventory)
                    .FirstOrDefaultAsync(m => m.SkuId == id);

                if (SkuHeader == null)
                {
                    return NotFound();
                }
            }

            AvailableItems = await _context.CommissaryInventories.ToListAsync();

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
            skuToUpdate.SubCategory = SkuHeader.SubCategory;
            skuToUpdate.SubClass = SkuHeader.SubClass;
            skuToUpdate.PackagingType = SkuHeader.PackagingType;
            skuToUpdate.PackagingUnit = SkuHeader.PackagingUnit;
            skuToUpdate.PackSize = SkuHeader.PackSize;
            skuToUpdate.Uom = SkuHeader.Uom;
            skuToUpdate.Supplier = SkuHeader.Supplier;
            skuToUpdate.IsSellingPriceEnabled = SkuHeader.IsSellingPriceEnabled;
            skuToUpdate.IsReorderLevelEnabled = SkuHeader.IsReorderLevelEnabled;
            skuToUpdate.SellingPrice = SkuHeader.SellingPrice;
            skuToUpdate.UnitCost = SkuHeader.UnitCost;

            // Update recipes: Clear and re-add for simplicity
            _context.SkuRecipes.RemoveRange(skuToUpdate.SkuRecipes);
            
            if (SkuHeader.SkuRecipes != null)
            {
                System.Console.WriteLine("---- DEBUGGING SKU POST ----");
                foreach (var recipe in SkuHeader.SkuRecipes)
                {
                    System.Console.WriteLine($"Recipe attached: ComId={recipe.ComId}, Quantity={recipe.QuantityNeeded}");
                    
                    // Defensive check: Ignore invalid IDs
                    if (recipe.ComId <= 0) continue;
                    
                    // Defensive check: Ensure it actually exists in CommissaryInventories
                    var inventoryItem = await _context.CommissaryInventories.FindAsync(recipe.ComId);
                    if (inventoryItem == null) continue;

                    skuToUpdate.SkuRecipes.Add(new SkuRecipe
                    {
                        SkuId = skuToUpdate.SkuId,
                        ComId = recipe.ComId,
                        CommissaryInventory = inventoryItem, // Force EF to populate shadow FK
                        QuantityNeeded = recipe.QuantityNeeded,
                        Uom = recipe.Uom
                    });
                }
                System.Console.WriteLine("----------------------------");
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = SkuHeader.SkuId });
        }
    }
}

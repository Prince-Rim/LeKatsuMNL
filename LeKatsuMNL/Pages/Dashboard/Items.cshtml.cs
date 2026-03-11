using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class ItemsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public ItemsModel(LeKatsuDb context)
        {
            _context = context;
        }

        public List<CommissaryInventory> Items { get; set; } = new List<CommissaryInventory>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<VendorInfo> Vendors { get; set; } = new List<VendorInfo>();

        [BindProperty]
        public InputModel NewItem { get; set; } = new InputModel();

        public class InputModel
        {
            public string ItemName { get; set; }
            public int CategoryId { get; set; }
            public string PackagingType { get; set; }
            public string PackagingUnit { get; set; }
            public string PackSize { get; set; }
            public string UOM { get; set; }
            public int VendorId { get; set; }
            public decimal? UnitPrice { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Items = await _context.CommissaryInventories
                .Include(i => i.Category)
                .Include(i => i.Vendor)
                .ToListAsync();

            Categories = await _context.Categories.ToListAsync();
            Vendors = await _context.VendorInfos.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage();
            }

            // Yield logic -> Type / Size + Unit
            // Example from user image: Type + Unit + Size = Yield. Like "Pack/10g"
            string calculatedYield = string.Empty;
            if (!string.IsNullOrEmpty(NewItem.PackagingType) && 
                !string.IsNullOrEmpty(NewItem.PackSize) && 
                !string.IsNullOrEmpty(NewItem.PackagingUnit))
            {
                calculatedYield = $"{NewItem.PackagingType}/{NewItem.PackSize}{NewItem.PackagingUnit}";
            }

            var item = new CommissaryInventory
            {
                ItemName = NewItem.ItemName,
                CategoryId = NewItem.CategoryId,
                VendorId = NewItem.VendorId,
                Yield = calculatedYield,
                Uom = NewItem.UOM ?? "pack",
                Price = NewItem.UnitPrice ?? 0,
                Stock = 0 // Initial logic
            };

            _context.CommissaryInventories.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}

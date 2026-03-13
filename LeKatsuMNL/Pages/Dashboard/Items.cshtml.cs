using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using LeKatsuMNL.Helpers;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class ItemsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public ItemsModel(LeKatsuDb context)
        {
            _context = context;
        }

        public PaginatedList<CommissaryInventory> Items { get; set; } = default!;
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<VendorInfo> Vendors { get; set; } = new List<VendorInfo>();

        public InputModel NewItem { get; set; } = new InputModel();
        public UpdateModel UpdateItem { get; set; } = new UpdateModel();

        public class UpdateModel : InputModel
        {
            public int ComId { get; set; }
        }

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

        public async Task<IActionResult> OnGetAsync(int? pageIndex)
        {
            var query = _context.CommissaryInventories
                .Include(i => i.Category)
                .Include(i => i.Vendor)
                .OrderByDescending(i => i.ComId);

            Items = await PaginatedList<CommissaryInventory>.CreateAsync(query, pageIndex ?? 1, 10);
            
            Categories = await _context.Categories.ToListAsync();
            Vendors = await _context.VendorInfos.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(
            string ItemName,
            int CategoryId,
            int VendorId,
            string PackagingType,
            string PackagingUnit,
            string PackSize,
            string UOM,
            decimal? UnitPrice)
        {
            if (!PermissionHelper.HasPermission(User, "Items", 'C')) return Forbid();

            if (string.IsNullOrWhiteSpace(ItemName))
            {
                return RedirectToPage();
            }

            // Yield logic -> Type / Size + Unit
            string calculatedYield = string.Empty;
            if (!string.IsNullOrEmpty(PackagingType) && 
                !string.IsNullOrEmpty(PackSize) && 
                !string.IsNullOrEmpty(PackagingUnit))
            {
                calculatedYield = $"{PackagingType}/{PackSize}{PackagingUnit}";
            }

            var item = new CommissaryInventory
            {
                ItemName = ItemName,
                CategoryId = CategoryId,
                VendorId = VendorId,
                Yield = calculatedYield,
                Uom = UOM ?? "pack",
                Price = UnitPrice ?? 0,
                Stock = 0
            };

            _context.CommissaryInventories.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(
            int ComId,
            string ItemName,
            int CategoryId,
            int VendorId,
            string PackagingType,
            string PackagingUnit,
            string PackSize,
            string UOM,
            decimal? UnitPrice)
        {
            if (!PermissionHelper.HasPermission(User, "Items", 'U')) return Forbid();

            var item = await _context.CommissaryInventories.FindAsync(ComId);
            if (item == null)
            {
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(ItemName))
            {
                return RedirectToPage();
            }

            // Yield logic -> Type / Size + Unit
            string calculatedYield = string.Empty;
            if (!string.IsNullOrEmpty(PackagingType) && 
                !string.IsNullOrEmpty(PackSize) && 
                !string.IsNullOrEmpty(PackagingUnit))
            {
                calculatedYield = $"{PackagingType}/{PackSize}{PackagingUnit}";
            }

            item.ItemName = ItemName;
            item.CategoryId = CategoryId;
            item.VendorId = VendorId;
            item.Yield = calculatedYield;
            item.Uom = UOM ?? "pack";
            item.Price = UnitPrice ?? 0;

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostRejectAsync(string RejectName, decimal RejectQty, string RejectUOM, string RejectReason)
        {
            if (!PermissionHelper.HasPermission(User, "Rejects", 'C')) return Forbid();

            if (string.IsNullOrEmpty(RejectName) || RejectQty <= 0)
            {
                return RedirectToPage();
            }

            var reject = new RejectItem
            {
                ItemName = RejectName,
                Quantity = RejectQty,
                Uom = RejectUOM,
                Reason = RejectReason,
                RejectedAt = System.DateTime.Now,
                RejectType = "Recipe"
            };

            _context.RejectItems.Add(reject);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Dashboard/Rejects", new { tab = "recipe" });
        }
    }
}

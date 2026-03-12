using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class SKUModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public SKUModel(LeKatsuDb context)
        {
            _context = context;
        }

        public IList<SkuHeader> SkuHeaders { get; set; } = new List<SkuHeader>();
        public IList<Category> Categories { get; set; } = new List<Category>();
        public IList<VendorInfo> Vendors { get; set; } = new List<VendorInfo>();

        public async Task OnGetAsync()
        {
            SkuHeaders = await _context.SkuHeaders
                .Include(s => s.Category)
                .ToListAsync();

            Categories = await _context.Categories.ToListAsync();
            Vendors = await _context.VendorInfos.ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync(
            string ProductName,
            int CategoryId,
            string SubClass,
            string PackagingType,
            string PackagingUnit,
            string PackSize,
            string UOM,
            string Supplier,
            string sellingPriceSku,
            decimal? SellingPrice,
            decimal? UnitCost)
        {
            if (string.IsNullOrWhiteSpace(ProductName))
            {
                ModelState.AddModelError("", "Product Name is required.");
                return await InitializeAndReturnPage();
            }

            var newSku = new SkuHeader
            {
                ItemName = ProductName.Trim(),
                CategoryId = CategoryId,
                SubClass = SubClass ?? "",
                PackagingType = PackagingType ?? "",
                PackagingUnit = PackagingUnit ?? "",
                PackSize = PackSize ?? "",
                Uom = UOM ?? "",
                Supplier = Supplier ?? "",
                IsSellingPriceEnabled = sellingPriceSku == "on" || sellingPriceSku == "true",
                SellingPrice = SellingPrice,
                UnitCost = UnitCost
            };

            // Force values to empty if checkbox is unchecked
            if (!newSku.IsSellingPriceEnabled)
            {
                newSku.SellingPrice = null;
                newSku.UnitCost = null;
            }

            _context.SkuHeaders.Add(newSku);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        private async Task<IActionResult> InitializeAndReturnPage()
        {
            await OnGetAsync();
            return Page();
        }
        public async Task<IActionResult> OnPostRejectAsync(string RejectName, decimal RejectQty, string RejectUOM, string RejectReason)
        {
            if (string.IsNullOrEmpty(RejectName) || RejectQty <= 0)
            {
                return await InitializeAndReturnPage();
            }

            var reject = new RejectItem
            {
                ItemName = RejectName,
                Quantity = RejectQty,
                Uom = RejectUOM,
                Reason = RejectReason,
                RejectedAt = System.DateTime.Now,
                RejectType = "SKU"
            };

            _context.RejectItems.Add(reject);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Dashboard/Rejects", new { tab = "sku" });
        }
    }
}

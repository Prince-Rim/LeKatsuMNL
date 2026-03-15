using System;
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
    public class SKUModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public SKUModel(LeKatsuDb context)
        {
            _context = context;
        }

        public PaginatedList<SkuHeader> SkuHeaders { get; set; } = default!;
        public IList<Category> Categories { get; set; } = new List<Category>();
        public IList<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public IList<VendorInfo> Vendors { get; set; } = new List<VendorInfo>();
        
        public string SearchTerm { get; set; } = "";
        public int? FilterCategoryId { get; set; }
        public int? FilterSubCategoryId { get; set; }

        public async Task OnGetAsync(int? pageIndex = null, string searchTerm = null, int? categoryId = null, int? subCategoryId = null)
        {
            SearchTerm = searchTerm;
            FilterCategoryId = categoryId;
            FilterSubCategoryId = subCategoryId;

            var query = _context.SkuHeaders
                .Include(s => s.Category)
                .Include(s => s.SubCategory)
                .AsQueryable();

            // Apply Filters
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(s => s.ItemName.Contains(SearchTerm));
            }

            if (FilterCategoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == FilterCategoryId);
            }

            if (FilterSubCategoryId.HasValue)
            {
                query = query.Where(s => s.SubCategoryId == FilterSubCategoryId);
            }


            query = query.OrderByDescending(s => s.SkuId);

            SkuHeaders = await PaginatedList<SkuHeader>.CreateAsync(query, pageIndex ?? 1, 10);

            Categories = await _context.Categories.ToListAsync();
            SubCategories = await _context.SubCategories.ToListAsync();
            Vendors = await _context.VendorInfos.ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync(
            string ProductName,
            int CategoryId,
            int? SubCategoryId,
            string PackagingType,
            string PackagingUnit,
            string PackSize,
            string UOM,
            string Supplier,
            decimal? SellingPrice,
            decimal? UnitCost)
        {
            if (!PermissionHelper.HasPermission(User, "SKU", 'C')) return Forbid();

            if (string.IsNullOrWhiteSpace(ProductName))
            {
                ModelState.AddModelError("", "Product Name is required.");
                return await InitializeAndReturnPage();
            }

            var newSku = new SkuHeader
            {
                ItemName = ProductName.Trim(),
                CategoryId = CategoryId,
                SubCategoryId = SubCategoryId,
                PackagingType = PackagingType ?? "",
                PackagingUnit = PackagingUnit ?? "",
                PackSize = PackSize ?? "",
                Uom = UOM ?? "",
                Supplier = Supplier ?? "",
                IsSellingPriceEnabled = true,
                IsReorderLevelEnabled = true,
                SellingPrice = SellingPrice ?? 0,
                UnitCost = UnitCost
            };

            _context.SkuHeaders.Add(newSku);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(
            int SkuId,
            string ProductName,
            int CategoryId,
            int? SubCategoryId,
            string PackagingType,
            string PackagingUnit,
            string PackSize,
            string UOM,
            string Supplier,
            decimal SellingPrice,
            decimal? UnitCost)
        {
            if (!PermissionHelper.HasPermission(User, "SKU", 'U')) return Forbid();

            var sku = await _context.SkuHeaders.FindAsync(SkuId);
            if (sku == null)
            {
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(ProductName))
            {
                ModelState.AddModelError("", "Product Name is required.");
                return await InitializeAndReturnPage();
            }

            sku.ItemName = ProductName.Trim();
            sku.CategoryId = CategoryId;
            sku.SubCategoryId = SubCategoryId;
            sku.PackagingType = PackagingType ?? "";
            sku.PackagingUnit = PackagingUnit ?? "";
            sku.PackSize = PackSize ?? "";
            sku.Uom = UOM ?? "";
            sku.Supplier = Supplier ?? "";
            sku.IsSellingPriceEnabled = true;
            sku.IsReorderLevelEnabled = true;
            sku.SellingPrice = SellingPrice;
            sku.UnitCost = UnitCost;

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        private async Task<IActionResult> InitializeAndReturnPage()
        {
            await OnGetAsync(1);
            return Page();
        }
        public async Task<IActionResult> OnPostRejectAsync(int RejectId, string RejectName, decimal RejectQty, string RejectUOM, string RejectReason)
        {
            if (!PermissionHelper.HasPermission(User, "Rejects", 'C')) return Forbid();

            if (RejectId <= 0 || RejectQty <= 0)
            {
                return await InitializeAndReturnPage();
            }

            var sku = await _context.SkuHeaders.FindAsync(RejectId);
            if (sku == null)
            {
                return await InitializeAndReturnPage();
            }

            var reject = new RejectItem
            {
                SkuId = RejectId,
                ItemName = sku.ItemName,
                Quantity = RejectQty,
                Uom = sku.Uom,
                Reason = string.IsNullOrWhiteSpace(RejectReason) ? "N/A" : RejectReason,
                RejectedAt = System.DateTime.Now,
                RejectType = "SKU"
            };

            _context.RejectItems.Add(reject);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Dashboard/Rejects", new { tab = "sku" });
        }
    }
}

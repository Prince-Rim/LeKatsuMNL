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

        [BindProperty]
        public SkuHeader Sku { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public PaginatedList<SkuHeader> SkuHeaders { get; set; } = default!;
        public List<Category> Categories { get; set; } = [];
        public IList<SubCategory> SubCategories { get; set; } = [];
        
        public string SearchTerm { get; set; } = "";
        public int? FilterCategoryId { get; set; }
        public int? FilterSubCategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync(int? pageIndex = null, string searchTerm = null, int? categoryId = null, int? subCategoryId = null)
        {
            SearchTerm = searchTerm;
            FilterCategoryId = categoryId;
            FilterSubCategoryId = subCategoryId;

            var query = _context.SkuHeaders
                .Where(s => !s.IsArchived)
                .Include(s => s.Category)
                .Include(s => s.SubCategory)
                .AsQueryable();

            // Apply Filters
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var search = SearchTerm.ToLower();
                int? parsedId = null;
                if (search.StartsWith("sku-", StringComparison.OrdinalIgnoreCase) && int.TryParse(search.AsSpan(4), out int id))
                    parsedId = id;
                else if (int.TryParse(search, out int id2))
                    parsedId = id2;

                query = query.Where(s => 
                    s.ItemName.ToLower().Contains(search) || 
                    (parsedId.HasValue && s.SkuId == parsedId.Value));
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

            int pageSize = PageSize > 0 ? PageSize : 10;
            SkuHeaders = await PaginatedList<SkuHeader>.CreateAsync(query, pageIndex ?? 1, pageSize);

            Categories = await _context.Categories.Where(c => !c.IsArchived).ToListAsync();
            SubCategories = await _context.SubCategories.ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync(
            string ProductName,
            int CategoryId,
            int? SubCategoryId,
            string PackagingType,
            string PackagingUnit,
            string PackSize,
            string UOM,
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
                IsSellingPriceEnabled = true,
                IsReorderLevelEnabled = true,
                SellingPrice = SellingPrice ?? 0,
                UnitCost = UnitCost
            };

            _context.SkuHeaders.Add(newSku);
            await _context.SaveChangesAsync();

            var firstVendor = await _context.VendorInfos.Where(v => !v.IsArchived).OrderBy(v => v.VendorId).FirstOrDefaultAsync();
            int defaultVendorId = firstVendor?.VendorId ?? 0;

            // Automatically create inventory entry for tracking
            var inventory = new CommissaryInventory
            {
                SkuId = newSku.SkuId,
                ItemName = newSku.ItemName,
                Stock = 0,
                Uom = newSku.Uom,
                CostPrice = newSku.UnitCost ?? 0,
                ReorderValue = 0,
                CategoryId = newSku.CategoryId,
                VendorId = defaultVendorId,
                Yield = "100%"
            };
            _context.CommissaryInventories.Add(inventory);
            await _context.SaveChangesAsync();

            StatusMessage = "Successfully recorded. Your new SKU has been added to the inventory.";
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
            sku.IsSellingPriceEnabled = true;
            sku.IsReorderLevelEnabled = true;
            sku.SellingPrice = SellingPrice;
            sku.UnitCost = UnitCost;

            // Ensure inventory record exists
            var inv = await _context.CommissaryInventories.FirstOrDefaultAsync(i => i.SkuId == sku.SkuId);
            if (inv == null)
            {
                var firstVendor = await _context.VendorInfos.Where(v => !v.IsArchived).OrderBy(v => v.VendorId).FirstOrDefaultAsync();
                int defaultVendorId = firstVendor?.VendorId ?? 0;

                inv = new CommissaryInventory
                {
                    SkuId = sku.SkuId,
                    ItemName = sku.ItemName,
                    Stock = 0,
                    Uom = sku.Uom,
                    CostPrice = sku.UnitCost ?? 0,
                    ReorderValue = 0,
                    CategoryId = sku.CategoryId,
                    VendorId = defaultVendorId,
                    Yield = "100%"
                };
                _context.CommissaryInventories.Add(inv);
            }
            else
            {
                inv.ItemName = sku.ItemName;
                inv.Uom = sku.Uom;
                inv.CostPrice = sku.UnitCost ?? 0;
                inv.CategoryId = sku.CategoryId;
            }

            await _context.SaveChangesAsync();

            StatusMessage = "Successfully recorded. The SKU details have been updated.";
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

            // Log Transaction
            var transactionType = await _context.InvTransactionTypes.FirstOrDefaultAsync(t => t.TransactionType == "Rejected");
            if (transactionType == null)
            {
                transactionType = new InvTransactionType { TransactionType = "Rejected" };
                _context.InvTransactionTypes.Add(transactionType);
                await _context.SaveChangesAsync();
            }

            // Find matching commissary inventory for this SKU to track stock movement
            var ci = await _context.CommissaryInventories.FirstOrDefaultAsync(i => i.SkuId == RejectId);
            if (ci != null)
            {
                ci.Stock -= RejectQty;
                
                var transaction = new InventoryTransaction
                {
                    ComId = ci.ComId,
                    TypeId = transactionType.TypeId,
                    QuantityChange = -RejectQty,
                    TimeStamp = System.DateTime.Now
                };
                _context.InventoryTransactions.Add(transaction);
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

            StatusMessage = "SKU request has been rejected.";
            return RedirectToPage("/Dashboard/Rejects", new { tab = "sku" });
        }

        public async Task<IActionResult> OnPostArchiveAsync(int id)
        {
            var sku = await _context.SkuHeaders.FindAsync(id);
            if (sku == null) return NotFound();

            sku.IsArchived = true;
            await _context.SaveChangesAsync();

            StatusMessage = "Successfully archived. The SKU has been moved to archives.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBulkArchiveAsync(string ids)
        {
            if (!PermissionHelper.HasPermission(User, "SKU", 'D')) return Forbid();
            if (string.IsNullOrEmpty(ids)) return RedirectToPage();

            var idList = ids.Split(',').Select(int.Parse).ToList();
            var skus = await _context.SkuHeaders.Where(s => idList.Contains(s.SkuId)).ToListAsync();
            foreach (var sku in skus)
            {
                sku.IsArchived = true;
            }

            await _context.SaveChangesAsync();

            StatusMessage = "Selected items have been archived.";
            return RedirectToPage();
        }
    }
}

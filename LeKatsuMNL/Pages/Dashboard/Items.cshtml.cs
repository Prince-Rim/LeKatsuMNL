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

        [BindProperty]
        public CommissaryInventory Item { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public PaginatedList<CommissaryInventory> Items { get; set; } = default!;
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public List<VendorInfo> Vendors { get; set; } = new List<VendorInfo>();

        public InputModel NewItem { get; set; } = new InputModel();
        public UpdateModel UpdateItem { get; set; } = new UpdateModel();

        public class UpdateModel : InputModel
        {
            public int ComId { get; set; }
        }

        public string SearchTerm { get; set; } = "";
        public int? FilterCategoryId { get; set; }
        public int? FilterSubCategoryId { get; set; }
        public int? FilterVendorId { get; set; }
        public string StockStatus { get; set; } = "All";
        public string RepackStatus { get; set; } = "All";
        
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public class InputModel
        {
            public string ItemName { get; set; }
            public int CategoryId { get; set; }
            public int? SubCategoryId { get; set; }
            public string PackagingType { get; set; }
            public string PackagingUnit { get; set; }
            public string PackSize { get; set; }
            public string UOM { get; set; }
            public int? VendorId { get; set; }
            public decimal? CostPrice { get; set; }
            public decimal? SellingPrice { get; set; }
            public decimal Stock { get; set; }
            public bool IsRepack { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(
            int? pageIndex = null, 
            string searchTerm = null, 
            int? categoryId = null, 
            int? subCategoryId = null, 
            int? vendorId = null, 
            string stockStatus = null,
            string repackStatus = null)
        {
            SearchTerm = searchTerm;
            FilterCategoryId = categoryId;
            FilterSubCategoryId = subCategoryId;
            FilterVendorId = vendorId;
            StockStatus = stockStatus ?? "All";
            RepackStatus = repackStatus ?? "All";

            var query = _context.CommissaryInventories
                .Where(i => !i.IsArchived)
                .Where(i => i.SkuId == null)
                .Include(i => i.Category)
                .Include(i => i.SubCategory)
                .Include(i => i.Vendor)
                .AsQueryable();

            // Apply Filters
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var search = SearchTerm.ToLower();
                int? parsedId = null;
                if (search.StartsWith("ing-") && int.TryParse(search.Substring(4), out int id))
                    parsedId = id;
                else if (int.TryParse(search, out int id2))
                    parsedId = id2;

                query = query.Where(i => 
                    i.ItemName.ToLower().Contains(search) || 
                    (parsedId.HasValue && i.ComId == parsedId.Value));
            }

            if (FilterCategoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == FilterCategoryId);
            }

            if (FilterSubCategoryId.HasValue)
            {
                query = query.Where(i => i.SubCategoryId == FilterSubCategoryId);
            }

            if (FilterVendorId.HasValue)
            {
                query = query.Where(i => i.VendorId == FilterVendorId);
            }

            if (StockStatus != "All")
            {
                if (StockStatus == "Low Stock")
                {
                    query = query.Where(i => i.Stock > 0 && i.Stock < 10);
                }
                else if (StockStatus == "Out of Stock")
                {
                    query = query.Where(i => i.Stock <= 0);
                }
                else if (StockStatus == "In Stock")
                {
                    query = query.Where(i => i.Stock >= 10);
                }
            }

            if (RepackStatus != "All")
            {
                if (RepackStatus == "Repack")
                {
                    query = query.Where(i => i.IsRepack);
                }
                else if (RepackStatus == "Non-Repack")
                {
                    query = query.Where(i => !i.IsRepack);
                }
            }

            query = query.OrderByDescending(i => i.ComId);

            int pageSize = PageSize > 0 ? PageSize : 10;
            Items = await PaginatedList<CommissaryInventory>.CreateAsync(query, pageIndex ?? 1, pageSize);
            
            Categories = await _context.Categories.Where(c => !c.IsArchived).ToListAsync();
            SubCategories = await _context.SubCategories.ToListAsync();
            Vendors = await _context.VendorInfos.Where(v => !v.IsArchived).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(
            string ItemName,
            int CategoryId,
            int? SubCategoryId,
            int? VendorId,
            string PackagingType,
            string PackagingUnit,
            string PackSize,
            string UOM,
            decimal? CostPrice,
            decimal? SellingPrice,
            decimal Stock,
            bool IsRepack)
        {
            if (!PermissionHelper.HasPermission(User, "Items", 'C')) return Forbid();

            if (string.IsNullOrWhiteSpace(ItemName))
            {
                return RedirectToPage();
            }

            var isActiveCategory = await _context.Categories
                .AnyAsync(c => c.CategoryId == CategoryId && !c.IsArchived);
            if (!isActiveCategory)
            {
                StatusMessage = "Selected category is archived. Please choose an active category.";
                return RedirectToPage();
            }

            // Yield logic -> Type / Size + Unit
            string calculatedYield = string.Empty;
            if (!string.IsNullOrEmpty(PackagingUnit))
            {
                if (!string.IsNullOrEmpty(PackagingType))
                {
                    calculatedYield = $"{PackagingType}/{(string.IsNullOrEmpty(PackSize) ? "1" : PackSize)}{PackagingUnit}";
                }
                else
                {
                    calculatedYield = $"{(string.IsNullOrEmpty(PackSize) ? "1" : PackSize)}{PackagingUnit}";
                }
            }

            var item = new CommissaryInventory
            {
                ItemName = ItemName,
                CategoryId = CategoryId,
                SubCategoryId = SubCategoryId,
                VendorId = VendorId,
                Yield = calculatedYield,
                Uom = UOM ?? "pack",
                CostPrice = CostPrice ?? 0,
                SellingPrice = SellingPrice ?? 0,
                Stock = Stock,
                IsRepack = IsRepack
            };

            _context.CommissaryInventories.Add(item);
            await _context.SaveChangesAsync();

            StatusMessage = "Successfully recorded. Your new item has been added to the inventory.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(
            int ComId,
            string ItemName,
            int CategoryId,
            int? SubCategoryId,
            int? VendorId,
            string PackagingType,
            string PackagingUnit,
            string PackSize,
            string UOM,
            decimal? CostPrice,
            decimal? SellingPrice,
            decimal Stock,
            bool IsRepack)
        {
            if (!PermissionHelper.HasPermission(User, "Items", 'U')) return Forbid();

            var isActiveCategory = await _context.Categories
                .AnyAsync(c => c.CategoryId == CategoryId && !c.IsArchived);
            if (!isActiveCategory)
            {
                StatusMessage = "Selected category is archived. Please choose an active category.";
                return RedirectToPage();
            }

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
            if (!string.IsNullOrEmpty(PackagingUnit))
            {
                if (!string.IsNullOrEmpty(PackagingType))
                {
                    calculatedYield = $"{PackagingType}/{(string.IsNullOrEmpty(PackSize) ? "1" : PackSize)}{PackagingUnit}";
                }
                else
                {
                    calculatedYield = $"{(string.IsNullOrEmpty(PackSize) ? "1" : PackSize)}{PackagingUnit}";
                }
            }

            item.ItemName = ItemName;
            item.CategoryId = CategoryId;
            item.SubCategoryId = SubCategoryId;
            item.VendorId = VendorId;
            item.Yield = calculatedYield;
            item.Uom = UOM ?? "pack";
            item.CostPrice = CostPrice ?? 0;
            item.SellingPrice = SellingPrice ?? 0;
            item.Stock = Stock;
            item.IsRepack = IsRepack;

            await _context.SaveChangesAsync();

            StatusMessage = "Successfully recorded. The item details have been updated.";
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostRejectAsync(int RejectId, string RejectName, decimal RejectQty, string RejectUOM, string RejectReason)
        {
            if (!PermissionHelper.HasPermission(User, "Rejects", 'C')) return Forbid();

            if (RejectId <= 0 || RejectQty <= 0)
            {
                return RedirectToPage();
            }

            var item = await _context.CommissaryInventories.FindAsync(RejectId);
            if (item == null)
            {
                return RedirectToPage();
            }

            // Validate stock
            if (RejectQty > item.Stock)
            {
                return RedirectToPage(); // Ideally add ModelError, but page has many Redirects
            }

            // Deduct Stock
            item.Stock -= RejectQty;

            // Log Transaction
            var transactionType = await _context.InvTransactionTypes.FirstOrDefaultAsync(t => t.TransactionType == "Rejected");
            if (transactionType == null)
            {
                transactionType = new InvTransactionType { TransactionType = "Rejected" };
                _context.InvTransactionTypes.Add(transactionType);
                await _context.SaveChangesAsync();
            }

            var transaction = new InventoryTransaction
            {
                ComId = RejectId,
                TypeId = transactionType.TypeId,
                QuantityChange = -RejectQty,
                TimeStamp = System.DateTime.Now
            };
            _context.InventoryTransactions.Add(transaction);

            var reject = new RejectItem
            {
                ComId = RejectId,
                ItemName = item.ItemName,
                Quantity = RejectQty,
                Uom = item.Uom,
                Reason = string.IsNullOrWhiteSpace(RejectReason) ? "N/A" : RejectReason,
                RejectedAt = System.DateTime.Now,
                RejectType = "Recipe"
            };

            _context.RejectItems.Add(reject);
            await _context.SaveChangesAsync();

            StatusMessage = "Item request has been rejected.";
            return RedirectToPage("/Dashboard/Rejects", new { tab = "recipe" });
        }

        public async Task<IActionResult> OnPostArchiveAsync(int id)
        {
            var item = await _context.CommissaryInventories.FindAsync(id);
            if (item == null) return NotFound();

            item.IsArchived = true;
            await _context.SaveChangesAsync();

            StatusMessage = "Successfully archived. The item has been moved to archives.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBulkArchiveAsync(string ids)
        {
            if (!PermissionHelper.HasPermission(User, "Items", 'D')) return Forbid();
            if (string.IsNullOrEmpty(ids)) return RedirectToPage();

            var idList = ids.Split(',').Select(int.Parse).ToList();
            var items = await _context.CommissaryInventories.Where(i => idList.Contains(i.ComId)).ToListAsync();
            foreach (var item in items)
            {
                item.IsArchived = true;
            }

            await _context.SaveChangesAsync();
            StatusMessage = "Selected items have been archived.";
            return RedirectToPage();
        }
    }
}

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
            public int VendorId { get; set; }
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
            
            Categories = await _context.Categories.ToListAsync();
            SubCategories = await _context.SubCategories.ToListAsync();
            Vendors = await _context.VendorInfos.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(
            string ItemName,
            int CategoryId,
            int? SubCategoryId,
            int VendorId,
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

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(
            int ComId,
            string ItemName,
            int CategoryId,
            int? SubCategoryId,
            int VendorId,
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
            item.SubCategoryId = SubCategoryId;
            item.VendorId = VendorId;
            item.Yield = calculatedYield;
            item.Uom = UOM ?? "pack";
            item.CostPrice = CostPrice ?? 0;
            item.SellingPrice = SellingPrice ?? 0;
            item.Stock = Stock;
            item.IsRepack = IsRepack;

            await _context.SaveChangesAsync();
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

            return RedirectToPage("/Dashboard/Rejects", new { tab = "recipe" });
        }
    }
}

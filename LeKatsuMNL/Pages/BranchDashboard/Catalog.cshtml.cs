using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using LeKatsuMNL.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System;

namespace LeKatsuMNL.Pages.BranchDashboard
{
    public class CatalogModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public CatalogModel(LeKatsuDb context)
        {
            _context = context;
        }

        public class CatalogItem
        {
            public string ItemId { get; set; }   // "SKU-5" or "COM-12"
            public string ItemName { get; set; }
            public string Classification { get; set; } // Category name
            public string Yield { get; set; }
            public decimal SellingPrice { get; set; }
            public decimal Stock { get; set; }
        }

        public PaginatedList<CatalogItem> Items { get; set; }
        public List<string> Classifications { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Classification { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync(int? pageIndex)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login/login");

            // Build combined catalog: SKUs + CommissaryInventory items with SellingPrice
            var skuItems = await _context.SkuHeaders
                .Where(s => !s.IsArchived)
                .Include(s => s.Category)
                .Select(s => new CatalogItem
                {
                    ItemId = "SKU-" + s.SkuId,
                    ItemName = s.ItemName,
                    Classification = s.Category != null ? s.Category.CategoryName : "—",
                    Yield = !string.IsNullOrEmpty(s.PackagingUnit) ? (!string.IsNullOrEmpty(s.PackagingType) ? $"{s.PackagingType}/{(string.IsNullOrEmpty(s.PackSize) ? "1" : s.PackSize)}{s.PackagingUnit}" : $"{(string.IsNullOrEmpty(s.PackSize) ? "1" : s.PackSize)}{s.PackagingUnit}") : s.Uom,
                    SellingPrice = s.SellingPrice,
                    Stock = 0
                })
                .ToListAsync();

            var comItems = await _context.CommissaryInventories
                .Where(c => !c.IsArchived && c.SellingPrice > 0)
                .Include(c => c.Category)
                .Select(c => new CatalogItem
                {
                    ItemId = "COM-" + c.ComId,
                    ItemName = c.ItemName,
                    Classification = c.Category != null ? c.Category.CategoryName : "—",
                    Yield = c.Yield ?? c.Uom,
                    SellingPrice = c.SellingPrice,
                    Stock = c.Stock
                })
                .ToListAsync();

            var combined = skuItems.Concat(comItems).AsQueryable();

            // Filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var s = SearchTerm.ToLower();
                combined = combined.Where(i => i.ItemName.ToLower().Contains(s) || i.Classification.ToLower().Contains(s));
            }

            if (!string.IsNullOrEmpty(Classification) && Classification != "All")
            {
                combined = combined.Where(i => i.Classification == Classification);
            }

            combined = combined.OrderBy(i => i.Classification).ThenBy(i => i.ItemName);

            // Distinct classifications for filter dropdown
            Classifications = skuItems.Concat(comItems)
                .Select(i => i.Classification)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            int ps = PageSize > 0 ? PageSize : 10;
            Items = PaginatedList<CatalogItem>.Create(combined, pageIndex ?? 1, ps);

            return Page();
        }

        public async Task<IActionResult> OnPostSubmitOrderAsync(
            List<string> ItemIds,
            List<decimal> Quantities)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login/login");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("/Login/login");

            if (ItemIds == null || !ItemIds.Any())
                return RedirectToPage();

            // 1. Validate that we have at least one valid item with qty > 0
            bool hasValidItems = false;
            for (int i = 0; i < ItemIds.Count; i++)
            {
                if (Quantities.Count > i && Quantities[i] > 0)
                {
                    hasValidItems = true;
                    break;
                }
            }

            if (!hasValidItems)
            {
                TempData["ErrorMessage"] = "Please select at least one item with a valid quantity.";
                return RedirectToPage();
            }

            // 2. Use a transaction to ensure atomic save
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Calculate Expected Delivery Date
                DateTime now = DateTime.Now;
                DateTime expectedDelivery;
                if (now.Hour >= 17) // 5 PM or later
                {
                    expectedDelivery = now.Date.AddDays(3);
                }
                else
                {
                    expectedDelivery = now.Date.AddDays(2);
                }

                var order = new OrderInfo
                {
                    BranchManagerId = branchManagerId,
                    OrderDate = now,
                    DeliveryDate = expectedDelivery,
                    Status = "Pending"
                };

                _context.OrderInfos.Add(order);
                await _context.SaveChangesAsync();

                for (int i = 0; i < ItemIds.Count; i++)
                {
                    if (Quantities.Count <= i || Quantities[i] <= 0) continue;

                    var parts = ItemIds[i].Split('-');
                    if (parts.Length != 2) continue;

                    var type = parts[0];
                    if (!int.TryParse(parts[1], out int id)) continue;

                    decimal actualPrice = 0;
                    int? skuId = null;
                    int? comId = null;

                    if (type == "SKU")
                    {
                        var sku = await _context.SkuHeaders.FindAsync(id);
                        if (sku == null) continue;
                        actualPrice = sku.SellingPrice;
                        skuId = id;
                    }
                    else if (type == "COM")
                    {
                        var com = await _context.CommissaryInventories.FindAsync(id);
                        if (com == null) continue;
                        actualPrice = com.SellingPrice;
                        comId = id;
                    }
                    else continue;

                    var orderList = new OrderList
                    {
                        OrderId = order.OrderId,
                        SkuId = skuId,
                        ComId = comId,
                        Quantity = Quantities[i],
                        TotalPrice = actualPrice * Quantities[i]
                    };

                    _context.OrderLists.Add(orderList);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Purchase order submitted successfully!";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "An error occurred while submitting your order. Please try again.";
                // Log exception here if logging is available
            }

            return RedirectToPage();
        }
    }
}

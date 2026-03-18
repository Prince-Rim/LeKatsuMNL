using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using LeKatsuMNL.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class BranchOrdersModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public BranchOrdersModel(LeKatsuDb context)
        {
            _context = context;
        }

        public PaginatedList<OrderInfo> Orders { get; set; } = default!;
        public List<BranchManager> BranchManagers { get; set; } = new();
        
        public class AvailableItem
        {
            public int? SkuId { get; set; }
            public int? ComId { get; set; }
            public string ItemName { get; set; }
            public decimal SellingPrice { get; set; }
            public string Type { get; set; } // "SKU" or "Ingredient"
        }
        public List<AvailableItem> AvailableItems { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? pageIndex)
        {
            var query = _context.OrderInfos
                .Include(o => o.BranchManager)
                    .ThenInclude(bm => bm.BranchLocation)
                .Include(o => o.Invoices)
                .AsQueryable(); // Add AsQueryable() to allow further filtering

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var search = SearchTerm.ToLower();
                
                // If search contains hyphen, try to parse the part after hyphen as ID
                int? parsedId = null;
                if (search.Contains("-"))
                {
                    var parts = search.Split('-');
                    if (int.TryParse(parts.Last(), out int id))
                    {
                        parsedId = id;
                    }
                }
                else if (int.TryParse(search, out int id))
                {
                    parsedId = id;
                }

                query = query.Where(o =>
                    o.BranchManager.BranchLocation.BranchName.ToLower().Contains(search) ||
                    o.Status.ToLower().Contains(search) ||
                    (parsedId.HasValue && o.OrderId == parsedId.Value));
            }

            query = query.OrderByDescending(o => o.OrderDate);
            
            int pageSize = PageSize > 0 ? PageSize : 10;
            Orders = await PaginatedList<OrderInfo>.CreateAsync(query, pageIndex ?? 1, pageSize);
            
            BranchManagers = await _context.BranchManagers
                .Include(bm => bm.BranchLocation)
                .Where(bm => bm.Status == "Active")
                .ToListAsync();

            var skus = await _context.SkuHeaders
                .Select(s => new AvailableItem
                {
                    SkuId = s.SkuId,
                    ItemName = s.ItemName,
                    SellingPrice = s.SellingPrice,
                    Type = "SKU"
                })
                .ToListAsync();

            var ingredients = await _context.CommissaryInventories
                .Where(i => i.SellingPrice > 0)
                .Select(i => new AvailableItem
                {
                    ComId = i.ComId,
                    ItemName = i.ItemName,
                    SellingPrice = i.SellingPrice,
                    Type = "Ingredient"
                })
                .ToListAsync();

            AvailableItems = skus.Concat(ingredients).OrderBy(i => i.ItemName).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateOrderAsync(int BranchManagerId, List<string> ItemIds, List<decimal> Quantities, List<decimal> Prices)
        {
            if (BranchManagerId <= 0 || ItemIds == null || !ItemIds.Any())
            {
                return RedirectToPage();
            }

            var order = new OrderInfo
            {
                BranchManagerId = BranchManagerId,
                OrderDate = System.DateTime.Now,
                Status = "Pending"
            };

            _context.OrderInfos.Add(order);
            await _context.SaveChangesAsync();

            for (int i = 0; i < ItemIds.Count; i++)
            {
                if (Quantities[i] <= 0) continue;

                var idParts = ItemIds[i].Split('-');
                if (idParts.Length != 2) continue;

                var type = idParts[0];
                int id = int.Parse(idParts[1]);

                var orderList = new OrderList
                {
                    OrderId = order.OrderId,
                    Quantity = Quantities[i],
                    TotalPrice = Prices[i] * Quantities[i]
                };

                if (type == "SKU")
                {
                    orderList.SkuId = id;
                }
                else if (type == "COM")
                {
                    orderList.ComId = id;
                }

                _context.OrderLists.Add(orderList);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}

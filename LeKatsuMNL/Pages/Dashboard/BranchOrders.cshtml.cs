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
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        public List<BranchManager> BranchManagers { get; set; } = new();
        public List<SkuHeader> AvailableSkus { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? pageIndex, int? pageSize)
        {
            IQueryable<OrderInfo> query = _context.OrderInfos
                .Include(o => o.BranchManager)
                    .ThenInclude(bm => bm.BranchLocation)
                .Include(o => o.Invoices)
                .OrderByDescending(o => o.OrderDate);

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var cleanSearch = SearchTerm.Trim();
                // Handle "ORD-" prefix
                if (cleanSearch.StartsWith("ORD-", StringComparison.OrdinalIgnoreCase))
                {
                    cleanSearch = cleanSearch.Substring(4);
                }

                var isNumeric = int.TryParse(cleanSearch, out int orderId);
                
                query = query.Where(o => (isNumeric && o.OrderId == orderId) || 
                                       o.OrderId.ToString().Contains(SearchTerm) || 
                                       (o.BranchManager != null && o.BranchManager.BranchLocation != null && o.BranchManager.BranchLocation.BranchName.Contains(SearchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(Status) && Status != "Status")
            {
                query = query.Where(o => o.Status == Status);
            }
            
            Orders = await PaginatedList<OrderInfo>.CreateAsync(query, pageIndex ?? 1, pageSize ?? 10);
            
            BranchManagers = await _context.BranchManagers
                .Include(bm => bm.BranchLocation)
                .Where(bm => bm.Status == "Active")
                .ToListAsync();

            AvailableSkus = await _context.SkuHeaders
                .OrderBy(s => s.ItemName)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateOrderAsync(int BranchManagerId, List<int> SkuIds, List<decimal> Quantities, List<decimal> Prices)
        {
            if (BranchManagerId <= 0 || SkuIds == null || !SkuIds.Any())
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

            for (int i = 0; i < SkuIds.Count; i++)
            {
                if (Quantities[i] <= 0) continue;

                var orderList = new OrderList
                {
                    OrderId = order.OrderId,
                    SkuId = SkuIds[i],
                    Quantity = Quantities[i],
                    TotalPrice = Prices[i] * Quantities[i]
                };
                _context.OrderLists.Add(orderList);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}

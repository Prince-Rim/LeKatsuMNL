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
        public List<SkuHeader> AvailableSkus { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? pageIndex)
        {
            var query = _context.OrderInfos
                .Include(o => o.BranchManager)
                    .ThenInclude(bm => bm.BranchLocation)
                .OrderByDescending(o => o.OrderDate);
            
            Orders = await PaginatedList<OrderInfo>.CreateAsync(query, pageIndex ?? 1, 10);
            
            BranchManagers = await _context.BranchManagers
                .Include(bm => bm.BranchLocation)
                .Where(bm => bm.Status == "Active")
                .ToListAsync();

            AvailableSkus = await _context.SkuHeaders
                .OrderBy(s => s.ItemName)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateOrderAsync(int BranchManagerId, List<int> SkuIds, List<decimal> Quantities)
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

                var sku = await _context.SkuHeaders.FindAsync(SkuIds[i]);
                if (sku == null) continue;

                var orderList = new OrderList
                {
                    OrderId = order.OrderId,
                    SkuId = SkuIds[i],
                    Quantity = Quantities[i],
                    TotalPrice = (sku.SellingPrice ?? 0) * Quantities[i]
                };
                _context.OrderLists.Add(orderList);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}

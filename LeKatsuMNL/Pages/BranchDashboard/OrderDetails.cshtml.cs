using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;

namespace LeKatsuMNL.Pages.BranchDashboard
{
    public class OrderDetailsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public OrderDetailsModel(LeKatsuDb context)
        {
            _context = context;
        }

        public OrderInfo Order { get; set; }
        public Invoice Invoice { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login/login");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("/Login/login");

            Order = await _context.OrderInfos
                .Where(o => o.OrderId == id && o.BranchManagerId == branchManagerId && !o.IsArchived)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                        .ThenInclude(s => s != null ? s.Category : null)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.CommissaryInventory)
                        .ThenInclude(c => c != null ? c.Category : null)
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync();

            if (Order == null)
                return RedirectToPage("./MyOrder");

            Invoice = Order.Invoices?.FirstOrDefault();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("./MyOrder");

            var order = await _context.OrderInfos
                .FirstOrDefaultAsync(o => o.OrderId == id && o.BranchManagerId == branchManagerId);

            if (order != null && order.Status == "Pending")
            {
                order.Status = "Cancelled";
                await _context.SaveChangesAsync();
                TempData["Message"] = "Order cancelled.";
            }
            return RedirectToPage("./MyOrder");
        }
    }
}

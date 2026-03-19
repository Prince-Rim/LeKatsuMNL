using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace LeKatsuMNL.Pages.BranchDashboard
{
    public class InvoicePDFModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public InvoicePDFModel(LeKatsuDb context)
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
                .Include(o => o.BranchManager)
                    .ThenInclude(bm => bm.BranchLocation)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                        .ThenInclude(s => s.Category)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.CommissaryInventory)
                        .ThenInclude(c => c.Category)
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync();

            if (Order == null)
                return NotFound();

            Invoice = Order.Invoices?.FirstOrDefault();
            if (Invoice == null)
                return RedirectToPage("./MyOrder");

            return Page();
        }
    }
}

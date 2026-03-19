using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Threading.Tasks;
using System.Security.Claims;

namespace LeKatsuMNL.Pages.BranchDashboard
{
    public class PrintPOModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public PrintPOModel(LeKatsuDb context)
        {
            _context = context;
        }

        public OrderInfo Order { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login/login");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("/Login/login");

            Order = await _context.OrderInfos
                .Include(o => o.BranchManager)
                    .ThenInclude(bm => bm.BranchLocation)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                        .ThenInclude(s => s.Category)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.CommissaryInventory)
                        .ThenInclude(c => c.Category)
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(m => m.OrderId == id
                    && m.BranchManagerId == branchManagerId
                    && !m.IsArchived);

            if (Order == null)
                return NotFound();

            return Page();
        }
    }
}

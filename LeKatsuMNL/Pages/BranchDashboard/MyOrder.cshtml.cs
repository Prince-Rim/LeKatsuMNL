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

namespace LeKatsuMNL.Pages.BranchDashboard
{
    public class MyOrderModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public MyOrderModel(LeKatsuDb context)
        {
            _context = context;
        }

        public PaginatedList<OrderInfo> Orders { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync(int? pageIndex)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login/login");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("/Login/login");

            var query = _context.OrderInfos
                .Where(o => o.BranchManagerId == branchManagerId && !o.IsArchived
                         && o.Status != "Completed" && o.Status != "Delivered" && o.Status != "Cancelled")
                .Include(o => o.Invoices)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            int ps = PageSize > 0 ? PageSize : 10;
            Orders = await PaginatedList<OrderInfo>.CreateAsync(query, pageIndex ?? 1, ps);

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage();

            var order = await _context.OrderInfos
                .FirstOrDefaultAsync(o => o.OrderId == id && o.BranchManagerId == branchManagerId);
            if (order != null && order.Status == "Pending")
            {
                order.Status = "Cancelled";
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}

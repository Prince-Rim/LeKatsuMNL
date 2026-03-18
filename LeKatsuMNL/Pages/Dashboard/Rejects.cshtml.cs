using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using LeKatsuMNL.Helpers;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class RejectsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public RejectsModel(LeKatsuDb context)
        {
            _context = context;
        }

        public PaginatedList<RejectItem> RejectLogs { get; set; } = default!;

        public int PageSize { get; set; } = 10;
 
        [TempData]
        public string StatusMessage { get; set; }

         public async Task OnGetAsync(int? pageIndex)
         {
             var query = _context.RejectItems.Where(r => r.RejectType == "Recipe");
 
             int pageSize = PageSize > 0 ? PageSize : 10;
             RejectLogs = await PaginatedList<RejectItem>.CreateAsync(
                 query.OrderByDescending(r => r.RejectedAt), 
                 pageIndex ?? 1, pageSize);
         }

         public async Task<IActionResult> OnPostClearLogsAsync()
         {
             if (!PermissionHelper.HasPermission(User, "Rejects", 'D')) return Forbid();

             var rejects = await _context.RejectItems.ToListAsync();
             _context.RejectItems.RemoveRange(rejects);
             await _context.SaveChangesAsync();

             StatusMessage = "All reject logs have been successfully cleared.";
             return RedirectToPage();
         }
    }
}

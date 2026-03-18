using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Threading.Tasks;

namespace LeKatsuMNL.Pages.Dashboard
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
            Order = await _context.OrderInfos
                .Include(o => o.BranchManager)
                    .ThenInclude(bm => bm.BranchLocation)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.CommissaryInventory)
                .FirstOrDefaultAsync(m => m.OrderId == id && !m.IsArchived);

            if (Order == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}

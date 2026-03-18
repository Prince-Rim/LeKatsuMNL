using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Threading.Tasks;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class PrintInvoiceModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public PrintInvoiceModel(LeKatsuDb context)
        {
            _context = context;
        }

        public OrderInfo Order { get; set; } = default!;
        public Invoice? Invoice { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _context.OrderInfos
                .Include(o => o.BranchManager)
                    .ThenInclude(bm => bm.BranchLocation)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.CommissaryInventory)
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (Order == null)
            {
                return NotFound();
            }

            Invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.OrderId == id);

            return Page();
        }
    }
}

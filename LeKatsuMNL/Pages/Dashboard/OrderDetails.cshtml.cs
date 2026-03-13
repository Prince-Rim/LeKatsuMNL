using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class OrderDetailsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public OrderDetailsModel(LeKatsuDb context)
        {
            _context = context;
        }

        public OrderInfo Order { get; set; } = default!;
        public IList<OrderComment> Comments { get; set; } = new List<OrderComment>();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("/Dashboard/BranchOrders");
            }

            // Parse ID from string like "FO-00001" to int 1
            int orderId = 0;
            if (id.StartsWith("FO-"))
            {
                int.TryParse(id.Replace("FO-", ""), out orderId);
            }
            else
            {
                int.TryParse(id, out orderId);
            }

            Order = await _context.OrderInfos
                .Include(o => o.BranchManager)
                    .ThenInclude(bm => bm.BranchLocation)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                        .ThenInclude(s => s.Category)
                .Include(o => o.OrderComments)
                    .ThenInclude(oc => oc.BranchManager)
                        .ThenInclude(bm => bm.BranchLocation)
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(m => m.OrderId == orderId);

            if (Order == null)
            {
                return NotFound();
            }

            Comments = Order.OrderComments.OrderByDescending(c => c.CommentId).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAddCommentAsync(int OrderId, string CommentText)
        {
            if (string.IsNullOrWhiteSpace(CommentText))
            {
                return RedirectToPage(new { id = $"FO-{OrderId:D5}" });
            }

            // For now, we'll associate with the order's branch manager if we don't have auth
            var order = await _context.OrderInfos.FindAsync(OrderId);
            if (order == null) return NotFound();

            var newComment = new OrderComment
            {
                OrderId = OrderId,
                BranchManagerId = order.BranchManagerId, // Placeholder for actual logged-in user
                Comment = CommentText
            };

            _context.OrderComments.Add(newComment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = $"FO-{OrderId:D5}" });
        }
    }
}

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

            // Parse ID from string like "ORD-00001" to int 1
            int orderId = 0;
            if (id.StartsWith("ORD-"))
            {
                int.TryParse(id.Replace("ORD-", ""), out orderId);
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
                .Include(o => o.OrderComments)
                    .ThenInclude(oc => oc.AdminAccount)
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
                return RedirectToPage(new { id = $"ORD-{OrderId:D5}", tab = "issues" });
            }

            // Get user info from Claims
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToPage(new { id = $"ORD-{OrderId:D5}", tab = "issues" });
            }

            var newComment = new OrderComment
            {
                OrderId = OrderId,
                Comment = CommentText
            };

            if (userRole == "Admin")
            {
                newComment.AdminAccountId = userId;
            }
            else if (userRole == "BranchManager")
            {
                newComment.BranchManagerId = userId;
            }

            _context.OrderComments.Add(newComment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = $"ORD-{OrderId:D5}", tab = "issues" });
        }

        public async Task<IActionResult> OnPostApproveOrderAsync(int OrderId)
        {
            var order = await _context.OrderInfos
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                        .ThenInclude(s => s.SkuRecipes)
                            .ThenInclude(r => r.CommissaryInventory)
                .FirstOrDefaultAsync(o => o.OrderId == OrderId);

            if (order == null) return NotFound();
            if (order.Status != "Pending") return RedirectToPage(new { id = $"ORD-{OrderId:D5}" });

            // 1. Deduct from inventory based on recipes
            foreach (var item in order.OrderLists)
            {
                if (item.SkuHeader?.SkuRecipes == null) continue;

                foreach (var recipe in item.SkuHeader.SkuRecipes)
                {
                    if (recipe.CommissaryInventory != null)
                    {
                        // Convert recipe UOM to inventory UOM before deducting
                        decimal convertedQty = Helpers.UomConverter.Convert(
                            recipe.QuantityNeeded, recipe.Uom, recipe.CommissaryInventory.Uom);
                        decimal totalDeduction = convertedQty * item.Quantity;
                        recipe.CommissaryInventory.Stock -= totalDeduction;
                        
                        // Log transaction (Optional: could add InventoryTransaction record here)
                    }
                }
            }

            // 2. Update status
            order.Status = "Approved";
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = $"ORD-{OrderId:D5}" });
        }
    }
}

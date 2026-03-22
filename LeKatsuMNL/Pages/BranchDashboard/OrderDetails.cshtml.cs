using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using LeKatsuMNL.Services;

using LeKatsuMNL.Helpers;
using System.Collections.Generic;

namespace LeKatsuMNL.Pages.BranchDashboard
{
    public class OrderDetailsModel : PageModel
    {
        private readonly LeKatsuDb _context;
        private readonly IPayMongoService _payMongoService;

        public OrderDetailsModel(LeKatsuDb context, IPayMongoService payMongoService)
        {
            _context = context;
            _payMongoService = payMongoService;
        }

        public class StockCheckDetail
        {
            public int ComId { get; set; }
            public string ItemName { get; set; } = "";
            public string Uom { get; set; } = "";
            public decimal RequiredQuantity { get; set; }
            public decimal CurrentStock { get; set; }
            public bool IsSufficient => CurrentStock >= RequiredQuantity;
        }

        public IList<StockCheckDetail> StockCheckList { get; set; } = new List<StockCheckDetail>();

        public OrderInfo Order { get; set; }
        public Invoice Invoice { get; set; }
        public System.Collections.Generic.List<OrderComment> Comments { get; set; }

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
                .Include(o => o.OrderComments)
                    .ThenInclude(oc => oc.AdminAccount)
                .Include(o => o.OrderComments)
                    .ThenInclude(oc => oc.BranchManager)
                        .ThenInclude(bm => bm != null ? bm.BranchLocation : null)
                .FirstOrDefaultAsync();

            if (Order == null)
                return RedirectToPage("./MyOrder");

            // PROACTIVE CHECK: If there's a pending invoice with SESSION: prefix, verify status with PayMongo
            var sessionInvoice = Order.Invoices?.FirstOrDefault(i => i.PaymentStatus == "Pending" && i.ReferenceNumber != null && i.ReferenceNumber.StartsWith("SESSION:"));
            if (sessionInvoice != null)
            {
                var sessionId = sessionInvoice.ReferenceNumber.Substring(8);
                var status = await _payMongoService.GetCheckoutSessionStatusAsync(sessionId);
                
                if (status == "paid")
                {
                    var details = await _payMongoService.GetPaymentDetailsAsync(sessionId);
                    sessionInvoice.PaymentStatus = "Paid";
                    sessionInvoice.PaymentDate = DateTime.Now;
                    
                    string method = details.Method;
                    if (!string.IsNullOrEmpty(method))
                    {
                        method = char.ToUpper(method[0]) + method.Substring(1).ToLower();
                        if (method.ToLower() == "paymaya") method = "Maya";
                    }
                    sessionInvoice.PaymentMethod = $"PayMongo ({method})";
                    sessionInvoice.ReferenceNumber = details.PaymentId;

                    if (Order.Status == "Approved")
                    {
                        Order.Status = "Preparing";
                        await LogSystemMessage(Order.OrderId, "Payment verified via PayMongo - Order status changed to Preparing");
                    }

                    await _context.SaveChangesAsync();
                }
                else if (status == "expired")
                {
                    // Optionally reset or handle expiry
                    sessionInvoice.ReferenceNumber = "EXPIRED:" + sessionId;
                    await _context.SaveChangesAsync();
                }
            }

            Invoice = Order.Invoices?.FirstOrDefault();
            Comments = Order.OrderComments.OrderBy(c => c.CreatedAt).ToList();

            // Populate Stock Check List (Required ingredients vs Commissary stock)
            var requirements = new Dictionary<int, decimal>();
            var stockSnapshots = new Dictionary<int, decimal>();

            foreach (var item in Order.OrderLists)
            {
                await AggregateDeductions(item.SkuId, item.ComId, item.Quantity, requirements, stockSnapshots);
            }

            foreach (var req in requirements)
            {
                var inventory = await _context.CommissaryInventories.FindAsync(req.Key);
                if (inventory != null)
                {
                    StockCheckList.Add(new StockCheckDetail
                    {
                        ComId = req.Key,
                        ItemName = inventory.ItemName ?? "Unknown",
                        Uom = inventory.Uom ?? "",
                        RequiredQuantity = req.Value,
                        CurrentStock = inventory.Stock
                    });
                }
            }

            return Page();
        }

        private async Task LogSystemMessage(int orderId, string message)
        {
            var systemComment = new OrderComment
            {
                OrderId = orderId,
                Comment = "System: " + message,
                CreatedAt = DateTime.Now
            };
            _context.OrderComments.Add(systemComment);
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
                await LogSystemMessage(id, "Order Cancelled by Branch");
                await _context.SaveChangesAsync();
                TempData["Message"] = "Order cancelled.";
            }
            return RedirectToPage("./MyOrder");
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("./MyOrder");

            var order = await _context.OrderInfos
                .FirstOrDefaultAsync(o => o.OrderId == id && o.BranchManagerId == branchManagerId);

            if (order != null && order.Status == "Delivering")
            {
                order.Status = "Completed";
                await LogSystemMessage(id, "Order received and completed by branch");
                await _context.SaveChangesAsync();
                TempData["Message"] = "Transaction completed.";
            }
            return RedirectToPage("./MyOrder", new { id = id });
        }

        public async Task<IActionResult> OnPostPayAsync(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("/Login/login");

            var order = await _context.OrderInfos
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.BranchManagerId == branchManagerId && !o.IsArchived);

            if (order == null)
                return NotFound();

            var invoice = order.Invoices.FirstOrDefault(i => i.PaymentStatus == "Pending");
            if (invoice == null)
            {
                TempData["ErrorMessage"] = "No pending invoice found for this order.";
                return RedirectToPage(new { id = id });
            }

            var description = $"Payment for Order #{order.OrderId}";
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var successUrl = $"{baseUrl}/Payment/Success?orderId={order.OrderId}";
            var cancelUrl = $"{baseUrl}/Payment/Cancel?orderId={order.OrderId}";

            try
            {
                /* 
                // Re-use active session if available
                if (invoice.ReferenceNumber != null && invoice.ReferenceNumber.StartsWith("SESSION:"))
                {
                    var existingSessionId = invoice.ReferenceNumber.Substring(8);
                    var existingSession = await _payMongoService.GetCheckoutSessionAsync(existingSessionId);
                    if (existingSession != null && existingSession.Status == "active")
                    {
                        return Redirect(existingSession.CheckoutUrl);
                    }
                }
                */

                var result = await _payMongoService.CreateCheckoutSessionAsync(
                    order.OrderId,
                    invoice.TotalPrice,
                    description,
                    successUrl,
                    cancelUrl);

                if (result == null)
                {
                    TempData["ErrorMessage"] = "Failed to create payment session. (Service returned null)";
                    return RedirectToPage(new { id = id });
                }

                if (!string.IsNullOrEmpty(result.Error))
                {
                    TempData["ErrorMessage"] = $"PayMongo API Error: {result.Error}";
                    return RedirectToPage(new { id = id });
                }

                // Store session ID in ReferenceNumber temporarily to retrieve details later
                invoice.ReferenceNumber = "SESSION:" + result.SessionId;
                await _context.SaveChangesAsync();

                return Redirect(result.CheckoutUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Internal error: {ex.Message}";
                return RedirectToPage(new { id = id });
            }
        }
        public async Task<IActionResult> OnPostVerifyAsync(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("/Login/login");

            var order = await _context.OrderInfos
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.BranchManagerId == branchManagerId && !o.IsArchived);

            if (order != null)
            {
                var invoice = order.Invoices.FirstOrDefault(i => i.PaymentStatus == "Pending" && i.ReferenceNumber != null && i.ReferenceNumber.StartsWith("SESSION:"));
                if (invoice != null)
                {
                    var sessionId = invoice.ReferenceNumber.Substring(8);
                    var status = await _payMongoService.GetCheckoutSessionStatusAsync(sessionId);
                    
                    if (status == "paid")
                    {
                        var details = await _payMongoService.GetPaymentDetailsAsync(sessionId);
                        invoice.PaymentStatus = "Paid";
                        invoice.PaymentDate = DateTime.Now;
                        
                        string method = details.Method;
                        if (!string.IsNullOrEmpty(method))
                        {
                            method = char.ToUpper(method[0]) + method.Substring(1).ToLower();
                            if (method.ToLower() == "paymaya") method = "Maya";
                        }
                        invoice.PaymentMethod = $"PayMongo ({method})";
                        invoice.ReferenceNumber = details.PaymentId;

                        if (order.Status == "Approved")
                        {
                            order.Status = "Preparing";
                            await LogSystemMessage(id, "Payment verified via PayMongo - Order status changed to Preparing");
                        }

                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Payment verified successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Payment status is still {status}. Please wait a moment.";
                    }
                }
            }
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostAddCommentAsync(int OrderId, string CommentText)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("/Login/login");

            // Verify order ownership
            var ownsOrder = await _context.OrderInfos.AnyAsync(o =>
                o.OrderId == OrderId &&
                o.BranchManagerId == branchManagerId &&
                !o.IsArchived);

            if (!ownsOrder)
                return Forbid();

            if (string.IsNullOrWhiteSpace(CommentText))
                return RedirectToPage(new { id = OrderId });

            var comment = new OrderComment
            {
                OrderId = OrderId,
                Comment = CommentText,
                BranchManagerId = branchManagerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.OrderComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = OrderId, tab = "chat" });
        }

        private async Task AggregateDeductions(int? skuId, int? comId, decimal quantity, Dictionary<int, decimal> deductions, Dictionary<int, decimal> stockSnapshots)
        {
            CommissaryInventory inv = null;
            SkuHeader sku = null;

            if (skuId.HasValue)
            {
                inv = await _context.CommissaryInventories
                    .Include(i => i.SkuHeader)
                        .ThenInclude(s => s.SkuRecipes)
                            .ThenInclude(r => r.CommissaryInventory)
                    .Include(i => i.SkuHeader)
                        .ThenInclude(s => s.SkuRecipes)
                            .ThenInclude(r => r.TargetSku)
                    .FirstOrDefaultAsync(i => i.SkuId == skuId);

                if (inv == null)
                {
                    sku = await _context.SkuHeaders
                        .Include(s => s.SkuRecipes)
                            .ThenInclude(r => r.CommissaryInventory)
                        .Include(s => s.SkuRecipes)
                            .ThenInclude(r => r.TargetSku)
                        .FirstOrDefaultAsync(s => s.SkuId == skuId);
                }
                else
                {
                    sku = inv.SkuHeader;
                }
            }
            else if (comId.HasValue)
            {
                inv = await _context.CommissaryInventories
                    .Include(i => i.IngredientRecipes)
                        .ThenInclude(r => r.Material)
                    .FirstOrDefaultAsync(i => i.ComId == comId);
            }

            if (inv != null)
            {
                if (!stockSnapshots.ContainsKey(inv.ComId))
                {
                    stockSnapshots[inv.ComId] = inv.Stock;
                }

                decimal snapshot = Math.Max(0, stockSnapshots[inv.ComId]);
                decimal available = Math.Min(quantity, snapshot);
                if (available > 0)
                {
                    stockSnapshots[inv.ComId] -= available;
                    deductions[inv.ComId] = deductions.GetValueOrDefault(inv.ComId) + available;
                }

                decimal remaining = quantity - available;
                if (remaining > 0)
                {
                    if (skuId.HasValue && sku?.SkuRecipes != null && sku.SkuRecipes.Any())
                    {
                        foreach (var recipe in sku.SkuRecipes)
                        {
                            string targetUom = recipe.ComId.HasValue ? recipe.CommissaryInventory?.Uom : recipe.TargetSku?.Uom;
                            if (string.IsNullOrEmpty(targetUom)) targetUom = recipe.Uom;

                            decimal componentQty = UomConverter.Convert(recipe.QuantityNeeded, recipe.Uom, targetUom);
                            await AggregateDeductions(recipe.TargetSkuId, recipe.ComId, componentQty * remaining, deductions, stockSnapshots);
                        }
                    }
                    else if (comId.HasValue && inv.IsRepack && inv.IngredientRecipes != null && inv.IngredientRecipes.Any())
                    {
                        foreach (var recipe in inv.IngredientRecipes)
                        {
                            if (recipe.MaterialId > 0 && recipe.Material != null)
                            {
                                decimal materialQty = UomConverter.Convert(recipe.QuantityNeeded, recipe.Uom, recipe.Material.Uom);
                                await AggregateDeductions(null, recipe.MaterialId, materialQty * remaining, deductions, stockSnapshots);
                            }
                        }
                    }
                    else
                    {
                        deductions[inv.ComId] = deductions.GetValueOrDefault(inv.ComId) + remaining;
                        stockSnapshots[inv.ComId] -= remaining;
                    }
                }
            }
            else if (skuId.HasValue && sku != null)
            {
                if (sku.SkuRecipes != null && sku.SkuRecipes.Any())
                {
                    foreach (var recipe in sku.SkuRecipes)
                    {
                        string targetUom = recipe.ComId.HasValue ? recipe.CommissaryInventory?.Uom : recipe.TargetSku?.Uom;
                        if (string.IsNullOrEmpty(targetUom)) targetUom = recipe.Uom;

                        decimal componentQty = UomConverter.Convert(recipe.QuantityNeeded, recipe.Uom, targetUom);
                        await AggregateDeductions(recipe.TargetSkuId, recipe.ComId, componentQty * quantity, deductions, stockSnapshots);
                    }
                }
                else
                {
                    deductions[-skuId.Value] = deductions.GetValueOrDefault(-skuId.Value) + quantity;
                }
            }
        }
    }
}

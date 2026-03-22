using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeKatsuMNL.Helpers;


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
        [TempData]
        public string ErrorMessage { get; set; }

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

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("/Dashboard/BranchOrders");
            }

            // Parse ID from string like "ORD-00001" or "2026-0001" to int
            int orderId = 0;
            if (id.Contains("-"))
            {
                // Handles both ORD-XXXXX and YYYY-XXXX
                int.TryParse(id.Split('-').Last(), out orderId);
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
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.CommissaryInventory)
                        .ThenInclude(ci => ci.IngredientRecipes)
                            .ThenInclude(r => r.Material)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.CommissaryInventory)
                        .ThenInclude(ci => ci.Category)
                .Include(o => o.OrderComments)
                    .ThenInclude(oc => oc.BranchManager)
                        .ThenInclude(bm => bm.BranchLocation)
                .Include(o => o.OrderComments)
                    .ThenInclude(oc => oc.AdminAccount)
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(m => m.OrderId == orderId && !m.IsArchived);

            if (Order == null)
            {
                return NotFound();
            }

            Comments = Order.OrderComments.OrderBy(c => c.CreatedAt).ToList();

            // Populate Stock Check List
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


        public async Task<IActionResult> OnPostAddCommentAsync(int OrderId, string CommentText)
        {
            if (string.IsNullOrWhiteSpace(CommentText))
            {
                var o = await _context.OrderInfos.FindAsync(OrderId);
                string fId = o != null ? $"{o.OrderDate.Year}-{o.OrderId:D4}" : OrderId.ToString();
                return RedirectToPage(new { id = fId, tab = "chat" });
            }

            // Get user info from Claims
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                var o = await _context.OrderInfos.FindAsync(OrderId);
                string fId = o != null ? $"{o.OrderDate.Year}-{o.OrderId:D4}" : OrderId.ToString();
                return RedirectToPage(new { id = fId, tab = "chat" });
            }

            // Security check: Only Admins or the owner of the order (BranchManager) can comment
            var order = await _context.OrderInfos.FindAsync(OrderId);
            if (order == null) return NotFound();

            if (userRole != "Admin")
            {
                if (userRole == "BranchManager" && order.BranchManagerId != userId)
                {
                    return Forbid();
                }
                else if (userRole != "BranchManager")
                {
                    return Forbid();
                }
            }

            var newComment = new OrderComment
            {
                OrderId = OrderId,
                Comment = CommentText,
                CreatedAt = DateTime.UtcNow
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

            var finalOrder = await _context.OrderInfos.FindAsync(OrderId);
            string finalFormattedId = finalOrder != null ? $"{finalOrder.OrderDate.Year}-{finalOrder.OrderId:D4}" : OrderId.ToString();
            return RedirectToPage(new { id = finalFormattedId, tab = "chat" });
        }

        private async Task LogSystemMessage(int orderId, string message)
        {
            var systemComment = new OrderComment
            {
                OrderId = orderId,
                Comment = "System: " + message,
                CreatedAt = DateTime.UtcNow
            };
            _context.OrderComments.Add(systemComment);
        }

        public async Task<IActionResult> OnPostRejectOrderAsync(int OrderId)
        {
            var order = await _context.OrderInfos.FirstOrDefaultAsync(o => o.OrderId == OrderId && !o.IsArchived);
            if (order == null) return NotFound();

            if (order.Status == "Pending")
            {
                order.Status = "Rejected";
                await LogSystemMessage(OrderId, "Order Rejected");
                await _context.SaveChangesAsync();
            }

            string formattedId = $"{order.OrderDate.Year}-{order.OrderId:D4}";
            return RedirectToPage(new { id = formattedId });
        }

        public async Task<IActionResult> OnPostApproveOrderAsync(int OrderId)
        {
            var order = await _context.OrderInfos
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                        .ThenInclude(s => s.SkuRecipes)
                            .ThenInclude(r => r.CommissaryInventory)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                        .ThenInclude(s => s.SkuRecipes)
                            .ThenInclude(r => r.TargetSku)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.CommissaryInventory)
                        .ThenInclude(ci => ci.IngredientRecipes)
                            .ThenInclude(r => r.Material)
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(m => m.OrderId == OrderId && !m.IsArchived);

            if (order == null) return NotFound();
            
            string formattedId = $"{order.OrderDate.Year}-{order.OrderId:D4}";
            if (order.Status != "Pending") return RedirectToPage(new { id = formattedId });

            // 1. Calculate deductions using Stock-Aware Logic (Stock First, then Recipe)
            var deductions = new Dictionary<int, decimal>();
            var stockSnapshots = new Dictionary<int, decimal>();
            foreach (var item in order.OrderLists)
            {
                await AggregateDeductions(item.SkuId, item.ComId, item.Quantity, deductions, stockSnapshots);
            }

            // Verify if any inventory goes negative (In cases where even recipes or raw materials are not enough)
            foreach (var det in deductions)
            {
                var inventory = await _context.CommissaryInventories.FindAsync(det.Key);
                if (inventory == null || inventory.Stock < det.Value)
                {
                    ErrorMessage = $"Insufficient stock for: {inventory?.ItemName ?? "Unknown Item"} (Required: {det.Value:0.##} {inventory?.Uom}, Available: {inventory?.Stock:0.##} {inventory?.Uom})";
                    return RedirectToPage(new { id = formattedId });
                }
            }

            // 2. Perform Deductions and log transactions
            var transactionType = await _context.InvTransactionTypes.FirstOrDefaultAsync(t => t.TransactionType == "Branch Order");
            if (transactionType == null)
            {
                transactionType = new InvTransactionType { TransactionType = "Branch Order" };
                _context.InvTransactionTypes.Add(transactionType);
                await _context.SaveChangesAsync();
            }

            foreach (var item in deductions)
            {
                var inventory = await _context.CommissaryInventories.FindAsync(item.Key);
                if (inventory != null)
                {
                    decimal oldStock = inventory.Stock;
                    inventory.Stock -= item.Value;

                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ComId = inventory.ComId,
                        TypeId = transactionType.TypeId,
                        QuantityChange = -item.Value,
                        TimeStamp = DateTime.UtcNow,
                        Remarks = $"Branch Order #{formattedId} deduction"
                    });
                }
            }

            // 3. Create Invoice
            var invoice = new Invoice
            {
                OrderId = order.OrderId,
                InvoiceDate = DateTime.UtcNow,
                TotalPrice = order.OrderLists.Sum(ol => ol.TotalPrice),
                PaymentStatus = "Pending",
                PaymentMethod = "TBD",
                VerifiedBy = "System"
            };
            _context.Invoices.Add(invoice);

            // 4. Update order status
            order.Status = "Approved";
            await LogSystemMessage(OrderId, "Order Approved - Invoice Created");
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = formattedId });
        }


        public async Task<IActionResult> OnPostPrepareAsync(int OrderId)
        {
            var order = await _context.OrderInfos
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(o => o.OrderId == OrderId && !o.IsArchived);
            
            if (order == null) return NotFound();
            
            if (order.Status == "Approved")
            {
                var invoice = order.Invoices.FirstOrDefault();
                if (invoice == null || !string.Equals(invoice.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = "Cannot start preparing: Invoice is not yet paid.";
                    string fId = $"{order.OrderDate.Year}-{order.OrderId:D4}";
                    return RedirectToPage(new { id = fId });
                }
                
                order.Status = "Preparing";
                await LogSystemMessage(OrderId, "Order status changed to Preparing");
                await _context.SaveChangesAsync();
            }

            string prepFormattedId = $"{order.OrderDate.Year}-{order.OrderId:D4}";
            return RedirectToPage(new { id = prepFormattedId });
        }

        public async Task<IActionResult> OnPostDeliverAsync(int OrderId)
        {
            var order = await _context.OrderInfos.FirstOrDefaultAsync(o => o.OrderId == OrderId && !o.IsArchived);
            if (order == null) return NotFound();

            if (order.Status == "Preparing")
            {
                order.Status = "Delivering";
                await LogSystemMessage(OrderId, "Order status changed to In Transit");
                await _context.SaveChangesAsync();
            }

            string deliverFormattedId = $"{order.OrderDate.Year}-{order.OrderId:D4}";
            return RedirectToPage(new { id = deliverFormattedId });
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
                        // No stock and no recipe/not repack - take from this item
                        deductions[inv.ComId] = deductions.GetValueOrDefault(inv.ComId) + remaining;
                        stockSnapshots[inv.ComId] -= remaining;
                    }
                }
            }
            else if (skuId.HasValue && sku != null)
            {
                // SKU exists but no inventory row - check recipes
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
                    // No inventory AND no recipe - this is a critical failure point.
                    // We must record this as a requirement for some ID, but since there's no inventory, 
                    // we'll use a special negative ID or just record it as is to trigger the "Insufficient stock" check later.
                    // For now, we'll assign it to -1 * skuId to indicate a missing inventory mapping.
                    deductions[-skuId.Value] = deductions.GetValueOrDefault(-skuId.Value) + quantity;
                }
            }
        }

    }
}

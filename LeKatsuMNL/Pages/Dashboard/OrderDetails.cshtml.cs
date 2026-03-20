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

            Comments = Order.OrderComments.OrderByDescending(c => c.CommentId).ToList();

            // Populate Stock Check List
            var requirements = new Dictionary<int, decimal>();
            foreach (var item in Order.OrderLists)
            {
                if (item.SkuId.HasValue && item.SkuHeader != null)
                {
                    await AggregateStockRequirements(item.SkuHeader.SkuId, item.Quantity, requirements);
                }
                else if (item.ComId.HasValue)
                {
                    await AggregateIngredientRequirements(item.ComId.Value, item.Quantity, requirements);
                }
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
                return RedirectToPage(new { id = fId, tab = "issues" });
            }

            // Get user info from Claims
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                var o = await _context.OrderInfos.FindAsync(OrderId);
                string fId = o != null ? $"{o.OrderDate.Year}-{o.OrderId:D4}" : OrderId.ToString();
                return RedirectToPage(new { id = fId, tab = "issues" });
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

            var finalOrder = await _context.OrderInfos.FindAsync(OrderId);
            string finalFormattedId = finalOrder != null ? $"{finalOrder.OrderDate.Year}-{finalOrder.OrderId:D4}" : OrderId.ToString();
            return RedirectToPage(new { id = finalFormattedId, tab = "issues" });
        }

        public async Task<IActionResult> OnPostRejectOrderAsync(int OrderId)
        {
            var order = await _context.OrderInfos.FirstOrDefaultAsync(o => o.OrderId == OrderId && !o.IsArchived);
            if (order == null) return NotFound();

            if (order.Status == "Pending")
            {
                order.Status = "Rejected";
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
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.CommissaryInventory)
                        .ThenInclude(ci => ci.IngredientRecipes)
                            .ThenInclude(r => r.Material)
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(m => m.OrderId == OrderId && !m.IsArchived);

            if (order == null) return NotFound();
            
            string formattedId = $"{order.OrderDate.Year}-{order.OrderId:D4}";
            if (order.Status != "Pending") return RedirectToPage(new { id = formattedId });

            // 1. Check stock availability recursively across all items in the order
            var stockRequirements = new Dictionary<int, decimal>();
            foreach (var item in order.OrderLists)
            {
                if (item.SkuId.HasValue && item.SkuHeader != null)
                {
                    await AggregateStockRequirements(item.SkuHeader.SkuId, item.Quantity, stockRequirements);
                }
                else if (item.ComId.HasValue)
                {
                    await AggregateIngredientRequirements(item.ComId.Value, item.Quantity, stockRequirements);
                }
            }

            // Verify aggregated requirements against current inventory
            foreach (var req in stockRequirements)
            {
                var inventory = await _context.CommissaryInventories.FindAsync(req.Key);
                if (inventory == null || inventory.Stock < req.Value)
                {
                    ErrorMessage = $"Insufficient stock for: {inventory?.ItemName ?? "Unknown Item"} (Required: {req.Value:0.##} {inventory?.Uom}, Available: {inventory?.Stock:0.##} {inventory?.Uom})";
                    string fId = $"{order.OrderDate.Year}-{order.OrderId:D4}";
                    return RedirectToPage(new { id = fId });
                }
            }

            // 2. If stock is sufficient, deduct from inventory and log transactions
            var transactionType = await _context.InvTransactionTypes.FirstOrDefaultAsync(t => t.TransactionType == "Branch Order");
            if (transactionType == null)
            {
                transactionType = new InvTransactionType { TransactionType = "Branch Order" };
                _context.InvTransactionTypes.Add(transactionType);
                await _context.SaveChangesAsync();
            }

            foreach (var req in stockRequirements)
            {
                var inventory = await _context.CommissaryInventories.FindAsync(req.Key);
                if (inventory != null)
                {
                    inventory.Stock -= req.Value;

                    // Log the transaction
                    var transaction = new InventoryTransaction
                    {
                        ComId = req.Key,
                        TypeId = transactionType.TypeId,
                        QuantityChange = -req.Value,
                        TimeStamp = System.DateTime.Now
                    };
                    _context.InventoryTransactions.Add(transaction);
                }
            }

            // 2.5 Deduct the SKU itself if it exists in CommissaryInventory (for SKU Report tracking)
            foreach (var item in order.OrderLists)
            {
                if (item.SkuId.HasValue && item.SkuHeader != null)
                {
                    var skuInventory = await _context.CommissaryInventories
                        .FirstOrDefaultAsync(i => i.SkuId == item.SkuHeader.SkuId);
                    
                    if (skuInventory == null)
                    {
                        var firstVendor = await _context.VendorInfos.OrderBy(v => v.VendorId).FirstOrDefaultAsync();
                        int defaultVendorId = firstVendor?.VendorId ?? 0;

                        // Create inventory record if missing
                        skuInventory = new CommissaryInventory
                        {
                            SkuId = item.SkuHeader.SkuId,
                            ItemName = item.SkuHeader.ItemName,
                            Stock = 0,
                            Uom = item.SkuHeader.Uom,
                            CostPrice = item.SkuHeader.UnitCost ?? 0,
                            ReorderValue = 0,
                            CategoryId = item.SkuHeader.CategoryId,
                            VendorId = defaultVendorId,
                            Yield = "100%"
                        };
                        _context.CommissaryInventories.Add(skuInventory);
                        await _context.SaveChangesAsync(); // Save to get ComId
                    }

                    skuInventory.Stock -= item.Quantity;
                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ComId = skuInventory.ComId,
                        TypeId = transactionType.TypeId,
                        QuantityChange = -item.Quantity,
                        TimeStamp = System.DateTime.Now
                    });
                }
                else if (item.ComId.HasValue)
                {
                    // Ingredient stock deduction is already handled in the stockRequirements loop above
                    // This section is for SKU-specific inventory tracking if needed.
                }
            }

            // 3. Create Invoice
            var invoice = new Invoice
            {
                OrderId = order.OrderId,
                InvoiceDate = System.DateTime.Now,
                TotalPrice = order.OrderLists.Sum(ol => ol.TotalPrice),
                PaymentStatus = "Pending",
                PaymentMethod = "TBD",
                VerifiedBy = "System"
            };
            _context.Invoices.Add(invoice);

            // 4. Update status
            order.Status = "Approved";
            await _context.SaveChangesAsync();

            string approvedFormattedId = $"{order.OrderDate.Year}-{order.OrderId:D4}";
            return RedirectToPage(new { id = approvedFormattedId });
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
                await _context.SaveChangesAsync();
            }

            string deliverFormattedId = $"{order.OrderDate.Year}-{order.OrderId:D4}";
            return RedirectToPage(new { id = deliverFormattedId });
        }

        private async Task AggregateStockRequirements(int skuId, decimal multiplier, Dictionary<int, decimal> requirements, HashSet<int> visitedSkuIds = null)
        {
            if (visitedSkuIds == null) visitedSkuIds = new HashSet<int>();
            if (visitedSkuIds.Contains(skuId)) return;

            visitedSkuIds.Add(skuId);

            var sku = await _context.SkuHeaders
                .Include(s => s.SkuRecipes)
                    .ThenInclude(r => r.CommissaryInventory)
                .Include(s => s.SkuRecipes)
                    .ThenInclude(r => r.TargetSku)
                .FirstOrDefaultAsync(s => s.SkuId == skuId);

            if (sku == null || sku.SkuRecipes == null) return;

            foreach (var recipe in sku.SkuRecipes)
            {
                if (recipe.ComId.HasValue && recipe.CommissaryInventory != null)
                {
                    decimal convertedQty = Helpers.UomConverter.Convert(
                        recipe.QuantityNeeded, recipe.Uom, recipe.CommissaryInventory.Uom);
                    decimal totalNeeded = convertedQty * multiplier;

                    if (requirements.ContainsKey(recipe.ComId.Value))
                        requirements[recipe.ComId.Value] += totalNeeded;
                    else
                        requirements[recipe.ComId.Value] = totalNeeded;
                }
                else if (recipe.TargetSkuId.HasValue)
                {
                    await AggregateStockRequirements(recipe.TargetSkuId.Value, multiplier * recipe.QuantityNeeded, requirements, new HashSet<int>(visitedSkuIds));
                }
            }
        }

        private async Task AggregateIngredientRequirements(int comId, decimal quantity, Dictionary<int, decimal> requirements, HashSet<int> visitedComIds = null)
        {
            if (visitedComIds == null) visitedComIds = new HashSet<int>();
            if (visitedComIds.Contains(comId)) return;
            visitedComIds.Add(comId);

            var inventory = await _context.CommissaryInventories
                .Include(ci => ci.IngredientRecipes)
                    .ThenInclude(r => r.Material)
                .FirstOrDefaultAsync(ci => ci.ComId == comId);

            if (inventory == null) return;

            // Add the parent ingredient itself
            if (requirements.ContainsKey(comId))
                requirements[comId] += quantity;
            else
                requirements[comId] = quantity;

            // Add materials if it's a repack
            if (inventory.IsRepack && inventory.IngredientRecipes != null)
            {
                foreach (var recipe in inventory.IngredientRecipes)
                {
                    if (recipe.Material == null) continue;

                    decimal convertedQty = Helpers.UomConverter.Convert(
                        recipe.QuantityNeeded, recipe.Uom, recipe.Material.Uom);
                    decimal materialQty = convertedQty * quantity;
                    
                    await AggregateIngredientRequirements(recipe.MaterialId, materialQty, requirements, new HashSet<int>(visitedComIds));
                }
            }
        }
    }
}

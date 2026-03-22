using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using LeKatsuMNL.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class BranchOrdersModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public BranchOrdersModel(LeKatsuDb context)
        {
            _context = context;
        }

        public PaginatedList<OrderInfo> Orders { get; set; } = default!;
        public List<BranchManager> BranchManagers { get; set; } = new();

        [TempData]
        public string SuccessMessage { get; set; }
        
        [TempData]
        public string ErrorMessage { get; set; }
        
        public class AvailableItem
        {
            public int? SkuId { get; set; }
            public int? ComId { get; set; }
            public string ItemName { get; set; }
            public decimal SellingPrice { get; set; }
            public string Type { get; set; } // "SKU" or "Ingredient"
            public decimal Stock { get; set; }
            public string Uom { get; set; }
        }

        public List<AvailableItem> AvailableItems { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public async Task<IActionResult> OnGetAsync(int? pageIndex)
        {
            var query = _context.OrderInfos
                .Where(o => !o.IsArchived)
                .Include(o => o.BranchManager)
                    .ThenInclude(bm => bm.BranchLocation)
                .Include(o => o.Invoices)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var search = SearchTerm.ToLower();
                
                // If search contains hyphen, try to parse the part after hyphen as ID
                int? parsedId = null;
                if (search.Contains("-"))
                {
                    var parts = search.Split('-');
                    if (int.TryParse(parts.Last(), out int id))
                    {
                        parsedId = id;
                    }
                }
                else if (int.TryParse(search, out int id))
                {
                    parsedId = id;
                }

                query = query.Where(o =>
                    o.BranchManager.BranchLocation.BranchName.ToLower().Contains(search) ||
                    o.Status.ToLower().Contains(search) ||
                    (parsedId.HasValue && o.OrderId == parsedId.Value));
            }

            query = query.OrderByDescending(o => o.OrderDate);
            
            int pageSize = PageSize > 0 ? PageSize : 10;
            Orders = await PaginatedList<OrderInfo>.CreateAsync(query, pageIndex ?? 1, pageSize);
            
            BranchManagers = await _context.BranchManagers
                .Include(bm => bm.BranchLocation)
                .Where(bm => bm.Status == "Active" && !bm.IsArchived)
                .ToListAsync();

            var skus = await _context.SkuHeaders
                .Where(s => !s.IsArchived)
                .Include(s => s.CommissaryInventory)
                .Select(s => new AvailableItem
                {
                    SkuId = s.SkuId,
                    ItemName = s.ItemName,
                    SellingPrice = s.SellingPrice,
                    Type = "SKU",
                    Stock = s.CommissaryInventory != null ? s.CommissaryInventory.Stock : 0,
                    Uom = s.Uom
                })
                .ToListAsync();


            var ingredients = await _context.CommissaryInventories
                .Where(i => i.SellingPrice > 0 && !i.IsArchived)
                .Select(i => new AvailableItem
                {
                    ComId = i.ComId,
                    ItemName = i.ItemName,
                    SellingPrice = i.SellingPrice,
                    Type = "Ingredient",
                    Stock = i.Stock,
                    Uom = i.Uom
                })
                .ToListAsync();


            AvailableItems = skus.Concat(ingredients).OrderBy(i => i.ItemName).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateOrderAsync(int BranchManagerId, List<string> ItemIds, List<decimal> Quantities, List<decimal> Prices)
        {
            if (BranchManagerId <= 0 || ItemIds == null || !ItemIds.Any())
            {
                return RedirectToPage();
            }

            DateTime now = DateTime.Now;
            DateTime expectedDelivery = now.Hour >= 17 ? now.Date.AddDays(3) : now.Date.AddDays(2);

            var order = new OrderInfo
            {
                BranchManagerId = BranchManagerId,
                OrderDate = now,
                DeliveryDate = expectedDelivery,
                Status = "Pending"
            };

            _context.OrderInfos.Add(order);
            await _context.SaveChangesAsync();

            for (int i = 0; i < ItemIds.Count; i++)
            {
                if (Quantities[i] <= 0) continue;

                var idParts = ItemIds[i].Split('-');
                if (idParts.Length != 2) continue;

                var type = idParts[0];
                int id = int.Parse(idParts[1]);

                var orderList = new OrderList
                {
                    OrderId = order.OrderId,
                    Quantity = Quantities[i],
                    TotalPrice = Prices[i] * Quantities[i]
                };

                if (type == "SKU")
                {
                    orderList.SkuId = id;
                }
                else if (type == "COM")
                {
                    orderList.ComId = id;
                }

                _context.OrderLists.Add(orderList);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostArchiveAsync(int id)
        {
            var order = await _context.OrderInfos.FindAsync(id);
            if (order != null)
            {
                order.IsArchived = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBulkArchiveAsync(string ids)
        {
            if (string.IsNullOrEmpty(ids)) return RedirectToPage();

            var idList = new List<int>();
            foreach (var token in ids.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(token.Trim(), out var parsedId))
                {
                    idList.Add(parsedId);
                }
            }

            if (idList.Count == 0) return RedirectToPage();

            var orders = await _context.OrderInfos.Where(o => idList.Contains(o.OrderId)).ToListAsync();
            foreach (var order in orders)
            {
                order.IsArchived = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBulkUpdateStatusAsync(string ids)
        {
            if (string.IsNullOrEmpty(ids)) return RedirectToPage();

            var idList = new List<int>();
            foreach (var token in ids.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(token.Trim(), out var parsedId))
                {
                    idList.Add(parsedId);
                }
            }

            if (idList.Count == 0) return RedirectToPage();

            var orders = await _context.OrderInfos
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                        .ThenInclude(s => s.SkuRecipes)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.CommissaryInventory)
                        .ThenInclude(ci => ci.IngredientRecipes)
                            .ThenInclude(r => r.Material)
                .Include(o => o.Invoices)
                .Where(o => idList.Contains(o.OrderId) && !o.IsArchived)
                .ToListAsync();

            int successCount = 0;
            var errorMessages = new List<string>();

            foreach(var order in orders)
            {
                string fId = $"{order.OrderDate.Year}-{order.OrderId:D4}";

                if (order.Status == "Pending")
                {
                    var (success, error) = await ProcessOrderApprovalAsync(order);
                    if (success) successCount++;
                    else errorMessages.Add($"Order {fId}: {error}");
                }
                else if (order.Status == "Approved")
                {
                    var invoice = order.Invoices.FirstOrDefault();
                    if (invoice == null || invoice.PaymentStatus != "Paid")
                    {
                        errorMessages.Add($"Order {fId}: Invoice missing or not yet paid.");
                        continue;
                    }
                    order.Status = "Preparing";
                    successCount++;
                }
                else if (order.Status == "Preparing")
                {
                    order.Status = "Delivering";
                    successCount++;
                }
            }

            if (successCount > 0 || errorMessages.Any())
            {
                await _context.SaveChangesAsync();
            }
            
            if (successCount > 0)
                SuccessMessage = $"Successfully updated statuses for {successCount} order(s).";
            
            if (errorMessages.Any())
                ErrorMessage = string.Join(" ", errorMessages);

            return RedirectToPage();
        }

        private async Task<(bool success, string error)> ProcessOrderApprovalAsync(OrderInfo order)
        {
            var finalDeductions = new Dictionary<int, decimal>();
            var stockSnapshots = new Dictionary<int, decimal>();

            foreach (var item in order.OrderLists)
            {
                await AggregateDeductions(item.SkuId, item.ComId, item.Quantity, finalDeductions, stockSnapshots);
            }

            // Verify availability
            foreach (var req in finalDeductions)
            {
                var inventory = await _context.CommissaryInventories.FindAsync(req.Key);
                if (inventory == null || inventory.Stock < req.Value)
                {
                    return (false, $"Insufficient stock for {inventory?.ItemName ?? "Unknown Item"}. Needed: {req.Value:G29}, Available: {inventory?.Stock:G29}");
                }
            }

            var transactionType = await _context.InvTransactionTypes.FirstOrDefaultAsync(t => t.TransactionType == "Branch Order");
            if (transactionType == null)
            {
                transactionType = new InvTransactionType { TransactionType = "Branch Order" };
                _context.InvTransactionTypes.Add(transactionType);
                await _context.SaveChangesAsync();
            }

            // Perform deductions
            foreach (var req in finalDeductions)
            {
                var inventory = await _context.CommissaryInventories.FindAsync(req.Key);
                if (inventory != null)
                {
                    inventory.Stock -= req.Value;
                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ComId = req.Key,
                        TypeId = transactionType.TypeId,
                        QuantityChange = -req.Value,
                        TimeStamp = System.DateTime.Now,
                        Uom = inventory.Uom,
                        Remarks = $"Deduction for Order #{order.OrderId}"
                    });
                }
            }


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

            order.Status = "Approved";

            return (true, string.Empty);
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
            }
        }



        public async Task<IActionResult> OnGetExportAsync()
        {
            if (!PermissionHelper.HasPermission(User, "Transactions", 'R')) return Forbid();

            var orders = await _context.OrderInfos
                .Where(o => !o.IsArchived)
                .Include(o => o.BranchManager)
                    .ThenInclude(bm => bm.BranchLocation)
                .Include(o => o.Invoices)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Order ID,Branch,Branch Manager,Invoice Status,Status,Order Date,Expected Date");

            foreach (var order in orders)
            {
                string formattedOrderId = $"{order.OrderDate.Year}-{order.OrderId:D4}";
                string branch = order.BranchManager?.BranchLocation?.BranchName ?? "N/A";
                string manager = $"{order.BranchManager?.FirstName} {order.BranchManager?.LastName}";
                string invStatus = order.Invoices?.Any() == true ? order.Invoices.First().PaymentStatus : "No Invoice";
                string status = order.Status ?? "Unknown";
                string orderDate = order.OrderDate.ToString("yyyy-MM-dd");
                string expectedDate = order.DeliveryDate?.ToString("yyyy-MM-dd") ?? "";

                csv.AppendLine(string.Join(",",
                    EscapeCsv(formattedOrderId),
                    EscapeCsv(branch),
                    EscapeCsv(manager),
                    EscapeCsv(invStatus),
                    EscapeCsv(status),
                    EscapeCsv(orderDate),
                    EscapeCsv(expectedDate)));
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"BranchOrders_{System.DateTime.Now:yyyyMMdd}.csv");
        }

        private static string EscapeCsv(string? value)
        {
            var sanitized = value ?? string.Empty;
            if (sanitized.Length > 0 && "=+-@".Contains(sanitized[0]))
            {
                sanitized = "'" + sanitized;
            }
            return $"\"{sanitized.Replace("\"", "\"\"")}\"";
        }
    }
}

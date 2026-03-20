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
                .Select(s => new AvailableItem
                {
                    SkuId = s.SkuId,
                    ItemName = s.ItemName,
                    SellingPrice = s.SellingPrice,
                    Type = "SKU"
                })
                .ToListAsync();

            var ingredients = await _context.CommissaryInventories
                .Where(i => i.SellingPrice > 0 && !i.IsArchived)
                .Select(i => new AvailableItem
                {
                    ComId = i.ComId,
                    ItemName = i.ItemName,
                    SellingPrice = i.SellingPrice,
                    Type = "Ingredient"
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
                    if (invoice != null && invoice.PaymentStatus != "Paid")
                    {
                        errorMessages.Add($"Order {fId}: Invoice not yet paid.");
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

            foreach (var req in stockRequirements)
            {
                var inventory = await _context.CommissaryInventories.FindAsync(req.Key);
                if (inventory == null || inventory.Stock < req.Value)
                {
                    return (false, $"Insufficient stock for {inventory?.ItemName ?? "Unknown Item"}.");
                }
            }

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
                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ComId = req.Key,
                        TypeId = transactionType.TypeId,
                        QuantityChange = -req.Value,
                        TimeStamp = System.DateTime.Now
                    });
                }
            }

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
                        await _context.SaveChangesAsync(); 
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

            if (requirements.ContainsKey(comId))
                requirements[comId] += quantity;
            else
                requirements[comId] = quantity;

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

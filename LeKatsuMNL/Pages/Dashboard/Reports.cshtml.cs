using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using LeKatsuMNL.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class ReportsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public ReportsModel(LeKatsuDb context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string ReportType { get; set; } = "Inventory";

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        // Ingredient Report
        public class InventoryReportRow
        {
            public string ItemName { get; set; }
            public decimal Beginning { get; set; }
            public decimal Ending { get; set; }
            public decimal Received { get; set; }
            public decimal Consumed { get; set; }
            public decimal Rejected { get; set; }
            public string Uom { get; set; }
        }

        public PaginatedList<InventoryReportRow> IngredientReports { get; set; }

        // SKU Inventory Report
        public class SkuInventoryReportRow
        {
            public string ItemName { get; set; }
            public decimal Beginning { get; set; }
            public decimal Ending { get; set; }
            public decimal Received { get; set; }
            public decimal Consumed { get; set; }
            public decimal Rejected { get; set; }
            public string Uom { get; set; }
        }

        public PaginatedList<SkuInventoryReportRow> SkuInventoryReports { get; set; }

        // Sales Report (Branch Order Report)
        public class SalesReportRow
        {
            public int OrderId { get; set; }
            public string FormattedOrderId => $"{Date.Year}-{OrderId:D4}";
            public string BranchName { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal MoneyPaid { get; set; }
            public DateTime Date { get; set; }
        }

        public PaginatedList<SalesReportRow> SalesReports { get; set; }

        // Reject Report
        public class RejectReportRow
        {
            public string ItemName { get; set; }
            public decimal Quantity { get; set; }
            public string Reason { get; set; }
            public string Type { get; set; }
            public DateTime Date { get; set; }
        }

        public PaginatedList<RejectReportRow> RejectReports { get; set; }

        // Vendor Summary Report
        public class VendorSummaryReportRow
        {
            public string VendorName { get; set; }
            public int TotalOrders { get; set; }
            public decimal TotalItems { get; set; }
            public decimal TotalSpent { get; set; }
        }

        public PaginatedList<VendorSummaryReportRow> VendorSummaryReports { get; set; }

        // Branch Summary Report
        public class BranchSummaryReportRow
        {
            public string BranchName { get; set; }
            public int TotalOrders { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal TotalCollected { get; set; }
        }

        public PaginatedList<BranchSummaryReportRow> BranchSummaryReports { get; set; }
        
        // Ingredient Finance Report
        public class IngredientFinanceReportRow
        {
            public string ItemName { get; set; }
            public string Uom { get; set; }
            public decimal CostPrice { get; set; }
            public decimal SellingPrice { get; set; }
            public decimal Markup => SellingPrice - CostPrice;
            public decimal MarginPercentage => SellingPrice != 0 ? (Markup / SellingPrice) * 100 : 0;
        }
        public PaginatedList<IngredientFinanceReportRow> IngredientFinanceReports { get; set; }

        // SKU Finance Report
        public class SkuFinanceReportRow
        {
            public string ItemName { get; set; }
            public string Uom { get; set; }
            public decimal CostPrice { get; set; }
            public decimal SellingPrice { get; set; }
            public decimal Markup => SellingPrice - CostPrice;
            public decimal MarginPercentage => SellingPrice != 0 ? (Markup / SellingPrice) * 100 : 0;
        }
        public PaginatedList<SkuFinanceReportRow> SkuFinanceReports { get; set; }

        public async Task OnGetAsync(int? pageIndex)
        {
            // Set default dates if not provided (default to current month)
            StartDate ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate ??= DateTime.Now;

            // Ensure StartDate is at the beginning of the day and EndDate is at the end of the day
            // This ensures inclusive filtering regardless of literal time values
            if (StartDate.HasValue) StartDate = StartDate.Value.Date;
            if (EndDate.HasValue) EndDate = EndDate.Value.Date.AddDays(1).AddTicks(-1);

            int pageSize = PageSize > 0 ? PageSize : 10;

            switch (ReportType)
            {
                case "Inventory":
                    await LoadIngredientReport(pageIndex ?? 1, pageSize);
                    break;
                case "SkuInventory":
                    await LoadSkuInventoryReport(pageIndex ?? 1, pageSize);
                    break;
                case "Sales":
                    await LoadSalesReport(pageIndex ?? 1, pageSize);
                    break;
                case "Rejects":
                    await LoadRejectReport(pageIndex ?? 1, pageSize);
                    break;
                case "VendorSummary":
                    await LoadVendorSummaryReport(pageIndex ?? 1, pageSize);
                    break;
                case "BranchSummary":
                    await LoadBranchSummaryReport(pageIndex ?? 1, pageSize);
                    break;
                case "IngredientFinance":
                    await LoadIngredientFinanceReport(pageIndex ?? 1, pageSize);
                    break;
                case "SkuFinance":
                    await LoadSkuFinanceReport(pageIndex ?? 1, pageSize);
                    break;
            }
        }

        private async Task LoadIngredientReport(int pageIndex, int pageSize)
        {
            await EnsureTransactionsLogged();

            var items = await _context.CommissaryInventories
                .Include(i => i.InventoryTransactions)
                    .ThenInclude(t => t.InvTransactionType)
                .Where(i => i.SkuId == null)
                .AsNoTracking()
                .ToListAsync();

            var reportData = items.Select(item =>
            {
                var transactionsInPeriod = item.InventoryTransactions
                    .Where(t => t.TimeStamp >= StartDate && t.TimeStamp <= EndDate)
                    .ToList();

                var transactionsAfterPeriod = item.InventoryTransactions
                    .Where(t => t.TimeStamp > EndDate)
                    .Sum(t => t.QuantityChange);

                // Current stock is after all transactions. 
                // Stock at EndDate = CurrentStock - Sum(Transactions after EndDate)
                decimal stockAtEnd = item.Stock - transactionsAfterPeriod;

                decimal received = transactionsInPeriod
                    .Where(t => t.QuantityChange > 0 && t.InvTransactionType?.TransactionType == "Stock In")
                    .Sum(t => t.QuantityChange);

                decimal consumed = Math.Abs(transactionsInPeriod
                    .Where(t => t.QuantityChange < 0 && t.InvTransactionType?.TransactionType == "Branch Order")
                    .Sum(t => t.QuantityChange));

                // For simplicity, let's assume "Beginning" is stockAtEnd minus net change in period
                decimal netChange = transactionsInPeriod.Sum(t => t.QuantityChange);
                decimal stockAtBeginning = stockAtEnd - netChange;

                return new InventoryReportRow
                {
                    ItemName = item.ItemName,
                    Beginning = stockAtBeginning,
                    Ending = stockAtEnd,
                    Received = received,
                    Consumed = consumed,
                    Rejected = Math.Abs(transactionsInPeriod
                        .Where(t => t.InvTransactionType?.TransactionType == "Rejected")
                        .Sum(t => t.QuantityChange)),
                    Uom = UomConverter.NormalizeUnit(item.Uom)
                };
            })
            .Where(r => string.IsNullOrEmpty(SearchQuery) || r.ItemName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();

            IngredientReports = PaginatedList<InventoryReportRow>.Create(reportData, pageIndex, pageSize);
        }

        private async Task LoadSkuInventoryReport(int pageIndex, int pageSize)
        {
            await EnsureTransactionsLogged();

            var skus = await _context.SkuHeaders
                .AsNoTracking()
                .ToListAsync();

            var inventoryItems = await _context.CommissaryInventories
                .Include(i => i.InventoryTransactions)
                    .ThenInclude(t => t.InvTransactionType)
                .Where(i => i.SkuId != null)
                .AsNoTracking()
                .ToListAsync();

            var reportData = skus.Select(sku =>
            {
                var item = inventoryItems.FirstOrDefault(i => i.SkuId == sku.SkuId);
                
                if (item == null)
                {
                    return new SkuInventoryReportRow
                    {
                        ItemName = sku.ItemName,
                        Beginning = 0,
                        Ending = 0,
                        Received = 0,
                        Consumed = 0,
                        Rejected = 0,
                        Uom = sku.Uom
                    };
                }

                var transactionsInPeriod = item.InventoryTransactions
                    .Where(t => t.TimeStamp >= StartDate && t.TimeStamp <= EndDate)
                    .ToList();

                var transactionsAfterPeriod = item.InventoryTransactions
                    .Where(t => t.TimeStamp > EndDate)
                    .Sum(t => t.QuantityChange);

                decimal stockAtEnd = item.Stock - transactionsAfterPeriod;
                decimal received = transactionsInPeriod.Where(t => t.QuantityChange > 0).Sum(t => t.QuantityChange);
                decimal consumed = Math.Abs(transactionsInPeriod.Where(t => t.QuantityChange < 0 && t.InvTransactionType?.TransactionType == "Branch Order").Sum(t => t.QuantityChange));
                decimal rejected = Math.Abs(transactionsInPeriod.Where(t => t.InvTransactionType?.TransactionType == "Rejected").Sum(t => t.QuantityChange));
                
                decimal netChange = transactionsInPeriod.Sum(t => t.QuantityChange);
                decimal stockAtBeginning = stockAtEnd - netChange;

                return new SkuInventoryReportRow
                {
                    ItemName = item.ItemName,
                    Beginning = stockAtBeginning,
                    Ending = stockAtEnd,
                    Received = received,
                    Consumed = consumed,
                    Rejected = rejected,
                    Uom = UomConverter.NormalizeUnit(item.Uom)
                };
            })
            .Where(r => string.IsNullOrEmpty(SearchQuery) || r.ItemName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.ItemName)
            .ToList();

            SkuInventoryReports = PaginatedList<SkuInventoryReportRow>.Create(reportData, pageIndex, pageSize);
        }

        private async Task EnsureTransactionsLogged()
        {
            // One-time fix for existing approved orders that are missing transaction logs
            var approvedOrders = await _context.OrderInfos
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                        .ThenInclude(s => s.SkuRecipes)
                .Where(o => (o.Status == "Approved" || o.Status == "Preparing" || o.Status == "Delivered"))
                .ToListAsync();

            var transactionType = await _context.InvTransactionTypes.FirstOrDefaultAsync(t => t.TransactionType == "Branch Order");
            if (transactionType == null) return;

            bool changes = false;
            foreach (var order in approvedOrders)
            {
                if (order.OrderDate.Date == DateTime.Now.Date || order.OrderId == 12)
                {
                    foreach (var ol in order.OrderLists)
                    {
                        if (ol.SkuId.HasValue && ol.SkuHeader != null)
                        {
                            // 2. Add: Check if the SKU itself needs a transaction logged for the SKU Report
                            var skuInventory = await _context.CommissaryInventories
                                .FirstOrDefaultAsync(i => i.SkuId == ol.SkuHeader.SkuId);
                            
                            if (skuInventory == null)
                            {
                                var firstVendor = await _context.VendorInfos.OrderBy(v => v.VendorId).FirstOrDefaultAsync();
                                int defaultVendorId = firstVendor?.VendorId ?? 0;

                                // Create inventory record if missing for backfill
                                skuInventory = new CommissaryInventory
                                {
                                    SkuId = ol.SkuHeader.SkuId,
                                    ItemName = ol.SkuHeader.ItemName,
                                    Stock = 0, // We'll let the transaction handle the balance
                                    Uom = ol.SkuHeader.Uom,
                                    CostPrice = ol.SkuHeader.UnitCost ?? 0,
                                    ReorderValue = 0,
                                    CategoryId = ol.SkuHeader.CategoryId,
                                    VendorId = defaultVendorId,
                                    Yield = "100%"
                                };
                                _context.CommissaryInventories.Add(skuInventory);
                                await _context.SaveChangesAsync();
                            }

                            if (skuInventory != null)
                            {
                                var existingSkuTransactions = await _context.InventoryTransactions
                                    .Where(t => t.ComId == skuInventory.ComId && t.TypeId == transactionType.TypeId)
                                    .ToListAsync();

                                bool skuExists = existingSkuTransactions.Any(t =>
                                    Math.Abs(t.QuantityChange) == ol.Quantity &&
                                    Math.Abs((t.TimeStamp - order.OrderDate).TotalMinutes) < 10);

                                if (!skuExists)
                                {
                                    _context.InventoryTransactions.Add(new InventoryTransaction
                                    {
                                        ComId = skuInventory.ComId,
                                        TypeId = transactionType.TypeId,
                                        QuantityChange = -ol.Quantity,
                                        TimeStamp = order.OrderDate
                                    });
                                    changes = true;
                                }
                            }

                            foreach (var recipe in ol.SkuHeader.SkuRecipes)
                            {
                                if (recipe.ComId.HasValue)
                                {
                                    decimal qty = recipe.QuantityNeeded * ol.Quantity;

                                    // Fetch existing transactions for this item and type today to avoid too many DB calls
                                    var existingTransactions = await _context.InventoryTransactions
                                        .Where(t => t.ComId == recipe.ComId && t.TypeId == transactionType.TypeId)
                                        .ToListAsync();

                                    bool alreadyExists = existingTransactions.Any(t =>
                                        Math.Abs(t.QuantityChange) == Math.Abs(qty) &&
                                        Math.Abs((t.TimeStamp - order.OrderDate).TotalMinutes) < 10);

                                    if (!alreadyExists)
                                    {
                                        _context.InventoryTransactions.Add(new InventoryTransaction
                                        {
                                            ComId = recipe.ComId.Value,
                                            TypeId = transactionType.TypeId,
                                            QuantityChange = -qty,
                                            TimeStamp = order.OrderDate
                                        });
                                        changes = true;
                                    }
                                }
                            }
                        }
                        else if (ol.ComId.HasValue)
                        {
                            // Direct ingredient sale backfill
                            var existingTransactions = await _context.InventoryTransactions
                                .Where(t => t.ComId == ol.ComId && t.TypeId == transactionType.TypeId)
                                .ToListAsync();

                            bool alreadyExists = existingTransactions.Any(t =>
                                Math.Abs(t.QuantityChange) == ol.Quantity &&
                                Math.Abs((t.TimeStamp - order.OrderDate).TotalMinutes) < 10);

                            if (!alreadyExists)
                            {
                                _context.InventoryTransactions.Add(new InventoryTransaction
                                {
                                    ComId = ol.ComId.Value,
                                    TypeId = transactionType.TypeId,
                                    QuantityChange = -ol.Quantity,
                                    TimeStamp = order.OrderDate
                                });
                                changes = true;
                            }
                        }
                    }
                }
            }

            if (changes) await _context.SaveChangesAsync();
        }

        private async Task LoadSalesReport(int pageIndex, int pageSize)
        {
            var query = _context.OrderInfos
                .Where(o => o.Status == "Completed" && !o.IsArchived)
                .Include(o => o.BranchManager)
                    .ThenInclude(bm => bm.BranchLocation)
                .Include(o => o.Invoices)
                .AsQueryable();

            if (StartDate.HasValue)
                query = query.Where(o => o.OrderDate >= StartDate.Value);
            if (EndDate.HasValue)
                query = query.Where(o => o.OrderDate <= EndDate.Value);

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(o => 
                    o.BranchManager.BranchLocation.BranchName.Contains(SearchQuery) ||
                    o.OrderId.ToString().Contains(SearchQuery));
            }

            var sales = await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new SalesReportRow
                {
                    OrderId = o.OrderId,
                    BranchName = o.BranchManager.BranchLocation.BranchName,
                    TotalAmount = o.Invoices.Any() ? o.Invoices.First().TotalPrice : 0,
                    MoneyPaid = o.Invoices.Any() && o.Invoices.First().PaymentStatus == "Paid" ? o.Invoices.First().TotalPrice : 0,
                    Date = o.OrderDate
                })
                .ToListAsync();

            SalesReports = PaginatedList<SalesReportRow>.Create(sales, pageIndex, pageSize);
        }

        private async Task LoadRejectReport(int pageIndex, int pageSize)
        {
            var rejects = await _context.RejectItems
                .Where(r => r.RejectedAt >= StartDate && r.RejectedAt <= EndDate)
                .Select(r => new RejectReportRow
                {
                    ItemName = r.ItemName,
                    Quantity = r.Quantity,
                    Reason = r.Reason,
                    Type = r.RejectType,
                    Date = r.RejectedAt
                })
                .Where(r => string.IsNullOrEmpty(SearchQuery) || r.ItemName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            RejectReports = PaginatedList<RejectReportRow>.Create(rejects, pageIndex, pageSize);
        }

        private async Task LoadVendorSummaryReport(int pageIndex, int pageSize)
        {
            var query = _context.VendorInfos
                .Where(v => !v.IsArchived)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(v => v.VendorName.Contains(SearchQuery));
            }

            var vendors = await query.ToListAsync();
            var reportData = new List<VendorSummaryReportRow>();

            foreach (var vendor in vendors)
            {
                var ordersQuery = _context.SupplyOrders
                    .Where(so => so.VendorId == vendor.VendorId && !so.IsArchived);

                if (StartDate.HasValue) ordersQuery = ordersQuery.Where(so => so.SupplyDate >= StartDate.Value);
                if (EndDate.HasValue) ordersQuery = ordersQuery.Where(so => so.SupplyDate <= EndDate.Value);

                var orderIds = await ordersQuery.Select(so => so.SoaId).ToListAsync();
                
                var supplyLists = await _context.SupplyLists
                    .Where(sl => orderIds.Contains(sl.SupplyId))
                    .ToListAsync();

                reportData.Add(new VendorSummaryReportRow
                {
                    VendorName = vendor.VendorName,
                    TotalOrders = orderIds.Count,
                    TotalItems = supplyLists.Sum(sl => sl.Quantity),
                    TotalSpent = supplyLists.Sum(sl => sl.TotalPrice)
                });
            }

            VendorSummaryReports = PaginatedList<VendorSummaryReportRow>.Create(reportData.OrderByDescending(r => r.TotalSpent).ToList(), pageIndex, pageSize);
        }

        private async Task LoadBranchSummaryReport(int pageIndex, int pageSize)
        {
            var query = _context.BranchLocations
                .Where(bl => !bl.IsArchived)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(bl => bl.BranchName.Contains(SearchQuery));
            }

            var branches = await query.ToListAsync();
            var reportData = new List<BranchSummaryReportRow>();

            foreach (var branch in branches)
            {
                var ordersQuery = _context.OrderInfos
                    .Include(o => o.BranchManager)
                    .Where(o => o.BranchManager.BranchId == branch.BranchId && o.Status == "Completed" && !o.IsArchived);

                if (StartDate.HasValue) ordersQuery = ordersQuery.Where(o => o.OrderDate >= StartDate.Value);
                if (EndDate.HasValue) ordersQuery = ordersQuery.Where(o => o.OrderDate <= EndDate.Value);

                var orders = await ordersQuery
                    .Include(o => o.Invoices)
                    .ToListAsync();

                reportData.Add(new BranchSummaryReportRow
                {
                    BranchName = branch.BranchName,
                    TotalOrders = orders.Count,
                    TotalRevenue = orders.Sum(o => o.Invoices.Any() ? o.Invoices.First().TotalPrice : 0),
                    TotalCollected = orders.Sum(o => o.Invoices.Any() && o.Invoices.First().PaymentStatus == "Paid" ? o.Invoices.First().TotalPrice : 0)
                });
            }

            BranchSummaryReports = PaginatedList<BranchSummaryReportRow>.Create(reportData.OrderByDescending(r => r.TotalRevenue).ToList(), pageIndex, pageSize);
        }

        private async Task LoadIngredientFinanceReport(int pageIndex, int pageSize)
        {
            var query = _context.CommissaryInventories
                .Where(i => !i.IsArchived);

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(i => i.ItemName.Contains(SearchQuery));
            }

            var items = await query
                .OrderBy(i => i.ItemName)
                .Select(i => new IngredientFinanceReportRow
                {
                    ItemName = i.ItemName,
                    Uom = i.Uom,
                    CostPrice = i.CostPrice,
                    SellingPrice = i.SellingPrice
                })
                .ToListAsync();

            IngredientFinanceReports = PaginatedList<IngredientFinanceReportRow>.Create(items, pageIndex, pageSize);
        }

        private async Task LoadSkuFinanceReport(int pageIndex, int pageSize)
        {
            var query = _context.SkuHeaders
                .Where(s => !s.IsArchived);

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(s => s.ItemName.Contains(SearchQuery));
            }

            var items = await query
                .OrderBy(s => s.ItemName)
                .Select(s => new SkuFinanceReportRow
                {
                    ItemName = s.ItemName,
                    Uom = s.Uom,
                    CostPrice = s.UnitCost ?? 0,
                    SellingPrice = s.SellingPrice
                })
                .ToListAsync();

            SkuFinanceReports = PaginatedList<SkuFinanceReportRow>.Create(items, pageIndex, pageSize);
        }
    }
}

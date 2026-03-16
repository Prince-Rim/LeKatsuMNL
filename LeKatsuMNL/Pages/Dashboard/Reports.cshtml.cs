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

        // Sales Report
        public class SalesReportRow
        {
            public string ItemName { get; set; }
            public decimal Quantity { get; set; }
            public decimal TotalSales { get; set; }
            public DateTime Date { get; set; }
        }

        public PaginatedList<SalesReportRow> SalesReports { get; set; }

        // Expense Report
        public class ExpenseReportRow
        {
            public string ExpenseName { get; set; }
            public string Category { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
        }

        public PaginatedList<ExpenseReportRow> ExpenseReports { get; set; }

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

        // Restaurant Inventory
        public class RestaurantInventoryReportRow
        {
            public string ItemName { get; set; }
            public decimal Beginning { get; set; }
            public decimal Added { get; set; }
            public decimal Deducted { get; set; }
            public decimal Current { get; set; }
        }

        public PaginatedList<RestaurantInventoryReportRow> RestaurantInventoryReports { get; set; }

        // Restaurant Sales
        public class RestaurantSalesReportRow
        {
            public string ReceiptNum { get; set; }
            public decimal TotalPrice { get; set; }
            public DateTime Date { get; set; }
            public string StaffName { get; set; }
        }

        public PaginatedList<RestaurantSalesReportRow> RestaurantSalesReports { get; set; }

        public async Task OnGetAsync(int? pageIndex, int? pageSize)
        {
            // Set default dates if not provided (default to current month)
            StartDate ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate ??= DateTime.Now;

            // Ensure StartDate is at the beginning of the day and EndDate is at the end of the day
            // This ensures inclusive filtering regardless of literal time values
            if (StartDate.HasValue) StartDate = StartDate.Value.Date;
            if (EndDate.HasValue) EndDate = EndDate.Value.Date.AddDays(1).AddTicks(-1);

            int size = pageSize ?? 15;

            switch (ReportType)
            {
                case "Inventory":
                    await LoadIngredientReport(pageIndex ?? 1, size);
                    break;
                case "SkuInventory":
                    await LoadSkuInventoryReport(pageIndex ?? 1, size);
                    break;
                case "Sales":
                    await LoadSalesReport(pageIndex ?? 1, size);
                    break;
                case "Expenses":
                    await LoadExpenseReport(pageIndex ?? 1, size);
                    break;
                case "Rejects":
                    await LoadRejectReport(pageIndex ?? 1, size);
                    break;
                case "RestaurantInventory":
                    await LoadRestaurantInventoryReport(pageIndex ?? 1, size);
                    break;
                case "RestaurantSales":
                    await LoadRestaurantSalesReport(pageIndex ?? 1, size);
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
                    .Where(t => t.QuantityChange > 0)
                    .Sum(t => t.QuantityChange);

                decimal consumed = Math.Abs(transactionsInPeriod
                    .Where(t => t.QuantityChange < 0)
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
                    Uom = item.Uom
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
                    Uom = item.Uom
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
                .Where(o => o.Status == "Approved" || o.Status == "Preparing" || o.Status == "Delivered")
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
                        if (ol.SkuHeader == null) continue;

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
                }
            }

            if (changes) await _context.SaveChangesAsync();
        }

        private async Task LoadSalesReport(int pageIndex, int pageSize)
        {
            var sales = await _context.OrderListArchives
                .Join(_context.OrderInfos,
                    ola => ola.OrderId,
                    oi => oi.OrderId,
                    (ola, oi) => new { ola, oi })
                .Where(x => x.oi.OrderDate >= StartDate && x.oi.OrderDate <= EndDate)
                .Join(_context.CommissaryInventories,
                    x => x.ola.ComId,
                    s => s.ComId,
                    (x, s) => new SalesReportRow
                    {
                        ItemName = s.ItemName,
                        Quantity = x.ola.Quantity,
                        TotalSales = x.ola.TotalPrice,
                        Date = x.oi.OrderDate
                    })
                .Where(r => string.IsNullOrEmpty(SearchQuery) || r.ItemName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            SalesReports = PaginatedList<SalesReportRow>.Create(sales, pageIndex, pageSize);
        }

        private async Task LoadExpenseReport(int pageIndex, int pageSize)
        {
            var expenses = await _context.CashExpenses
                .Include(e => e.ExpenseType)
                .Where(e => e.DateTime >= StartDate && e.DateTime <= EndDate)
                .Select(e => new ExpenseReportRow
                {
                    ExpenseName = e.ExpenseName,
                    Category = e.ExpenseType.TypeName,
                    Amount = e.ExpenseAmount,
                    Date = e.DateTime
                })
                .Where(r => string.IsNullOrEmpty(SearchQuery) || r.ExpenseName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            ExpenseReports = PaginatedList<ExpenseReportRow>.Create(expenses, pageIndex, pageSize);
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

        private async Task LoadRestaurantInventoryReport(int pageIndex, int pageSize)
        {
            var resItems = await _context.RestaurantInventories
                .AsNoTracking()
                .ToListAsync();

            var reportData = resItems.Select(item => new RestaurantInventoryReportRow
            {
                ItemName = item.ItemName,
                Beginning = item.BeginningStock,
                Added = item.AddedStock ?? 0,
                Deducted = item.DeductedStock ?? 0,
                Current = item.CurrentStock
            })
            .Where(r => string.IsNullOrEmpty(SearchQuery) || r.ItemName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();

            RestaurantInventoryReports = PaginatedList<RestaurantInventoryReportRow>.Create(reportData, pageIndex, pageSize);
        }

        private async Task LoadRestaurantSalesReport(int pageIndex, int pageSize)
        {
            var sales = await _context.RestaurantTransactions
                .Include(t => t.Staff)
                .Where(t => t.DateTime >= StartDate && t.DateTime <= EndDate && !t.IsRefunded)
                .Select(t => new RestaurantSalesReportRow
                {
                    ReceiptNum = t.ReceiptNum,
                    TotalPrice = t.TotalPrice,
                    Date = t.DateTime,
                    StaffName = t.Staff.FirstName + " " + t.Staff.LastName
                })
                .Where(r => string.IsNullOrEmpty(SearchQuery) || r.ReceiptNum.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            RestaurantSalesReports = PaginatedList<RestaurantSalesReportRow>.Create(sales, pageIndex, pageSize);
        }
    }
}

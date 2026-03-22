using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeKatsuMNL.Helpers;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class AnalyticsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public AnalyticsModel(LeKatsuDb context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        // KPI Metrics
        public decimal TotalRevenue { get; set; }
        public decimal AccountsReceivable { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalSupplierExpenses { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        
        public int TotalOrders { get; set; }
        public int TotalCompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PreparingOrders { get; set; }
        public int DeliveringOrders { get; set; }
        public decimal TotalRejects { get; set; }
        public decimal TotalWasteValue { get; set; }
        public decimal TotalAssetValue { get; set; }

        // Chart Data (JSON-serializable)
        public string SalesTrendLabels { get; set; }
        public string SalesTrendData { get; set; }
        public string CategoryLabels { get; set; }
        public string CategoryData { get; set; }
        public string BranchLabels { get; set; }
        public string BranchData { get; set; }
        public string RejectLabels { get; set; }
        public string RejectData { get; set; }

        public async Task OnGetAsync()
        {
            // Default to last 30 days if not provided
            EndDate ??= DateTime.Now;
            StartDate ??= EndDate.Value.AddDays(-30);

            // Normalize dates for inclusive filtering
            var filterStart = StartDate.Value.Date;
            var filterEnd = EndDate.Value.Date.AddDays(1).AddTicks(-1);

            // Fetch all relevant orders within period (including full end day)
            var allOrders = await _context.OrderInfos
                .Where(o => o.OrderDate >= filterStart && o.OrderDate <= filterEnd && !o.IsArchived)
                .Include(o => o.Invoices)
                .Include(o => o.BranchManager)
                    .ThenInclude(bm => bm.BranchLocation)
                .ToListAsync();

            // 1. Financial Performance Analysis
            var revenueStatuses = new[] { "Approved", "Preparing", "Delivering", "Completed" };
            
            // Total Revenue (Total Sales Volume - sum of all invoices for approved/in-progress/completed orders)
            TotalRevenue = allOrders.Where(o => revenueStatuses.Contains(o.Status))
                                   .Sum(o => o.Invoices.Sum(i => i.TotalPrice));
            
            // Accounts Receivable (Total of invoices NOT paid)
            AccountsReceivable = allOrders.Where(o => revenueStatuses.Contains(o.Status))
                                         .SelectMany(o => o.Invoices)
                                         .Where(i => i.PaymentStatus != "Paid")
                                         .Sum(i => i.TotalPrice);

            // Total Collected (Actually paid)
            TotalCollected = allOrders.Where(o => revenueStatuses.Contains(o.Status))
                                     .SelectMany(o => o.Invoices)
                                     .Where(i => i.PaymentStatus == "Paid")
                                     .Sum(i => i.TotalPrice);
            
            TotalOrders = allOrders.Count(o => revenueStatuses.Contains(o.Status));
            TotalCompletedOrders = allOrders.Count(o => o.Status == "Completed");
            
            // 2. Supplier Expenses (Supply Orders in period)
            var supplyOrders = await _context.SupplyOrders
                .Where(so => so.SupplyDate >= filterStart && so.SupplyDate <= filterEnd && !so.IsArchived)
                .Include(so => so.SupplyLists)
                .ToListAsync();
            TotalSupplierExpenses = supplyOrders.Sum(so => so.SupplyLists.Sum(sl => sl.TotalPrice));

            // 3. Gross Profit Calculation (Total Revenue - COGS)
            var relevantOrderLists = await _context.OrderLists
                .Include(ol => ol.OrderInfo)
                .Include(ol => ol.CommissaryInventory)
                .Include(ol => ol.SkuHeader)
                .Where(ol => ol.OrderInfo.OrderDate >= filterStart && ol.OrderInfo.OrderDate <= filterEnd && 
                           revenueStatuses.Contains(ol.OrderInfo.Status) && !ol.OrderInfo.IsArchived)
                .ToListAsync();

            decimal estimatedCogs = relevantOrderLists.Sum(ol => 
            {
                decimal cost = 0;
                if (ol.CommissaryInventory != null) cost = ol.CommissaryInventory.CostPrice;
                else if (ol.SkuHeader != null) cost = ol.SkuHeader.UnitCost ?? 0;
                return ol.Quantity * cost;
            });

            GrossProfit = TotalRevenue - estimatedCogs;
            ProfitMargin = TotalRevenue > 0 ? (GrossProfit / TotalRevenue * 100) : 0;

            // 4. Pipeline Metrics
            PendingOrders = allOrders.Count(o => o.Status == "Pending" || o.Status == "Approved");
            PreparingOrders = allOrders.Count(o => o.Status == "Preparing");
            DeliveringOrders = allOrders.Count(o => o.Status == "Delivering");

            // 5. Waste & Assets
            var rejects = await _context.RejectItems
                .Include(r => r.CommissaryInventory)
                .Include(r => r.SkuHeader)
                .Where(r => r.RejectedAt >= filterStart && r.RejectedAt <= filterEnd &&
                           (r.CommissaryInventory == null || !r.CommissaryInventory.IsArchived) &&
                           (r.SkuHeader == null || !r.SkuHeader.IsArchived))
                .ToListAsync();
            TotalRejects = rejects.Sum(r => r.Quantity);
            TotalWasteValue = rejects.Sum(r => 
            {
                decimal qty = r.Quantity;
                decimal price = 0;
                string uom = "";

                if (r.CommissaryInventory != null) 
                {
                    price = r.CommissaryInventory.CostPrice;
                    uom = r.CommissaryInventory.Uom;
                }
                else if (r.SkuHeader != null) 
                {
                    price = r.SkuHeader.UnitCost ?? 0;
                    uom = "pcs";
                }

                if (string.IsNullOrEmpty(uom)) return qty * price;

                if (UomConverter.AreUnitsCompatible(uom, "kg"))
                    return UomConverter.Convert(qty, uom, "kg") * price;
                if (UomConverter.AreUnitsCompatible(uom, "liter"))
                    return UomConverter.Convert(qty, uom, "liter") * price;

                return qty * price;
            });

            var inventory = await _context.CommissaryInventories
                .Where(i => !i.IsArchived)
                .ToListAsync();

            TotalAssetValue = inventory.Sum(i => 
            {
                if (string.IsNullOrEmpty(i.Uom)) return i.Stock * i.CostPrice;
                if (UomConverter.AreUnitsCompatible(i.Uom, "kg"))
                    return UomConverter.Convert(i.Stock, i.Uom, "kg") * i.CostPrice;
                if (UomConverter.AreUnitsCompatible(i.Uom, "liter"))
                    return UomConverter.Convert(i.Stock, i.Uom, "liter") * i.CostPrice;
                return i.Stock * i.CostPrice;
            });

            // 1. Sales Trend (Line Chart)
            var dailySales = allOrders
                .Where(o => revenueStatuses.Contains(o.Status))
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(o => o.Invoices.Sum(i => i.TotalPrice)) })
                .OrderBy(g => g.Date)
                .ToList();

            SalesTrendLabels = System.Text.Json.JsonSerializer.Serialize(dailySales.Select(s => s.Date.ToString("MMM dd")));
            SalesTrendData = System.Text.Json.JsonSerializer.Serialize(dailySales.Select(s => s.Total));

            // 2. Branch Revenue (Bar Chart)
            var branchRevenue = allOrders
                .Where(o => revenueStatuses.Contains(o.Status))
                .GroupBy(o => o.BranchManager.BranchLocation.BranchName)
                .Select(g => new { Branch = g.Key, Total = g.Sum(o => o.Invoices.Sum(i => i.TotalPrice)) })
                .OrderByDescending(g => g.Total)
                .ToList();

            BranchLabels = System.Text.Json.JsonSerializer.Serialize(branchRevenue.Select(b => b.Branch));
            BranchData = System.Text.Json.JsonSerializer.Serialize(branchRevenue.Select(b => b.Total));

            // 3. Category Distribution (Doughnut Chart - based on inventory consumption)
            var orderLists = await _context.OrderLists
                .Include(ol => ol.OrderInfo)
                .Include(ol => ol.CommissaryInventory)
                    .ThenInclude(ci => ci.Category)
                .Where(ol => ol.OrderInfo.OrderDate >= filterStart && ol.OrderInfo.OrderDate <= filterEnd && 
                           revenueStatuses.Contains(ol.OrderInfo.Status) && !ol.OrderInfo.IsArchived)
                .ToListAsync();

            var categoryUsage = orderLists
                .Where(ol => ol.CommissaryInventory?.Category != null)
                .GroupBy(ol => ol.CommissaryInventory.Category.CategoryName)
                .Select(g => new { Category = g.Key, Count = g.Count() }) // Using count of items ordered as a proxy for distribution
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();

            CategoryLabels = System.Text.Json.JsonSerializer.Serialize(categoryUsage.Select(c => c.Category));
            CategoryData = System.Text.Json.JsonSerializer.Serialize(categoryUsage.Select(c => c.Count));

            // 4. Reject Analysis (Pie Chart)
            var rejectReasons = rejects
                .GroupBy(r => r.Reason ?? "Unknown")
                .Select(g => new { Reason = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();

            RejectLabels = System.Text.Json.JsonSerializer.Serialize(rejectReasons.Select(r => r.Reason));
            RejectData = System.Text.Json.JsonSerializer.Serialize(rejectReasons.Select(r => r.Count));
        }
    }
}

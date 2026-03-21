using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeKatsuMNL.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public IndexModel(LeKatsuDb context)
        {
            _context = context;
        }

        // KPI Metrics
        public decimal TotalBranchSales { get; set; }
        public int LowStockCount { get; set; }
        public int PendingOrdersCount { get; set; }
        public int DiscrepancyCount { get; set; }

        public List<LowStockItem> LowStockAlerts { get; set; } = new List<LowStockItem>();
        public List<RejectItem> RecentRejects { get; set; } = new List<RejectItem>();
        public List<BranchPerformance> TopBranches { get; set; } = new List<BranchPerformance>();
        public List<ActivityItem> RecentActivities { get; set; } = new List<ActivityItem>();

        public class BranchPerformance
        {
            public string BranchName { get; set; }
            public decimal TotalRevenue { get; set; }
            public double GrowthPercentage { get; set; }
        }

        public class ActivityItem
        {
            public string Title { get; set; }
            public string TimeAgo { get; set; }
            public string Color { get; set; }
        }

        public class LowStockItem
        {
            public string ItemName { get; set; }
            public decimal CurrentStock { get; set; }
            public decimal? ReorderLevel { get; set; }
            public string Status { get; set; }
            public string StatusColor { get; set; }
        }

        public async Task OnGetAsync()
        {
            // 1. Total Branch Sales (Total from all completed order invoices)
            TotalBranchSales = await _context.Invoices
                .Include(i => i.OrderInfo)
                .Where(i => i.OrderInfo.Status == "Completed" && !i.OrderInfo.IsArchived)
                .SumAsync(i => i.TotalPrice);

            // 2. Low Stock Count
            var lowStockItems = await _context.CommissaryInventories
                .Where(i => i.SkuId == null && i.Stock < i.ReorderValue && !i.IsArchived)
                .ToListAsync();
            LowStockCount = lowStockItems.Count;

            // 3. Pending Orders Count (Approved/Preparing/Delivered but not Completed)
            PendingOrdersCount = await _context.OrderInfos
                .Where(o => (o.Status == "Approved" || o.Status == "Preparing" || o.Status == "Delivered") && !o.IsArchived)
                .CountAsync();

            // 4. Discrepancy Count (Using recent rejects of non-archived items)
            DiscrepancyCount = await _context.RejectItems
                .Include(r => r.CommissaryInventory)
                .Include(r => r.SkuHeader)
                .Where(r => r.RejectedAt >= DateTime.Now.Date && 
                           (r.CommissaryInventory == null || !r.CommissaryInventory.IsArchived) &&
                           (r.SkuHeader == null || !r.SkuHeader.IsArchived))
                .CountAsync();

            // 5. Low Stock Alerts Table
            LowStockAlerts = lowStockItems.Take(5).Select(i => new LowStockItem
            {
                ItemName = i.ItemName,
                CurrentStock = i.Stock,
                ReorderLevel = i.ReorderValue,
                Status = i.Stock <= (i.ReorderValue * 0.5m) ? "Critical" : "Warning",
                StatusColor = i.Stock <= (i.ReorderValue * 0.5m) ? "red" : "orange"
            }).ToList();

            // 6. Recent Rejects Table (Excluding archived items)
            RecentRejects = await _context.RejectItems
                .Include(r => r.CommissaryInventory)
                .Include(r => r.SkuHeader)
                .Where(r => (r.CommissaryInventory == null || !r.CommissaryInventory.IsArchived) &&
                           (r.SkuHeader == null || !r.SkuHeader.IsArchived))
                .OrderByDescending(r => r.RejectedAt)
                .Take(5)
                .ToListAsync();

            // 7. Top Branches (Refactored to avoid SqlException)
            TopBranches = await _context.Invoices
                .Include(i => i.OrderInfo)
                    .ThenInclude(o => o.BranchManager)
                        .ThenInclude(bm => bm.BranchLocation)
                .Where(i => i.OrderInfo.Status == "Completed" && !i.OrderInfo.IsArchived)
                .GroupBy(i => i.OrderInfo.BranchManager.BranchLocation.BranchName)
                .Select(g => new BranchPerformance
                {
                    BranchName = g.Key,
                    TotalRevenue = g.Sum(i => i.TotalPrice),
                    GrowthPercentage = 15.0 // Static for now
                })
                .OrderByDescending(b => b.TotalRevenue)
                .Take(3)
                .ToListAsync();

            // 8. Recent Activity (Excluding archived items)
            var recentTransactions = await _context.InventoryTransactions
                .Include(t => t.CommissaryInventory)
                .Include(t => t.InvTransactionType)
                .Where(t => !t.CommissaryInventory.IsArchived)
                .OrderByDescending(t => t.TimeStamp)
                .Take(3)
                .ToListAsync();

            RecentActivities = recentTransactions.Select(t => new ActivityItem
            {
                Title = $"Commissary: {t.CommissaryInventory.ItemName} stock {(t.QuantityChange > 0 ? "increased" : "decreased")} by {Math.Abs(t.QuantityChange)}",
                TimeAgo = GetTimeAgo(t.TimeStamp),
                Color = t.QuantityChange > 0 ? "blue" : "red"
            }).ToList();
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} minutes ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
            return $"{(int)span.TotalDays} days ago";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Security.Claims;

namespace LeKatsuMNL.Pages.BranchDashboard
{
    public class IndexModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public IndexModel(LeKatsuDb context)
        {
            _context = context;
        }

        public decimal TotalMonthlySpend { get; set; }
        public int PendingOrdersCount { get; set; }
        public int PendingPaymentsCount { get; set; }
        public string BranchName { get; set; }

        public List<decimal> WeeklySpendData { get; set; } = new List<decimal>();
        public List<string> WeeklySpendLabels { get; set; } = new List<string>();
        public List<int> MonthlyOrdersData { get; set; } = new List<int>();

        public List<OrderInfo> RecentOrders { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login/login");

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
                return RedirectToPage("/Login/login");

            BranchName = User.FindFirst("BranchName")?.Value ?? "Branch";

            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfToday = now.Date;
            var startOfLast7Days = startOfToday.AddDays(-6);
            var startOfYear = new DateTime(now.Year, 1, 1);

            // Base query for this branch's orders
            IQueryable<OrderInfo> branchOrders = _context.OrderInfos
                .Where(o => o.BranchManagerId == userId && !o.IsArchived);

            // 1. Total Monthly Spend (Current Month, Exclude Cancelled)
            TotalMonthlySpend = await _context.OrderLists
                .Where(ol => ol.OrderInfo.BranchManagerId == userId 
                          && ol.OrderInfo.OrderDate >= startOfMonth 
                          && ol.OrderInfo.Status != "Cancelled")
                .SumAsync(ol => (decimal?)ol.TotalPrice) ?? 0;

            // 2. Pending Orders (Active statuses: Pending, Approved, Preparing)
            PendingOrdersCount = await branchOrders
                .CountAsync(o => o.Status == "Pending" || o.Status == "Approved" || o.Status == "Preparing");

            // 3. Pending Payments
            PendingPaymentsCount = await _context.Invoices
                .CountAsync(i => i.OrderInfo.BranchManagerId == userId && i.PaymentStatus == "Pending");

            // 4. Recent Orders
            RecentOrders = await branchOrders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Include(o => o.OrderLists)
                .Include(o => o.Invoices)
                .ToListAsync();

            // 5. Weekly Spend Data (Last 7 days including today)
            var weeklyOrders = await _context.OrderLists
                .Where(ol => ol.OrderInfo.BranchManagerId == userId 
                          && ol.OrderInfo.OrderDate >= startOfLast7Days 
                          && ol.OrderInfo.Status != "Cancelled")
                .Select(ol => new { ol.OrderInfo.OrderDate, ol.TotalPrice })
                .ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                var date = startOfLast7Days.AddDays(i);
                var daySpend = weeklyOrders
                    .Where(o => o.OrderDate.Date == date)
                    .Sum(o => o.TotalPrice);
                WeeklySpendData.Add(daySpend);
                WeeklySpendLabels.Add(date.ToString("ddd"));
            }

            // 6. Monthly Orders Data (Current Year)
            var yearlyOrders = await branchOrders
                .Where(o => o.OrderDate >= startOfYear)
                .Select(o => o.OrderDate.Month)
                .ToListAsync();

            for (int m = 1; m <= 12; m++)
            {
                MonthlyOrdersData.Add(yearlyOrders.Count(month => month == m));
            }

            return Page();
        }
    }
}

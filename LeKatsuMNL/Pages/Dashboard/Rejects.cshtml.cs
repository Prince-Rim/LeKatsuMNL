using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using LeKatsuMNL.Helpers;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class RejectsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public RejectsModel(LeKatsuDb context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        public PaginatedList<RejectItem> RejectLogs { get; set; } = default!;

        public int PageSize { get; set; } = 10;
 
        [TempData]
        public string StatusMessage { get; set; }

         public async Task OnGetAsync(int? pageIndex)
         {
             // Set default dates if not provided (default to current month)
             StartDate ??= new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
             EndDate ??= System.DateTime.Now;

             // Normalize dates
             var filterStart = StartDate.Value.Date;
             var filterEnd = EndDate.Value.Date.AddDays(1).AddTicks(-1);

             var query = _context.RejectItems
                .Include(r => r.CommissaryInventory)
                .Include(r => r.SkuHeader)
                .Where(r => r.RejectedAt >= filterStart && r.RejectedAt <= filterEnd);

             if (!string.IsNullOrEmpty(SearchQuery))
             {
                 query = query.Where(r => r.ItemName.Contains(SearchQuery));
             }

             int pageSize = PageSize > 0 ? PageSize : 10;
             RejectLogs = await PaginatedList<RejectItem>.CreateAsync(
                 query.OrderByDescending(r => r.RejectedAt), 
                 pageIndex ?? 1, pageSize);
         }

        public async Task<IActionResult> OnPostClearLogsAsync(DateTime? StartDate, DateTime? EndDate, string? SearchQuery)
        {
            if (!PermissionHelper.HasPermission(User, "Rejects", 'D')) return Forbid();

            // Apply the same filter logic as OnGet
            var filterStart = (StartDate ?? new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1)).Date;
            var filterEnd = (EndDate ?? System.DateTime.Now).Date.AddDays(1).AddTicks(-1);

            var query = _context.RejectItems
                .Where(r => r.RejectType == "Recipe" && r.RejectedAt >= filterStart && r.RejectedAt <= filterEnd);

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(r => r.ItemName.Contains(SearchQuery));
            }

            var rejects = await query.ToListAsync();
            _context.RejectItems.RemoveRange(rejects);
            await _context.SaveChangesAsync();

            StatusMessage = "Filtered reject logs have been successfully cleared.";
            return RedirectToPage(new { StartDate, EndDate, SearchQuery });
        }

         public async Task<IActionResult> OnGetExportAsync()
         {
             if (!PermissionHelper.HasPermission(User, "Rejects", 'R')) return Forbid();

             // Use the same filter logic for export
             var filterStart = (StartDate ?? new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1)).Date;
             var filterEnd = (EndDate ?? System.DateTime.Now).Date.AddDays(1).AddTicks(-1);

             var query = _context.RejectItems
                 .Where(r => r.RejectType == "Recipe" && r.RejectedAt >= filterStart && r.RejectedAt <= filterEnd);

             if (!string.IsNullOrEmpty(SearchQuery))
             {
                 query = query.Where(r => r.ItemName.Contains(SearchQuery));
             }

             var rejects = await query
                 .OrderByDescending(r => r.RejectedAt)
                 .ToListAsync();

             var csv = new System.Text.StringBuilder();
             csv.AppendLine("Item Name,Quantity,UOM,Reason,Rejected At");

             foreach (var reject in rejects)
             {
                 csv.AppendLine(string.Join(",",
                     EscapeCsv(reject.ItemName),
                     reject.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture),
                     EscapeCsv(reject.Uom),
                     EscapeCsv(string.IsNullOrWhiteSpace(reject.Reason) ? "-" : reject.Reason),
                     EscapeCsv(reject.RejectedAt.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture))));
             }

             return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"RejectLogs_{System.DateTime.Now:yyyyMMdd}.csv");
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

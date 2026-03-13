using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LeKatsuMNL.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class ReportsModel : PageModel
    {
        public class ReportRow
        {
            public string ItemName { get; set; }
            public int Beginning { get; set; }
            public int Ending { get; set; }
            public int Received { get; set; }
            public int Consumed { get; set; }
            public int Rejected { get; set; }
        }

        public PaginatedList<ReportRow> InventoryReports { get; set; }

        public void OnGet(int? pageIndex)
        {
            // Sample data for the reports
            var samples = new List<ReportRow>
            {
                new ReportRow { ItemName = "Marinated Chicken (kg)", Beginning = 150, Ending = 120, Received = 50, Consumed = 80, Rejected = 0 },
                new ReportRow { ItemName = "Cooking Oil (18L)", Beginning = 40, Ending = 32, Received = 10, Consumed = 18, Rejected = 0 },
                new ReportRow { ItemName = "Breading Mix (kg)", Beginning = 25, Ending = 20, Received = 5, Consumed = 10, Rejected = 0 }
            };

            InventoryReports = PaginatedList<ReportRow>.Create(samples, pageIndex ?? 1, 10);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LeKatsuMNL.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class ReceivedStocksModel : PageModel
    {
        public class ReceivedStockRow
        {
            public string ItemName { get; set; }
            public string Supplier { get; set; }
            public string OrderDate { get; set; }
            public string ExpectedDate { get; set; }
            public string ArrivalDate { get; set; }
            public int Quantity { get; set; }
            public string Unit { get; set; }
            public string Status { get; set; }
        }

        public PaginatedList<ReceivedStockRow> ReceivedItems { get; set; }

        public void OnGet(int? pageIndex)
        {
            // Sample data to maintain the UI state
            var samples = new List<ReceivedStockRow>
            {
                new ReceivedStockRow { ItemName = "Marinated Chicken", Supplier = "Fresh Choice", OrderDate = "Dec 10, 2026", ExpectedDate = "Dec 12, 2026", ArrivalDate = "Dec 13, 2026", Quantity = 25, Unit = "Pack", Status = "Received" },
                new ReceivedStockRow { ItemName = "Cooking Oil", Supplier = "UniOil", OrderDate = "Dec 11, 2026", ExpectedDate = "Dec 13, 2026", ArrivalDate = "-", Quantity = 10, Unit = "Pail", Status = "In Transit" },
                new ReceivedStockRow { ItemName = "Breading Mix", Supplier = "Global Spices", OrderDate = "Dec 12, 2026", ExpectedDate = "Dec 14, 2026", ArrivalDate = "-", Quantity = 5, Unit = "Bag", Status = "In Transit" }
            };

            ReceivedItems = PaginatedList<ReceivedStockRow>.Create(samples, pageIndex ?? 1, 10);
        }
    }
}

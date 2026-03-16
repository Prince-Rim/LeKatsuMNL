using System;
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
    public class BillingModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public BillingModel(LeKatsuDb context)
        {
            _context = context;
        }

        public PaginatedList<Invoice> Invoices { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        public string CurrentSortColumn { get; set; } = "Date";
        public string CurrentSortOrder { get; set; } = "desc";

        public async Task OnGetAsync(int? pageIndex, string sortColumn, string sortOrder)
        {
            CurrentSortColumn = sortColumn ?? "Date";
            CurrentSortOrder = sortOrder ?? "desc";

            IQueryable<Invoice> query = _context.Invoices
                .Include(i => i.OrderInfo);
            
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var cleanSearch = SearchTerm.Trim();
                // Handle "ORD-" prefix
                if (cleanSearch.StartsWith("ORD-", StringComparison.OrdinalIgnoreCase))
                {
                    cleanSearch = cleanSearch.Substring(4);
                }

                var isNumeric = int.TryParse(cleanSearch, out int orderId);

                // Filter by Order ID (ORD-XXXXX) or status
                query = query.Where(i => (isNumeric && i.OrderId == orderId) || 
                                       i.OrderId.ToString().Contains(SearchTerm) || 
                                       i.PaymentStatus.Contains(SearchTerm));
            }

            // Apply Sorting
            query = CurrentSortColumn switch
            {
                "Date" => CurrentSortOrder == "asc" ? query.OrderBy(i => i.InvoiceDate) : query.OrderByDescending(i => i.InvoiceDate),
                "OrderId" => CurrentSortOrder == "asc" ? query.OrderBy(i => i.OrderId) : query.OrderByDescending(i => i.OrderId),
                "Amount" => CurrentSortOrder == "asc" ? query.OrderBy(i => i.TotalPrice) : query.OrderByDescending(i => i.TotalPrice),
                "Status" => CurrentSortOrder == "asc" ? query.OrderBy(i => i.PaymentStatus) : query.OrderByDescending(i => i.PaymentStatus),
                _ => query.OrderByDescending(i => i.InvoiceDate)
            };

            Invoices = await PaginatedList<Invoice>.CreateAsync(query, pageIndex ?? 1, 10);
        }

        public async Task<IActionResult> OnPostMarkAsPaidAsync(int id, string paymentMethod, string referenceNumber)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            invoice.PaymentStatus = "Paid";
            invoice.PaymentMethod = paymentMethod;
            invoice.ReferenceNumber = referenceNumber;
            invoice.PaymentDate = DateTime.Now;
            invoice.VerifiedBy = User.Identity?.Name ?? "System";

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}

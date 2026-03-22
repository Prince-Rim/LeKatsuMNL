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
        public List<BranchLocation> BranchLocations { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int? SelectedBranchId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedPaymentStatus { get; set; }

        public async Task OnGetAsync(int? pageIndex)
        {
            var query = _context.Invoices
                .Include(i => i.OrderInfo)
                    .ThenInclude(o => o.BranchManager)
                        .ThenInclude(bm => bm.BranchLocation)
                .Where(i => !i.OrderInfo.IsArchived)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var search = SearchTerm.Trim().ToLower();
                query = query.Where(i => i.OrderId.ToString().Contains(search) || 
                                         i.ReferenceNumber.ToLower().Contains(search) || 
                                         i.PaymentStatus.ToLower().Contains(search) ||
                                         i.PaymentMethod.ToLower().Contains(search) ||
                                         i.OrderInfo.BranchManager.BranchLocation.BranchName.ToLower().Contains(search));
            }

            if (SelectedBranchId.HasValue && SelectedBranchId.Value > 0)
            {
                query = query.Where(i => i.OrderInfo.BranchManager.BranchLocation.BranchId == SelectedBranchId.Value);
            }

            if (!string.IsNullOrEmpty(SelectedPaymentStatus))
            {
                query = query.Where(i => i.PaymentStatus == SelectedPaymentStatus);
            }

            query = query.OrderByDescending(i => i.InvoiceDate);
            
            int pageSize = PageSize > 0 ? PageSize : 10;
            Invoices = await PaginatedList<Invoice>.CreateAsync(query, pageIndex ?? 1, pageSize);

            BranchLocations = await _context.BranchLocations
                .Where(bl => !bl.IsArchived)
                .OrderBy(bl => bl.BranchName)
                .ToListAsync();
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
        public async Task<IActionResult> OnGetExportAsync()
        {
            var invoices = await _context.Invoices
                .Include(i => i.OrderInfo)
                    .ThenInclude(o => o.BranchManager)
                        .ThenInclude(bm => bm.BranchLocation)
                .Where(i => !i.OrderInfo.IsArchived)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Date,Branch,Order ID,Reference,Total Amount,Payment Status,Payment Method");

            foreach (var inv in invoices)
            {
                string date = inv.InvoiceDate.ToString("yyyy-MM-dd");
                string branch = inv.OrderInfo?.BranchManager?.BranchLocation?.BranchName ?? "N/A";
                string orderId = $"{inv.OrderInfo?.OrderDate.Year}-{inv.OrderId:D4}";
                string reference = inv.ReferenceNumber ?? "";
                string amount = inv.TotalPrice.ToString("F2");
                string status = inv.PaymentStatus;
                string method = inv.PaymentMethod;

                csv.AppendLine(string.Join(",",
                    EscapeCsv(date),
                    EscapeCsv(branch),
                    EscapeCsv(orderId),
                    EscapeCsv(reference),
                    EscapeCsv(amount),
                    EscapeCsv(status),
                    EscapeCsv(method)));
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Billing_{DateTime.Now:yyyyMMdd}.csv");
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

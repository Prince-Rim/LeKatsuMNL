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

        public async Task OnGetAsync(int? pageIndex)
        {
            var query = _context.Invoices
                .Include(i => i.OrderInfo)
                .OrderByDescending(i => i.InvoiceDate);
            
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

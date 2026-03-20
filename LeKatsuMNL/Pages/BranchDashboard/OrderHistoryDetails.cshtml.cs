using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace LeKatsuMNL.Pages.BranchDashboard
{
    public class OrderHistoryDetailsModel : PageModel
    {
        private readonly LeKatsuDb _context;
        private readonly LeKatsuMNL.Services.IPayMongoService _payMongoService;

        public OrderHistoryDetailsModel(LeKatsuDb context, LeKatsuMNL.Services.IPayMongoService payMongoService)
        {
            _context = context;
            _payMongoService = payMongoService;
        }

        public OrderInfo Order { get; set; }
        public Invoice Invoice { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login/login");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("/Login/login");

            Order = await _context.OrderInfos
                .Where(o => o.OrderId == id
                         && o.BranchManagerId == branchManagerId
                         && !o.IsArchived)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                        .ThenInclude(s => s.Category)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.CommissaryInventory)
                        .ThenInclude(c => c.Category)
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync();

            if (Order == null)
                return RedirectToPage("./OrderHistory");

            // PROACTIVE CHECK: If there's a pending invoice with SESSION: prefix, verify status with PayMongo
            var sessionInvoice = Order.Invoices?.FirstOrDefault(i => i.PaymentStatus == "Pending" && i.ReferenceNumber != null && i.ReferenceNumber.StartsWith("SESSION:"));
            if (sessionInvoice != null)
            {
                var sessionId = sessionInvoice.ReferenceNumber.Substring(8);
                var status = await _payMongoService.GetCheckoutSessionStatusAsync(sessionId);
                
                if (status == "paid")
                {
                    var details = await _payMongoService.GetPaymentDetailsAsync(sessionId);
                    sessionInvoice.PaymentStatus = "Paid";
                    sessionInvoice.PaymentDate = System.DateTime.Now;
                    
                    string method = details.Method;
                    if (!string.IsNullOrEmpty(method))
                    {
                        method = char.ToUpper(method[0]) + method.Substring(1).ToLower();
                        if (method.ToLower() == "paymaya") method = "Maya";
                    }
                    sessionInvoice.PaymentMethod = $"PayMongo ({method})";
                    sessionInvoice.ReferenceNumber = details.PaymentId;

                    if (Order.Status == "Approved")
                    {
                        Order.Status = "Preparing";
                    }

                    await _context.SaveChangesAsync();
                }
            }

            Invoice = Order.Invoices?.OrderByDescending(i => i.InvoiceDate).FirstOrDefault();

            return Page();
        }

        public async Task<IActionResult> OnPostVerifyAsync(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("/Login/login");

            var order = await _context.OrderInfos
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.BranchManagerId == branchManagerId && !o.IsArchived);

            if (order != null)
            {
                var invoice = order.Invoices.FirstOrDefault(i => (i.PaymentStatus == "Pending" || i.PaymentStatus == null) && i.ReferenceNumber != null && i.ReferenceNumber.StartsWith("SESSION:"));
                if (invoice != null)
                {
                    var sessionId = invoice.ReferenceNumber.Substring(8);
                    var status = await _payMongoService.GetCheckoutSessionStatusAsync(sessionId);
                    
                    if (status == "paid")
                    {
                        var details = await _payMongoService.GetPaymentDetailsAsync(sessionId);
                        invoice.PaymentStatus = "Paid";
                        invoice.PaymentDate = DateTime.Now;
                        
                        string method = details.Method;
                        if (!string.IsNullOrEmpty(method))
                        {
                            method = char.ToUpper(method[0]) + method.Substring(1).ToLower();
                            if (method.ToLower() == "paymaya") method = "Maya";
                        }
                        invoice.PaymentMethod = $"PayMongo ({method})";
                        invoice.ReferenceNumber = details.PaymentId;

                        if (order.Status == "Approved")
                        {
                            order.Status = "Preparing";
                        }

                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Payment verified successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Payment status is still {status}. Please wait a moment.";
                    }
                }
            }
            return RedirectToPage(new { id = id });
        }
    }
}

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Threading.Tasks;
using System.Linq;

namespace LeKatsuMNL.Pages.Payment
{
    public class SuccessModel : PageModel
    {
        private readonly LeKatsuDb _context;
        private readonly LeKatsuMNL.Services.IPayMongoService _payMongoService;

        public SuccessModel(LeKatsuDb context, LeKatsuMNL.Services.IPayMongoService payMongoService)
        {
            _context = context;
            _payMongoService = payMongoService;
        }

        public int OrderId { get; set; }

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            OrderId = orderId;

            var order = await _context.OrderInfos
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order != null)
            {
                // Find the pending invoice, prioritizing one with a session ID
                var invoice = order.Invoices.FirstOrDefault(i => i.ReferenceNumber != null && i.ReferenceNumber.StartsWith("SESSION:"))
                           ?? order.Invoices.FirstOrDefault(i => i.PaymentStatus == "Pending");

                if (invoice != null)
                {
                    bool isPaid = false;
                    string method = "PayMongo";
                    string refNo = invoice.ReferenceNumber ?? "PAYMON-" + Guid.NewGuid().ToString().ToUpper().Substring(0, 8);

                    if (invoice.ReferenceNumber != null && invoice.ReferenceNumber.StartsWith("SESSION:"))
                    {
                        var sessionId = invoice.ReferenceNumber.Substring(8);
                        var status = await _payMongoService.GetCheckoutSessionStatusAsync(sessionId);
                        
                        if (status == "paid")
                        {
                            isPaid = true;
                            var details = await _payMongoService.GetPaymentDetailsAsync(sessionId);
                            method = details.Method;
                            refNo = details.PaymentId;
                        }
                    }
                    else
                    {
                        // Direct fallback is insecure. Require a session for verification.
                        isPaid = false; 
                    }

                    if (isPaid)
                    {
                        invoice.PaymentStatus = "Paid";
                        invoice.PaymentDate = DateTime.Now;
                        
                        // Format method name (e.g. gcash -> Gcash, paymaya -> Paymaya)
                        string displayMethod = method;
                        if (!string.IsNullOrEmpty(displayMethod))
                        {
                            displayMethod = char.ToUpper(displayMethod[0]) + displayMethod.Substring(1).ToLower();
                            if (displayMethod.ToLower() == "paymaya") displayMethod = "Maya";
                        }
                        
                        invoice.PaymentMethod = $"PayMongo ({displayMethod})";
                        invoice.ReferenceNumber = refNo;

                        if (order.Status == "Approved")
                        {
                            order.Status = "Preparing";
                        }

                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Page();
        }
    }
}

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using LeKatsuMNL.Services;

namespace LeKatsuMNL.Pages.BranchDashboard
{
    public class OrderDetailsModel : PageModel
    {
        private readonly LeKatsuDb _context;
        private readonly IPayMongoService _payMongoService;

        public OrderDetailsModel(LeKatsuDb context, IPayMongoService payMongoService)
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
                .Where(o => o.OrderId == id && o.BranchManagerId == branchManagerId && !o.IsArchived)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.SkuHeader)
                        .ThenInclude(s => s != null ? s.Category : null)
                .Include(o => o.OrderLists)
                    .ThenInclude(ol => ol.CommissaryInventory)
                        .ThenInclude(c => c != null ? c.Category : null)
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync();

            if (Order == null)
                return RedirectToPage("./MyOrder");

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
                    sessionInvoice.PaymentDate = DateTime.Now;
                    
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
                else if (status == "expired")
                {
                    // Optionally reset or handle expiry
                    sessionInvoice.ReferenceNumber = "EXPIRED:" + sessionId;
                    await _context.SaveChangesAsync();
                }
            }

            Invoice = Order.Invoices?.FirstOrDefault();

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("./MyOrder");

            var order = await _context.OrderInfos
                .FirstOrDefaultAsync(o => o.OrderId == id && o.BranchManagerId == branchManagerId);

            if (order != null && order.Status == "Pending")
            {
                order.Status = "Cancelled";
                await _context.SaveChangesAsync();
                TempData["Message"] = "Order cancelled.";
            }
            return RedirectToPage("./MyOrder");
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("./MyOrder");

            var order = await _context.OrderInfos
                .FirstOrDefaultAsync(o => o.OrderId == id && o.BranchManagerId == branchManagerId);

            if (order != null && order.Status == "Delivering")
            {
                order.Status = "Completed";
                await _context.SaveChangesAsync();
                TempData["Message"] = "Transaction completed.";
            }
            return RedirectToPage("./MyOrder", new { id = id });
        }

        public async Task<IActionResult> OnPostPayAsync(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int branchManagerId))
                return RedirectToPage("/Login/login");

            var order = await _context.OrderInfos
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.BranchManagerId == branchManagerId && !o.IsArchived);

            if (order == null)
                return NotFound();

            var invoice = order.Invoices.FirstOrDefault(i => i.PaymentStatus == "Pending");
            if (invoice == null)
            {
                TempData["ErrorMessage"] = "No pending invoice found for this order.";
                return RedirectToPage(new { id = id });
            }

            var description = $"Payment for Order #{order.OrderId}";
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var successUrl = $"{baseUrl}/Payment/Success?orderId={order.OrderId}";
            var cancelUrl = $"{baseUrl}/Payment/Cancel?orderId={order.OrderId}";

            try
            {
                // Re-use active session if available
                if (invoice.ReferenceNumber != null && invoice.ReferenceNumber.StartsWith("SESSION:"))
                {
                    var existingSessionId = invoice.ReferenceNumber.Substring(8);
                    var existingSession = await _payMongoService.GetCheckoutSessionAsync(existingSessionId);
                    if (existingSession != null && existingSession.Status == "active")
                    {
                        return Redirect(existingSession.CheckoutUrl);
                    }
                }

                var result = await _payMongoService.CreateCheckoutSessionAsync(
                    order.OrderId,
                    invoice.TotalPrice,
                    description,
                    successUrl,
                    cancelUrl);

                if (result == null)
                {
                    TempData["ErrorMessage"] = "Failed to create payment session. (Service returned null)";
                    return RedirectToPage(new { id = id });
                }

                if (!string.IsNullOrEmpty(result.Error))
                {
                    TempData["ErrorMessage"] = $"PayMongo API Error: {result.Error}";
                    return RedirectToPage(new { id = id });
                }

                // Store session ID in ReferenceNumber temporarily to retrieve details later
                invoice.ReferenceNumber = "SESSION:" + result.SessionId;
                await _context.SaveChangesAsync();

                return Redirect(result.CheckoutUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Internal error: {ex.Message}";
                return RedirectToPage(new { id = id });
            }
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
                var invoice = order.Invoices.FirstOrDefault(i => i.PaymentStatus == "Pending" && i.ReferenceNumber != null && i.ReferenceNumber.StartsWith("SESSION:"));
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

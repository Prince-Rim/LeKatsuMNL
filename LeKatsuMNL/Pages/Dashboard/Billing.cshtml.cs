using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class BillingModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public BillingModel(LeKatsuDb context)
        {
            _context = context;
        }

        public IList<Invoice> Invoices { get; set; }

        public async Task OnGetAsync()
        {
            Invoices = await _context.Invoices
                .Include(i => i.OrderInfo)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }
    }
}

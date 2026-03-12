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
    public class RejectsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public RejectsModel(LeKatsuDb context)
        {
            _context = context;
        }

        public IList<RejectItem> RejectLogs { get; set; } = new List<RejectItem>();

        [BindProperty(SupportsGet = true)]
        public string Tab { get; set; } = "recipe";

        public async Task OnGetAsync()
        {
            if (Tab != "recipe" && Tab != "sku")
            {
                Tab = "recipe";
            }

            var query = _context.RejectItems.AsQueryable();

            if (Tab == "recipe")
            {
                query = query.Where(r => r.RejectType == "Recipe");
            }
            else if (Tab == "sku")
            {
                query = query.Where(r => r.RejectType == "SKU");
            }

            RejectLogs = await query
                .OrderByDescending(r => r.RejectedAt)
                .ToListAsync();
        }
    }
}

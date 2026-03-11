using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class BranchModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public BranchModel(LeKatsuDb context)
        {
            _context = context;
        }

        public IList<BranchLocation> Branches { get; set; } = default!;

        [BindProperty]
        public BranchLocation NewBranch { get; set; } = default!;

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Branches = await _context.BranchLocations.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                Branches = await _context.BranchLocations.ToListAsync();
                ErrorMessage = "Please make sure all required fields are filled.";
                return Page();
            }

            _context.BranchLocations.Add(NewBranch);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int BranchId, string BranchName, string BranchLocationAddress)
        {
            if (string.IsNullOrWhiteSpace(BranchName) || string.IsNullOrWhiteSpace(BranchLocationAddress))
            {
                ErrorMessage = "Branch name and address cannot be empty.";
                return RedirectToPage();
            }

            var branch = await _context.BranchLocations.FindAsync(BranchId);

            if (branch == null)
            {
                return NotFound();
            }

            branch.BranchName = BranchName;
            branch.BranchLocationAddress = BranchLocationAddress;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var branch = await _context.BranchLocations.FindAsync(id);
            if (branch != null)
            {
                _context.BranchLocations.Remove(branch);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class BranchesModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public BranchesModel(LeKatsuDb context)
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
            Branches = await _context.BranchLocations
                .Include(b => b.BranchManagers)
                .ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            // Remove BranchLocationAddress from validation since it's computed on the server
            ModelState.Remove("NewBranch.BranchLocationAddress");

            if (!ModelState.IsValid)
            {
                Branches = await _context.BranchLocations
                    .Include(b => b.BranchManagers)
                    .ToListAsync();
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                ErrorMessage = $"Validation Failed: {errors}";
                return Page();
            }


            try
            {
                // Compile address string from structured fields
                NewBranch.BranchLocationAddress = CompileAddress(NewBranch);
                NewBranch.CreatedAt = DateTime.Now;

                _context.BranchLocations.Add(NewBranch);
                await _context.SaveChangesAsync();

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                Branches = await _context.BranchLocations
                    .Include(b => b.BranchManagers)
                    .ToListAsync();
                ErrorMessage = $"Database Error: {ex.Message} {(ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "")}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEditAsync(int BranchId, string BranchName, string IslandGroup, string Region, string Province, string CityMunicipality, string Barangay, string StreetAddress, string ZipCode)
        {
            if (string.IsNullOrWhiteSpace(BranchName) || string.IsNullOrWhiteSpace(CityMunicipality) || string.IsNullOrWhiteSpace(Barangay))
            {
                ErrorMessage = "Branch name, city, and barangay cannot be empty.";
                return RedirectToPage();
            }

            var branch = await _context.BranchLocations.FindAsync(BranchId);

            if (branch == null)
            {
                return NotFound();
            }

            branch.BranchName = BranchName;
            branch.IslandGroup = IslandGroup;
            branch.Region = Region;
            branch.Province = Province;
            branch.CityMunicipality = CityMunicipality;
            branch.Barangay = Barangay;
            branch.StreetAddress = StreetAddress;
            branch.ZipCode = ZipCode;

            try
            {
                // Recompile address string
                branch.BranchLocationAddress = CompileAddress(branch);
                await _context.SaveChangesAsync();
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Update Failed: {ex.Message} {(ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "")}";
                return RedirectToPage();
            }
        }

        private string CompileAddress(BranchLocation b)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(b.StreetAddress)) parts.Add(b.StreetAddress);
            if (!string.IsNullOrWhiteSpace(b.Barangay)) parts.Add(b.Barangay);
            if (!string.IsNullOrWhiteSpace(b.CityMunicipality)) parts.Add(b.CityMunicipality);
            if (!string.IsNullOrWhiteSpace(b.Province)) parts.Add(b.Province);
            if (!string.IsNullOrWhiteSpace(b.Region)) parts.Add(b.Region);
            if (!string.IsNullOrWhiteSpace(b.IslandGroup)) parts.Add(b.IslandGroup);
            if (!string.IsNullOrWhiteSpace(b.ZipCode)) parts.Add(b.ZipCode);

            return string.Join(", ", parts);
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Helpers;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class BranchesModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public BranchesModel(LeKatsuDb context)
        {
            _context = context;
        }

        public PaginatedList<BranchLocation> Branches { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public string CurrentSortColumn { get; set; } = "Date";
        public string CurrentSortOrder { get; set; } = "desc";

        [BindProperty]
        public BranchLocation NewBranch { get; set; } = default!;

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? pageIndex, string sortColumn = null, string sortOrder = null, int? pageSize = null)
        {
            CurrentSortColumn = sortColumn ?? "Date";
            CurrentSortOrder = sortOrder ?? "desc";

            IQueryable<BranchLocation> query = _context.BranchLocations
                .Include(b => b.BranchManagers);

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var search = SearchTerm.ToLower().Trim();
                var isNumeric = int.TryParse(search, out int branchId);
                int? managerSearchId = search.StartsWith("brch-") && search.Length > 5 ? int.TryParse(search.Substring(5), out int mId) ? mId : null : null;

                query = query.Where(b => b.BranchName.ToLower().Contains(search) || 
                                       b.BranchLocationAddress.ToLower().Contains(search) ||
                                       (isNumeric && b.BranchId == branchId) ||
                                       (managerSearchId.HasValue && b.BranchManagers.Any(m => m.BManagerId == managerSearchId.Value)));
            }

            // Apply Sorting
            query = CurrentSortColumn switch
            {
                "Id" => CurrentSortOrder == "asc" ? query.OrderBy(b => b.BranchId) : query.OrderByDescending(b => b.BranchId),
                "Name" => CurrentSortOrder == "asc" ? query.OrderBy(b => b.BranchName) : query.OrderByDescending(b => b.BranchName),
                "Island" => CurrentSortOrder == "asc" ? query.OrderBy(b => b.IslandGroup) : query.OrderByDescending(b => b.IslandGroup),
                "Date" => CurrentSortOrder == "asc" ? query.OrderBy(b => b.CreatedAt) : query.OrderByDescending(b => b.CreatedAt),
                _ => query.OrderByDescending(b => b.CreatedAt)
            };

            Branches = await PaginatedList<BranchLocation>.CreateAsync(query, pageIndex ?? 1, pageSize ?? 10);
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!PermissionHelper.HasPermission(User, "Branches", 'C'))
            {
                return Forbid();
            }

            // Remove BranchLocationAddress from validation since it's computed on the server
            ModelState.Remove("NewBranch.BranchLocationAddress");

            if (!ModelState.IsValid)
            {
                var query = _context.BranchLocations
                    .Include(b => b.BranchManagers)
                    .OrderByDescending(b => b.CreatedAt);
                Branches = await PaginatedList<BranchLocation>.CreateAsync(query, 1, 10);
                
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
                var query = _context.BranchLocations
                    .Include(b => b.BranchManagers)
                    .OrderByDescending(b => b.CreatedAt);
                Branches = await PaginatedList<BranchLocation>.CreateAsync(query, 1, 10);

                ErrorMessage = $"Database Error: {ex.Message} {(ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "")}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEditAsync(int BranchId, string BranchName, string IslandGroup, string Region, string Province, string CityMunicipality, string Barangay, string StreetAddress, string ZipCode)
        {
            if (!PermissionHelper.HasPermission(User, "Branches", 'U'))
            {
                return Forbid();
            }

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
            if (!PermissionHelper.HasPermission(User, "Branches", 'D'))
            {
                return Forbid();
            }

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

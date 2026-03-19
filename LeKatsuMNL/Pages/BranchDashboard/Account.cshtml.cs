using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Pages.BranchDashboard
{
    public class AccountModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public AccountModel(LeKatsuDb context)
        {
            _context = context;
        }

        public BranchManager Manager { get; set; }

        [BindProperty]
        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [BindProperty]
        [MaxLength(50)]
        public string MiddleName { get; set; }

        [BindProperty]
        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [BindProperty]
        [MaxLength(255), EmailAddress]
        public string Email { get; set; }

        [BindProperty]
        [MaxLength(20)]
        public string ContactNum { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        private async Task<BranchManager> GetManagerAsync()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int id)) return null;

            return await _context.BranchManagers
                .Include(b => b.BranchLocation)
                .FirstOrDefaultAsync(b => b.BManagerId == id && !b.IsArchived);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login/login");

            Manager = await GetManagerAsync();
            if (Manager == null) return RedirectToPage("/Login/login");

            // Pre-fill bound properties
            FirstName  = Manager.FirstName;
            MiddleName = Manager.MiddleName;
            LastName   = Manager.LastName;
            Email      = Manager.Email;
            ContactNum = Manager.ContactNum;

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            Manager = await GetManagerAsync();
            if (Manager == null) return RedirectToPage("/Login/login");

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors below.";
                return Page();
            }

            Manager.FirstName  = FirstName?.Trim();
            Manager.MiddleName = MiddleName?.Trim();
            Manager.LastName   = LastName?.Trim();
            Manager.Email      = Email?.Trim();
            Manager.ContactNum = ContactNum?.Trim();

            await _context.SaveChangesAsync();
            SuccessMessage = "Account updated successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            Manager = await GetManagerAsync();
            if (Manager == null) return RedirectToPage("/Login/login");

            Manager.IsArchived = true;
            await _context.SaveChangesAsync();

            // Sign out
            return RedirectToPage("/Login/login", new { handler = "Logout" });
        }
    }
}

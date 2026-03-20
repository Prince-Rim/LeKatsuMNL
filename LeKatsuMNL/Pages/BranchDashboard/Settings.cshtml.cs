using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LeKatsuMNL.Pages.BranchDashboard
{
    public class SettingsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public SettingsModel(LeKatsuDb context)
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

        public string UserSystemId { get; set; }

        [BindProperty]
        public string CurrentPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmNewPassword { get; set; }

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

            FirstName = Manager.FirstName;
            MiddleName = Manager.MiddleName;
            LastName = Manager.LastName;
            Email = Manager.Email;
            ContactNum = Manager.ContactNum;
            UserSystemId = "BRCH-" + Manager.BManagerId.ToString("D4");

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAccountAsync()
        {
            Manager = await GetManagerAsync();
            if (Manager == null) return RedirectToPage("/Login/login");

            if (!ModelState.IsValid)
            {
                UserSystemId = "BRCH-" + Manager.BManagerId.ToString("D4");
                ErrorMessage = "Please correct the errors below.";
                return Page();
            }

            Manager.FirstName = FirstName?.Trim();
            Manager.MiddleName = MiddleName?.Trim();
            Manager.LastName = LastName?.Trim();
            Manager.Email = Email?.Trim();
            Manager.ContactNum = ContactNum?.Trim();

            await _context.SaveChangesAsync();
            SuccessMessage = "Account updated successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            Manager = await GetManagerAsync();
            if (Manager == null) return RedirectToPage("/Login/login");

            // Repopulate user info for Page() return
            UserSystemId = "BRCH-" + Manager.BManagerId.ToString("D4");

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ErrorMessage = "New password cannot be empty.";
                return Page();
            }

            if (NewPassword != ConfirmNewPassword)
            {
                ErrorMessage = "New passwords do not match.";
                return Page();
            }

            if (string.IsNullOrEmpty(CurrentPassword) || !BCrypt.Net.BCrypt.Verify(CurrentPassword, Manager.Password))
            {
                ErrorMessage = "Incorrect current password.";
                return Page();
            }

            // Update password
            Manager.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            await _context.SaveChangesAsync();
            
            SuccessMessage = "Password updated successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync()
        {
            Manager = await GetManagerAsync();
            if (Manager == null) return RedirectToPage("/Login/login");

            Manager.IsArchived = true;
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Login/login");
        }
    }
}

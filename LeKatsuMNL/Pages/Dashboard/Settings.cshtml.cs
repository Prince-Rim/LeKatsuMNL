using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using Microsoft.EntityFrameworkCore;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class SettingsModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public SettingsModel(LeKatsuDb context)
        {
            _context = context;
        }

        [BindProperty]
        public string FirstName { get; set; }
        [BindProperty]
        public string LastName { get; set; }
        [BindProperty]
        public string MiddleName { get; set; }
        
        public string Role { get; set; }
        
        [BindProperty]
        public string Email { get; set; }
        public string UserSystemId { get; set; }
        [BindProperty]
        public string CurrentPassword { get; set; }
        [BindProperty]
        public string NewPassword { get; set; }
        [BindProperty]
        public string ConfirmNewPassword { get; set; }
        
        public string Privileges { get; set; }
        public string AccountType { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login/login");
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return RedirectToPage("/Login/login");
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            Role = userRole;

            if (userRole == "Admin")
            {
                var admin = await _context.AdminAccounts.FirstOrDefaultAsync(a => a.ManagerId == userId);
                if (admin != null)
                {
                    FirstName = admin.FirstName;
                    LastName = admin.LastName;
                    Email = admin.Email;
                    MiddleName = admin.MiddleName;
                    UserSystemId = "ADM-" + admin.ManagerId.ToString("D4");
                    Privileges = admin.Privileges;
                    AccountType = "Admin";
                }
            }
            else if (userRole == "BranchManager")
            {
                var manager = await _context.BranchManagers.FirstOrDefaultAsync(m => m.BManagerId == userId);
                if (manager != null)
                {
                    FirstName = manager.FirstName;
                    LastName = manager.LastName;
                    Email = manager.Email;
                    MiddleName = manager.MiddleName;
                    UserSystemId = "BRCH-" + manager.BManagerId.ToString("D4");
                    Privileges = "N/A";
                    Role = "Branch Manager";
                    AccountType = "BranchManager";
                }
            }
            else if (userRole == "Staff")
            {
                var staff = await _context.StaffInformations.FirstOrDefaultAsync(s => s.StaffId == userId);
                if (staff != null)
                {
                    FirstName = staff.FirstName;
                    LastName = staff.LastName;
                    Email = staff.Email ?? "N/A";
                    MiddleName = staff.MiddleName;
                    UserSystemId = "STF-" + staff.StaffId.ToString("D4");
                    Privileges = "N/A";
                    AccountType = "Staff";
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login/login");

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return RedirectToPage("/Login/login");
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Admin")
            {
                var admin = await _context.AdminAccounts.FindAsync(userId);
                if (admin != null && !admin.IsSuperAdmin)
                {
                    _context.AdminAccounts.Remove(admin);
                }
            }
            else if (userRole == "BranchManager")
            {
                var manager = await _context.BranchManagers.FindAsync(userId);
                if (manager != null)
                {
                    _context.BranchManagers.Remove(manager);
                }
            }
            else if (userRole == "Staff")
            {
                var staff = await _context.StaffInformations.FindAsync(userId);
                if (staff != null)
                {
                    _context.StaffInformations.Remove(staff);
                }
            }

            await _context.SaveChangesAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Login/login");
        }

        public async Task<IActionResult> OnPostEditAccountAsync()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login/login");

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return RedirectToPage("/Login/login");
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Admin")
            {
                var admin = await _context.AdminAccounts.FindAsync(userId);
                if (admin != null)
                {
                    admin.FirstName = FirstName;
                    admin.LastName = LastName;
                    admin.MiddleName = MiddleName;
                    admin.Email = Email;
                }
            }
            else if (userRole == "BranchManager")
            {
                var manager = await _context.BranchManagers.FindAsync(userId);
                if (manager != null)
                {
                    manager.FirstName = FirstName;
                    manager.LastName = LastName;
                    manager.MiddleName = MiddleName;
                    manager.Email = Email;
                }
            }
            else if (userRole == "Staff")
            {
                var staff = await _context.StaffInformations.FindAsync(userId);
                if (staff != null)
                {
                    staff.FirstName = FirstName;
                    staff.LastName = LastName;
                    staff.MiddleName = MiddleName;
                    staff.Email = Email;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/Dashboard/Settings");
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login/login");

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return RedirectToPage("/Login/login");
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (NewPassword != ConfirmNewPassword)
            {
                TempData["ErrorMessage"] = "New passwords do not match.";
                return RedirectToPage();
            }

            string storedHash = null;
            object userEntity = null;

            if (userRole == "Admin")
            {
                var admin = await _context.AdminAccounts.FindAsync(userId);
                if (admin != null)
                {
                    storedHash = admin.Password;
                    userEntity = admin;
                }
            }
            else if (userRole == "BranchManager")
            {
                var manager = await _context.BranchManagers.FindAsync(userId);
                if (manager != null)
                {
                    storedHash = manager.Password;
                    userEntity = manager;
                }
            }
            else if (userRole == "Staff")
            {
                var staff = await _context.StaffInformations.FindAsync(userId);
                if (staff != null)
                {
                    storedHash = staff.Password;
                    userEntity = staff;
                }
            }

            if (userEntity == null || string.IsNullOrEmpty(storedHash) || !BCrypt.Net.BCrypt.Verify(CurrentPassword, storedHash))
            {
                TempData["ErrorMessage"] = "Incorrect current password.";
                return RedirectToPage();
            }

            // Update password
            string newHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            if (userEntity is AdminAccount a) a.Password = newHash;
            else if (userEntity is BranchManager m) m.Password = newHash;
            else if (userEntity is StaffInformation s) s.Password = newHash;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Password updated successfully.";
            return RedirectToPage();
        }
    }
}

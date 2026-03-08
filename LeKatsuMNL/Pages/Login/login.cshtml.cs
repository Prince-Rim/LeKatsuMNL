using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using LeKatsuMNL.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LeKatsuMNL.Pages.Login
{
    public class loginModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public loginModel(LeKatsuDb context)
        {
            _context = context;
        }

        [BindProperty]
        public string UserId { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                Response.Redirect("/Dashboard");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!int.TryParse(UserId, out int numericId))
            {
                ErrorMessage = "Invalid User ID format.";
                return Page();
            }

            // 1. Check AdminAccount
            var admin = await _context.AdminAccounts.FirstOrDefaultAsync(a => a.ManagerId == numericId);
            if (admin != null)
            {
                if (BCrypt.Net.BCrypt.Verify(Password, admin.Password))
                {
                    await SignInUserAsync(admin.ManagerId.ToString(), "Admin", admin.FirstName + " " + admin.LastName);
                    return RedirectToPage("/Dashboard/Index");
                }
            }
            else
            {
                // 2. Check BranchManager
                var manager = await _context.BranchManagers.FirstOrDefaultAsync(m => m.BManagerId == numericId);
                if (manager != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(Password, manager.Password))
                    {
                        await SignInUserAsync(manager.BManagerId.ToString(), "BranchManager", manager.FirstName + " " + manager.LastName);
                        return RedirectToPage("/Dashboard/Index");
                    }
                }
                else
                {
                    // 3. Check StaffInformation
                    var staff = await _context.StaffInformations.FirstOrDefaultAsync(s => s.StaffId == numericId);
                    if (staff != null)
                    {
                        if (BCrypt.Net.BCrypt.Verify(Password, staff.Password))
                        {
                            await SignInUserAsync(staff.StaffId.ToString(), "Staff", staff.FirstName + " " + staff.LastName);
                            return RedirectToPage("/Dashboard/Index");
                        }
                    }
                }
            }

            ErrorMessage = "Invalid User ID or Password.";
            return Page();
        }

        private async Task SignInUserAsync(string id, string role, string name)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        }
    }
}

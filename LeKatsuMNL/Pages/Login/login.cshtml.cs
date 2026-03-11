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

            // Attempt to parse out prefixes like "ADM-", "BM-", "BRCH-", "STF-"
            string rawId = UserId.Trim().ToUpper();
            int numericId;
            string userType = null; // Used to optimize which table we check

            if (rawId.StartsWith("ADM-"))
            {
                userType = "Admin";
                rawId = rawId.Substring(4);
            }
            else if (rawId.StartsWith("BRCH-"))
            {
                userType = "BranchManager";
                rawId = rawId.Substring(5);
            }
            else if (rawId.StartsWith("BM-"))
            {
                userType = "BranchManager";
                rawId = rawId.Substring(3); // Support legacy BM- prefix
            }
            else if (rawId.StartsWith("STF-"))
            {
                userType = "Staff";
                rawId = rawId.Substring(4);
            }

            // Remove any leading zeroes: "0007" becomes "7" before parsing
            if (!int.TryParse(rawId, out numericId))
            {
                ErrorMessage = "Invalid User ID format.";
                return Page();
            }

            // 1. Check AdminAccount (If no prefix supplied, or if ADM- supplied)
            if (userType == null || userType == "Admin")
            {
                var admin = await _context.AdminAccounts.FirstOrDefaultAsync(a => a.ManagerId == numericId);
                if (admin != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(Password, admin.Password))
                    {
                        if (admin.Status?.ToLower() != "active")
                        {
                            ErrorMessage = "Account is deactivated.";
                            return Page();
                        }
                        
                        await SignInUserAsync(admin.ManagerId.ToString(), "Admin", admin.FirstName + " " + admin.LastName);
                        return RedirectToPage("/Dashboard/Index");
                    }
                }
            }

            // 2. Check BranchManager (If no prefix supplied, or if BRCH-/BM- supplied)
            if (userType == null || userType == "BranchManager")
            {
                var manager = await _context.BranchManagers.FirstOrDefaultAsync(m => m.BManagerId == numericId);
                if (manager != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(Password, manager.Password))
                    {
                        if (manager.Status?.ToLower() != "active")
                        {
                            ErrorMessage = "Account is deactivated.";
                            return Page();
                        }

                        await SignInUserAsync(manager.BManagerId.ToString(), "BranchManager", manager.FirstName + " " + manager.LastName);
                        return RedirectToPage("/Dashboard/Index");
                    }
                }
            }

            // 3. Check StaffInformation (If no prefix supplied, or if STF- supplied)
            if (userType == null || userType == "Staff")
            {
                var staff = await _context.StaffInformations.FirstOrDefaultAsync(s => s.StaffId == numericId);
                if (staff != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(Password, staff.Password))
                    {
                        if (staff.Status?.ToLower() != "active")
                        {
                            ErrorMessage = "Account is deactivated.";
                            return Page();
                        }

                        await SignInUserAsync(staff.StaffId.ToString(), "Staff", staff.FirstName + " " + staff.LastName);
                        return RedirectToPage("/Dashboard/Index");
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

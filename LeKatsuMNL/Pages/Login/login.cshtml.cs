using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using LeKatsuMNL.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

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

            string rawId = UserId.Trim().ToUpper();
            int numericId;
            string userType = null;

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
                rawId = rawId.Substring(3);
            }
            else if (rawId.StartsWith("STF-"))
            {
                userType = "Staff";
                rawId = rawId.Substring(4);
            }

            if (userType == null)
            {
                ErrorMessage = "Invalid User ID or Password.";
                return Page();
            }

            if (!int.TryParse(rawId, out numericId))
            {
                ErrorMessage = "Invalid User ID or Password.";
                return Page();
            }

            // 1. Check AdminAccount
            if (userType == "Admin")
            {
                var admin = await _context.AdminAccounts.FirstOrDefaultAsync(a => a.ManagerId == numericId && !a.IsArchived);
                if (admin != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(Password, admin.Password))
                    {
                        if (admin.Status?.ToLower() != "active")
                        {
                            ErrorMessage = "Invalid User ID or Password.";
                            return Page();
                        }
                        
                        string privileges = admin.IsSuperAdmin ? "All" : admin.Privileges;
                        await SignInUserAsync(admin.ManagerId.ToString(), "Admin", admin.FirstName + " " + admin.LastName, privileges);
                        return RedirectToPage("/Dashboard/Index");
                    }
                }
            }

            // 2. Check BranchManager
            if (userType == "BranchManager")
            {
                var manager = await _context.BranchManagers
                    .Include(m => m.BranchLocation)
                    .FirstOrDefaultAsync(m => m.BManagerId == numericId && !m.IsArchived);
                    
                if (manager != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(Password, manager.Password))
                    {
                        if (manager.Status?.ToLower() != "active")
                        {
                            ErrorMessage = "Invalid User ID or Password.";
                            return Page();
                        }

                        await SignInUserAsync(manager.BManagerId.ToString(), "BranchManager", manager.FirstName + " " + manager.LastName, "All", manager.BranchId.ToString(), manager.BranchLocation?.BranchName);
                        return RedirectToPage("/BranchDashboard/Index");
                    }
                }
            }

            // 3. Check StaffInformation
            if (userType == "Staff")
            {
                var staff = await _context.StaffInformations.FirstOrDefaultAsync(s => s.StaffId == numericId && !s.IsArchived);
                if (staff != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(Password, staff.Password))
                    {
                        if (staff.Status?.ToLower() != "active")
                        {
                            ErrorMessage = "Invalid User ID or Password.";
                            return Page();
                        }

                        await SignInUserAsync(staff.StaffId.ToString(), "Staff", staff.FirstName + " " + staff.LastName, "All");
                        return RedirectToPage("/BranchDashboard/Index");
                    }
                }
            }

            ErrorMessage = "Invalid User ID or Password.";
            return Page();
        }

        private async Task SignInUserAsync(string id, string role, string name, string privileges, string branchId = null, string branchName = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role),
                new Claim("Permissions", privileges ?? ""),
                new Claim("BranchId", branchId ?? ""),
                new Claim("BranchName", branchName ?? "")
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

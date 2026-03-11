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
    public class UsersModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public UsersModel(LeKatsuDb context)
        {
            _context = context;
        }

        public class UserViewModel
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public string UserSystemId { get; set; }
            public string Email { get; set; }
            public string ContactNum { get; set; }
            public string Role { get; set; }
            public string Status { get; set; }
            public string BranchName { get; set; }
            public string Type { get; set; } // "Admin" or "Manager"
        }

        public IList<UserViewModel> AllUsers { get; set; }
        public IList<BranchLocation> Branches { get; set; }

        [BindProperty]
        public UserInputModel NewUser { get; set; } = new();

        [BindProperty]
        public UserInputModel EditUser { get; set; } = new();

        public class UserInputModel
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string ContactNum { get; set; }
            public string Password { get; set; }
            public string Role { get; set; } // "Admin", "Branch Manager", "Staff", etc.
            public int? BranchId { get; set; }
            public string Privileges { get; set; }
            public string Status { get; set; } = "Active";
            public string Type { get; set; } // "Admin" or "Manager"
        }

        public async Task OnGetAsync()
        {
            Branches = await _context.BranchLocations.ToListAsync();
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            var admins = await _context.AdminAccounts
                .Where(a => !a.IsSuperAdmin)
                .Select(a => new UserViewModel
                {
                    Id = a.ManagerId,
                    FirstName = a.FirstName,
                    MiddleName = a.MiddleName,
                    LastName = a.LastName,
                    UserSystemId = "ADM-" + a.ManagerId.ToString("D4"),
                    Email = a.Email,
                    Role = a.Privileges,
                    Status = a.Status,
                    BranchName = "Main / All",
                    Type = "Admin"
                }).ToListAsync();

            var managers = await _context.BranchManagers
                .Include(m => m.BranchLocation)
                .Select(m => new UserViewModel
                {
                    Id = m.BManagerId,
                    FirstName = m.FirstName,
                    MiddleName = m.MiddleName,
                    LastName = m.LastName,
                    UserSystemId = "BM-" + m.BManagerId.ToString("D4"),
                    Email = m.Email,
                    ContactNum = m.ContactNum,
                    Role = "Branch Manager",
                    Status = m.Status,
                    BranchName = m.BranchLocation != null ? m.BranchLocation.BranchName : "N/A",
                    Type = "Manager"
                }).ToListAsync();

            AllUsers = admins.Concat(managers).OrderBy(u => u.LastName).ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (NewUser.Role == "Branch Manager")
            {
                if (!NewUser.BranchId.HasValue || NewUser.BranchId <= 0)
                {
                    // Fallback to avoid FK constraint error.
                    // Ideally handled via UI validation.
                    var firstBranch = await _context.BranchLocations.FirstOrDefaultAsync();
                    if (firstBranch != null) NewUser.BranchId = firstBranch.BranchId;
                }

                var manager = new BranchManager
                {
                    FirstName = NewUser.FirstName,
                    MiddleName = NewUser.MiddleName ?? "",
                    LastName = NewUser.LastName,
                    Email = NewUser.Email,
                    ContactNum = NewUser.ContactNum ?? "N/A",
                    Password = NewUser.Password, // In a real app, hash this
                    BranchId = NewUser.BranchId ?? 0,
                    Status = "Active"
                };
                _context.BranchManagers.Add(manager);
            }
            else
            {
                var admin = new AdminAccount
                {
                    FirstName = NewUser.FirstName,
                    MiddleName = NewUser.MiddleName ?? "",
                    LastName = NewUser.LastName,
                    Email = NewUser.Email,
                    Password = NewUser.Password, // In a real app, hash this
                    Privileges = NewUser.Role == "admin" ? (NewUser.Privileges ?? "Admin") : "Staff",
                    Status = "Active",
                    IsSuperAdmin = false
                };
                _context.AdminAccounts.Add(admin);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (EditUser.Type == "Admin")
            {
                var admin = await _context.AdminAccounts.FindAsync(EditUser.Id);
                if (admin != null)
                {
                    admin.FirstName = EditUser.FirstName;
                    admin.MiddleName = EditUser.MiddleName ?? "";
                    admin.LastName = EditUser.LastName;
                    admin.Email = EditUser.Email;
                    if (EditUser.Role == "admin") {
                        admin.Privileges = EditUser.Privileges ?? admin.Privileges;
                    } else if (EditUser.Role == "Staff") {
                        admin.Privileges = "Staff";
                    }
                    admin.Status = EditUser.Status;
                    if (!string.IsNullOrEmpty(EditUser.Password)) admin.Password = EditUser.Password;
                }
            }
            else
            {
                var manager = await _context.BranchManagers.FindAsync(EditUser.Id);
                if (manager != null)
                {
                    manager.FirstName = EditUser.FirstName;
                    manager.MiddleName = EditUser.MiddleName ?? "";
                    manager.LastName = EditUser.LastName;
                    manager.Email = EditUser.Email;
                    manager.ContactNum = EditUser.ContactNum ?? manager.ContactNum;
                    
                    if (EditUser.BranchId.HasValue && EditUser.BranchId > 0)
                    {
                        manager.BranchId = EditUser.BranchId.Value;
                    }

                    manager.Status = EditUser.Status;
                    if (!string.IsNullOrEmpty(EditUser.Password)) manager.Password = EditUser.Password;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, string type)
        {
            if (type == "Admin")
            {
                var admin = await _context.AdminAccounts.FindAsync(id);
                if (admin != null) _context.AdminAccounts.Remove(admin);
            }
            else
            {
                var manager = await _context.BranchManagers.FindAsync(id);
                if (manager != null) _context.BranchManagers.Remove(manager);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using LeKatsuMNL.Helpers;

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
            public string Privileges { get; set; }
            public string Status { get; set; }
            public string BranchName { get; set; }
            public string Type { get; set; } // "Admin" or "Manager"
        }

        public PaginatedList<UserViewModel> AllUsers { get; set; }
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

        public async Task OnGetAsync(int? pageIndex)
        {
            Branches = await _context.BranchLocations.ToListAsync();
            await LoadUsersAsync(pageIndex ?? 1);
        }

        private async Task LoadUsersAsync(int pageIndex)
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
                    Role = a.Role,
                    Privileges = a.Privileges,
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
                    UserSystemId = "BRCH-" + m.BManagerId.ToString("D4"),
                    Email = m.Email,
                    ContactNum = m.ContactNum,
                    Role = m.Role,
                    Privileges = "N/A", // Managers no longer have granular privileges
                    Status = m.Status,
                    BranchName = m.BranchLocation != null ? m.BranchLocation.BranchName : "N/A",
                    Type = "Manager"
                }).ToListAsync();

            var staff = await _context.StaffInformations
                .Select(s => new UserViewModel
                {
                    Id = s.StaffId,
                    FirstName = s.FirstName,
                    MiddleName = s.MiddleName,
                    LastName = s.LastName,
                    UserSystemId = "STF-" + s.StaffId.ToString("D4"),
                    Email = s.Email,
                    ContactNum = s.ContactNum,
                    Role = "Staff",
                    Privileges = "N/A",
                    Status = s.Status,
                    BranchName = "N/A",
                    Type = "Staff"
                }).ToListAsync();

            var combined = admins.Concat(managers).Concat(staff).OrderBy(u => u.LastName);
            AllUsers = PaginatedList<UserViewModel>.Create(combined.AsQueryable(), pageIndex, 10);
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!PermissionHelper.HasPermission(User, "Users", 'C')) return Forbid();

            if (NewUser.Role == "Branch Manager")
            {
                if (!NewUser.BranchId.HasValue || NewUser.BranchId <= 0)
                {
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
                    Password = BCrypt.Net.BCrypt.HashPassword(NewUser.Password),
                    BranchId = NewUser.BranchId ?? 0,
                    Status = "Active",
                    Role = "Branch Manager"
                };
                _context.BranchManagers.Add(manager);
            }
            else if (NewUser.Role == "Staff")
            {
                var staff = new StaffInformation
                {
                    FirstName = NewUser.FirstName,
                    MiddleName = NewUser.MiddleName ?? "",
                    LastName = NewUser.LastName,
                    Email = NewUser.Email,
                    ContactNum = NewUser.ContactNum ?? "N/A",
                    Password = BCrypt.Net.BCrypt.HashPassword(NewUser.Password),
                    Status = "Active"
                };
                _context.StaffInformations.Add(staff);
            }
            else
            {
                var admin = new AdminAccount
                {
                    FirstName = NewUser.FirstName,
                    MiddleName = NewUser.MiddleName ?? "",
                    LastName = NewUser.LastName,
                    Email = NewUser.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(NewUser.Password),
                    Role = NewUser.Role,
                    Privileges = NewUser.Privileges ?? "N/A",
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
            if (!PermissionHelper.HasPermission(User, "Users", 'U')) return Forbid();

            if (EditUser.Type == "Admin")
            {
                var admin = await _context.AdminAccounts.FindAsync(EditUser.Id);
                if (admin != null)
                {
                    admin.FirstName = EditUser.FirstName;
                    admin.MiddleName = EditUser.MiddleName ?? "";
                    admin.LastName = EditUser.LastName;
                    admin.Email = EditUser.Email;
                    admin.Role = EditUser.Role;
                    admin.Privileges = EditUser.Privileges ?? admin.Privileges;
                    admin.Status = EditUser.Status;
                    if (!string.IsNullOrEmpty(EditUser.Password)) admin.Password = BCrypt.Net.BCrypt.HashPassword(EditUser.Password);
                }
            }
            else if (EditUser.Type == "Manager")
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
                    if (!string.IsNullOrEmpty(EditUser.Password)) manager.Password = BCrypt.Net.BCrypt.HashPassword(EditUser.Password);
                }
            }
            else if (EditUser.Type == "Staff")
            {
                var staff = await _context.StaffInformations.FindAsync(EditUser.Id);
                if (staff != null)
                {
                    staff.FirstName = EditUser.FirstName;
                    staff.MiddleName = EditUser.MiddleName ?? "";
                    staff.LastName = EditUser.LastName;
                    staff.Email = EditUser.Email;
                    staff.ContactNum = EditUser.ContactNum ?? staff.ContactNum;
                    staff.Status = EditUser.Status;
                    if (!string.IsNullOrEmpty(EditUser.Password)) staff.Password = BCrypt.Net.BCrypt.HashPassword(EditUser.Password);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id, string type)
        {
            if (!PermissionHelper.HasPermission(User, "Users", 'U')) return Forbid();

            if (type == "Admin")
            {
                var admin = await _context.AdminAccounts.FindAsync(id);
                if (admin != null)
                {
                    admin.Status = admin.Status.ToLower() == "active" ? "Deactivated" : "Active";
                }
            }
            else if (type == "Manager")
            {
                var manager = await _context.BranchManagers.FindAsync(id);
                if (manager != null)
                {
                    manager.Status = manager.Status.ToLower() == "active" ? "Deactivated" : "Active";
                }
            }
            else if (type == "Staff")
            {
                var staff = await _context.StaffInformations.FindAsync(id);
                if (staff != null)
                {
                    staff.Status = staff.Status.ToLower() == "active" ? "Deactivated" : "Active";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, string type)
        {
            if (!PermissionHelper.HasPermission(User, "Users", 'D')) return Forbid();

            if (type == "Admin")
            {
                var admin = await _context.AdminAccounts.FindAsync(id);
                if (admin != null) _context.AdminAccounts.Remove(admin);
            }
            else if (type == "Manager")
            {
                var manager = await _context.BranchManagers.FindAsync(id);
                if (manager != null) _context.BranchManagers.Remove(manager);
            }
            else if (type == "Staff")
            {
                var staff = await _context.StaffInformations.FindAsync(id);
                if (staff != null) _context.StaffInformations.Remove(staff);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}

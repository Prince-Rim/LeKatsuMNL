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

        public string SearchString { get; set; }
        public string RoleFilter { get; set; }
        public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

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

        public async Task OnGetAsync(int? pageIndex, string searchString, string roleFilter, string statusFilter)
        {
            SearchString = searchString;
            RoleFilter = roleFilter;
            StatusFilter = statusFilter;

            Branches = await _context.BranchLocations.ToListAsync();
            int currentStepPageIndex = pageIndex ?? 1;
            await LoadUsersAsync(currentStepPageIndex);
        }

        private async Task LoadUsersAsync(int pageIndex)
        {
            var adminQuery = _context.AdminAccounts.Where(a => !a.IsSuperAdmin && !a.IsArchived);
            var managerQuery = _context.BranchManagers.Where(m => !m.IsArchived).Include(m => m.BranchLocation).AsQueryable();
            var staffQuery = _context.StaffInformations.Where(s => !s.IsArchived).AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
            {
                var search = SearchString.ToLower();
                
                // Support searching by System ID (e.g., ADM-0001, BRCH-0001, STF-0001)
                int? adminSearchId = (search.StartsWith("adm-") && int.TryParse(search.Substring(4), out int aId)) ? aId : (int.TryParse(search, out int aId2) ? aId2 : (int?)null);
                int? managerSearchId = (search.StartsWith("brch-") && int.TryParse(search.Substring(5), out int mId)) ? mId : (int.TryParse(search, out int mId2) ? mId2 : (int?)null);
                int? staffSearchId = (search.StartsWith("stf-") && int.TryParse(search.Substring(4), out int sId)) ? sId : (int.TryParse(search, out int sId2) ? sId2 : (int?)null);

                adminQuery = adminQuery.Where(a => 
                    a.FirstName.ToLower().Contains(search) || 
                    a.LastName.ToLower().Contains(search) || 
                    a.Email.ToLower().Contains(search) ||
                    (adminSearchId.HasValue && a.ManagerId == adminSearchId.Value));

                managerQuery = managerQuery.Where(m => 
                    m.FirstName.ToLower().Contains(search) || 
                    m.LastName.ToLower().Contains(search) || 
                    m.Email.ToLower().Contains(search) || 
                    (m.BranchLocation != null && m.BranchLocation.BranchName.ToLower().Contains(search)) ||
                    (managerSearchId.HasValue && m.BManagerId == managerSearchId.Value));

                staffQuery = staffQuery.Where(s => 
                    s.FirstName.ToLower().Contains(search) || 
                    s.LastName.ToLower().Contains(search) || 
                    s.Email.ToLower().Contains(search) ||
                    (staffSearchId.HasValue && s.StaffId == staffSearchId.Value));
            }

            if (!string.IsNullOrEmpty(StatusFilter) && StatusFilter != "All")
            {
                adminQuery = adminQuery.Where(a => a.Status == StatusFilter);
                managerQuery = managerQuery.Where(m => m.Status == StatusFilter);
                staffQuery = staffQuery.Where(s => s.Status == StatusFilter);
            }

            IEnumerable<UserViewModel> admins = new List<UserViewModel>();
            IEnumerable<UserViewModel> managers = new List<UserViewModel>();
            IEnumerable<UserViewModel> staff = new List<UserViewModel>();

            if (string.IsNullOrEmpty(RoleFilter) || RoleFilter == "All" || RoleFilter == "Admin")
            {
                admins = await adminQuery.Select(a => new UserViewModel
                {
                    Id = a.ManagerId,
                    FirstName = a.FirstName,
                    MiddleName = a.MiddleName,
                    LastName = a.LastName,
                    UserSystemId = "ADM-" + a.ManagerId.ToString("D4"),
                    Email = a.Email,
                    ContactNum = a.ContactNum,
                    Role = a.Role,
                    Privileges = a.Privileges,
                    Status = a.Status,
                    BranchName = "Main / All",
                    Type = "Admin"
                }).ToListAsync();
            }

            if (string.IsNullOrEmpty(RoleFilter) || RoleFilter == "All" || RoleFilter == "Branch Manager")
            {
                managers = await managerQuery.Select(m => new UserViewModel
                {
                    Id = m.BManagerId,
                    FirstName = m.FirstName,
                    MiddleName = m.MiddleName,
                    LastName = m.LastName,
                    UserSystemId = "BRCH-" + m.BManagerId.ToString("D4"),
                    Email = m.Email,
                    ContactNum = m.ContactNum,
                    Role = m.Role,
                    Privileges = "N/A",
                    Status = m.Status,
                    BranchName = m.BranchLocation != null ? m.BranchLocation.BranchName : "N/A",
                    Type = "Manager"
                }).ToListAsync();
            }

            if (string.IsNullOrEmpty(RoleFilter) || RoleFilter == "All" || RoleFilter == "Staff")
            {
                staff = await staffQuery.Select(s => new UserViewModel
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
            }

            var combined = admins.Concat(managers).Concat(staff).OrderBy(u => u.LastName);
            int pageSize = PageSize > 0 ? PageSize : 10;
            AllUsers = PaginatedList<UserViewModel>.Create(combined.AsQueryable(), pageIndex, pageSize);
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
                    ContactNum = NewUser.ContactNum,
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
                    admin.ContactNum = EditUser.ContactNum;
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

        public async Task<IActionResult> OnPostArchiveAsync(int id, string type)
        {
            if (!PermissionHelper.HasPermission(User, "Users", 'D')) return Forbid();

            if (type == "Admin")
            {
                var admin = await _context.AdminAccounts.FindAsync(id);
                if (admin != null) admin.IsArchived = true;
            }
            else if (type == "Manager")
            {
                var manager = await _context.BranchManagers.FindAsync(id);
                if (manager != null) manager.IsArchived = true;
            }
            else if (type == "Staff")
            {
                var staff = await _context.StaffInformations.FindAsync(id);
                if (staff != null) staff.IsArchived = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBulkArchiveAsync(string ids)
        {
            if (!PermissionHelper.HasPermission(User, "Users", 'D')) return Forbid();
            if (string.IsNullOrEmpty(ids)) return RedirectToPage();

            var idList = ids.Split(',');
            foreach (var idStr in idList)
            {
                var parts = idStr.Split(':'); // Expected format "id:type"
                if (parts.Length != 2) continue;
                
                int id = int.Parse(parts[0]);
                string type = parts[1];

                if (type == "Admin")
                {
                    var admin = await _context.AdminAccounts.FindAsync(id);
                    if (admin != null) admin.IsArchived = true;
                }
                else if (type == "Manager")
                {
                    var manager = await _context.BranchManagers.FindAsync(id);
                    if (manager != null) manager.IsArchived = true;
                }
                else if (type == "Staff")
                {
                    var staff = await _context.StaffInformations.FindAsync(id);
                    if (staff != null) staff.IsArchived = true;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}

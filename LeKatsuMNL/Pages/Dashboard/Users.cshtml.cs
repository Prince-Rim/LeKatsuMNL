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

        public string OpenModal { get; set; }

        [BindProperty]
        public int CurrentPageIndex { get; set; } = 1;

        public class UserInputModel
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string ContactNum { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
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

            Branches = await _context.BranchLocations.Where(b => !b.IsArchived).ToListAsync();
            CurrentPageIndex = pageIndex ?? 1;
            await LoadUsersAsync(CurrentPageIndex);
        }

        private async Task LoadUsersAsync(int pageIndex)
        {
            var adminQuery = _context.AdminAccounts.Where(a => !a.IsSuperAdmin && !a.IsArchived);
            var managerQuery = _context.BranchManagers.Where(m => !m.IsArchived).Include(m => m.BranchLocation).AsQueryable();
            var staffQuery = _context.StaffInformations.Where(s => !s.IsArchived).AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
            {
                var search = SearchString.ToLower();
                
                // Support searching by System ID (e.g., ADM-0001, BRCH-0001, STF-0001, or USR-0001)
                int? parsedId = null;
                string idStr = "";
                bool isAdminOnly = false;
                bool isManagerOnly = false;
                bool isStaffOnly = false;

                if (search.Contains("-"))
                {
                    var parts = search.Split('-');
                    var prefix = parts[0].ToLower();
                    var idPart = parts.Last();
                    if (int.TryParse(idPart, out int id))
                    {
                        parsedId = id;
                        idStr = idPart.TrimStart('0'); // Search by the actual number part
                        if (string.IsNullOrEmpty(idStr) && id == 0) idStr = "0";

                        if (prefix == "adm") isAdminOnly = true;
                        else if (prefix == "brch") isManagerOnly = true;
                        else if (prefix == "stf") isStaffOnly = true;
                    }
                }
                else if (int.TryParse(search, out int id))
                {
                    parsedId = id;
                    idStr = search.TrimStart('0');
                    if (string.IsNullOrEmpty(idStr) && id == 0) idStr = "0";
                }

                adminQuery = adminQuery.Where(a => 
                    a.FirstName.ToLower().Contains(search) || 
                    a.LastName.ToLower().Contains(search) || 
                    a.Email.ToLower().Contains(search) ||
                    (parsedId.HasValue && !isManagerOnly && !isStaffOnly && a.ManagerId.ToString().Contains(idStr)));

                managerQuery = managerQuery.Where(m => 
                    m.FirstName.ToLower().Contains(search) || 
                    m.LastName.ToLower().Contains(search) || 
                    m.Email.ToLower().Contains(search) || 
                    (m.BranchLocation != null && m.BranchLocation.BranchName.ToLower().Contains(search)) ||
                    (parsedId.HasValue && !isAdminOnly && !isStaffOnly && m.BManagerId.ToString().Contains(idStr)));

                staffQuery = staffQuery.Where(s => 
                    s.FirstName.ToLower().Contains(search) || 
                    s.LastName.ToLower().Contains(search) || 
                    s.Email.ToLower().Contains(search) ||
                    (parsedId.HasValue && !isAdminOnly && !isManagerOnly && s.StaffId.ToString().Contains(idStr)));
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

            if (string.IsNullOrWhiteSpace(NewUser.Password))
            {
                TempData["ErrorMessage"] = "Password cannot be empty.";
                OpenModal = "create";
                Branches = await _context.BranchLocations.Where(b => !b.IsArchived).ToListAsync();
                await LoadUsersAsync(CurrentPageIndex);
                return Page();
            }

            if (NewUser.Password != NewUser.ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                OpenModal = "create";
                Branches = await _context.BranchLocations.Where(b => !b.IsArchived).ToListAsync();
                
                // Security: Clear password fields
                NewUser.Password = null;
                NewUser.ConfirmPassword = null;
                ModelState.Remove("NewUser.Password");
                ModelState.Remove("NewUser.ConfirmPassword");

                await LoadUsersAsync(CurrentPageIndex);
                return Page();
            }

            if (NewUser.Role == "Branch Manager")
            {
                if (!NewUser.BranchId.HasValue || NewUser.BranchId <= 0)
                {
                    var firstBranch = await _context.BranchLocations.FirstOrDefaultAsync();
                    if (firstBranch != null) NewUser.BranchId = firstBranch.BranchId;
                }

                // Check if branch already has an active manager
                var existingManager = await _context.BranchManagers
                    .FirstOrDefaultAsync(m => m.BranchId == NewUser.BranchId && !m.IsArchived);
                
                if (existingManager != null)
                {
                    TempData["ErrorMessage"] = $"Branch '{existingManager.FirstName} {existingManager.LastName}' is already assigned to this branch. Each branch can only have one manager.";
                    OpenModal = "create";
                    Branches = await _context.BranchLocations.Where(b => !b.IsArchived).ToListAsync();
                    await LoadUsersAsync(CurrentPageIndex);
                    return Page();
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

            if (!string.IsNullOrEmpty(EditUser.Password))
            {
                if (string.IsNullOrWhiteSpace(EditUser.Password))
                {
                    TempData["ErrorMessage"] = "Password cannot be whitespace.";
                    OpenModal = "edit";
                    Branches = await _context.BranchLocations.Where(b => !b.IsArchived).ToListAsync();
                    await LoadUsersAsync(CurrentPageIndex);
                    return Page();
                }

                if (EditUser.Password != EditUser.ConfirmPassword)
                {
                    TempData["ErrorMessage"] = "Passwords do not match.";
                    OpenModal = "edit";
                    Branches = await _context.BranchLocations.Where(b => !b.IsArchived).ToListAsync();
                    
                    // Security: Clear password fields
                    EditUser.Password = null;
                    EditUser.ConfirmPassword = null;
                    ModelState.Remove("EditUser.Password");
                    ModelState.Remove("EditUser.ConfirmPassword");

                    await LoadUsersAsync(CurrentPageIndex);
                    return Page();
                }
            }

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
                        // Check if the branch is changing or if we are becoming a manager for the first time
                        if (manager.BranchId != EditUser.BranchId.Value)
                        {
                            var existingManager = await _context.BranchManagers
                                .FirstOrDefaultAsync(m => m.BranchId == EditUser.BranchId.Value && m.BManagerId != manager.BManagerId && !m.IsArchived);
                            
                            if (existingManager != null)
                            {
                                TempData["ErrorMessage"] = "This branch already has an active manager.";
                                OpenModal = "edit";
                                Branches = await _context.BranchLocations.Where(b => !b.IsArchived).ToListAsync();
                                await LoadUsersAsync(CurrentPageIndex);
                                return Page();
                            }
                        }
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

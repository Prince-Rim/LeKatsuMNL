using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;
using LeKatsuMNL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeKatsuMNL.Pages.Dashboard
{
    public class ArchiveModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public ArchiveModel(LeKatsuDb context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string ActiveTab { get; set; } = "Items";

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public PaginatedList<ArchiveRow> ArchivedItems { get; set; }

        public class ArchiveRow
        {
            public int Id { get; set; }
            public string FormattedId { get; set; } // New property for styled IDs
            public string Name { get; set; }
            public string Type { get; set; }
            public string Details { get; set; }
        }

        public async Task OnGetAsync(int? pageIndex)
        {
            int pageSize = 10;
            string search = SearchTerm?.ToLower();

            if (ActiveTab == "Users")
            {
                var admins = await _context.AdminAccounts
                    .Where(a => a.IsArchived)
                    .Select(a => new ArchiveRow { Id = a.ManagerId, FormattedId = "ADM-" + a.ManagerId.ToString("D4"), Name = a.FirstName + " " + a.LastName, Type = "Admin", Details = a.Role })
                    .ToListAsync();

                var managers = await _context.BranchManagers
                    .IgnoreQueryFilters()
                    .Where(m => m.IsArchived)
                    .Select(m => new ArchiveRow { Id = m.BManagerId, FormattedId = "BRCH-" + m.BManagerId.ToString("D4"), Name = m.FirstName + " " + m.LastName, Type = "Manager", Details = "Branch Manager" })
                    .ToListAsync();

                var staff = await _context.StaffInformations
                    .Where(s => s.IsArchived)
                    .Select(s => new ArchiveRow { Id = s.StaffId, FormattedId = "STF-" + s.StaffId.ToString("D4"), Name = s.FirstName + " " + s.LastName, Type = "Staff", Details = "Staff" })
                    .ToListAsync();

                var combined = admins.Concat(managers).Concat(staff);

                if (!string.IsNullOrEmpty(search))
                {
                    combined = combined.Where(r => r.Name.ToLower().Contains(search) || r.Type.ToLower().Contains(search) || r.FormattedId.ToLower().Contains(search));
                }

                ArchivedItems = PaginatedList<ArchiveRow>.Create(combined.OrderBy(r => r.Name), pageIndex ?? 1, pageSize);
                return;
            }

            IQueryable<ArchiveRow> query = null;
            string searchString = SearchTerm?.Trim();

            switch (ActiveTab)
            {
                case "Items":
                    var itemsQuery = _context.CommissaryInventories.Where(i => i.IsArchived);
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        if (searchString.StartsWith("ING-", StringComparison.OrdinalIgnoreCase) && int.TryParse(searchString.Substring(4), out int id))
                        {
                            itemsQuery = itemsQuery.Where(i => i.ItemName.Contains(searchString) || i.Uom.Contains(searchString) || i.ComId == id);
                        }
                        else
                        {
                            itemsQuery = itemsQuery.Where(i => i.ItemName.Contains(searchString) || i.Uom.Contains(searchString));
                        }
                    }
                    query = itemsQuery.Select(i => new ArchiveRow { Id = i.ComId, FormattedId = "ING-" + i.ComId.ToString("D3"), Name = i.ItemName, Type = "Ingredient", Details = i.Uom });
                    break;

                case "SKU":
                    var skuQuery = _context.SkuHeaders.Where(s => s.IsArchived);
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        if (searchString.StartsWith("SKU-", StringComparison.OrdinalIgnoreCase) && int.TryParse(searchString.Substring(4), out int id))
                        {
                            skuQuery = skuQuery.Where(s => s.ItemName.Contains(searchString) || s.Uom.Contains(searchString) || s.SkuId == id);
                        }
                        else
                        {
                            skuQuery = skuQuery.Where(s => s.ItemName.Contains(searchString) || s.Uom.Contains(searchString));
                        }
                    }
                    query = skuQuery.Select(s => new ArchiveRow { Id = s.SkuId, FormattedId = "SKU-" + s.SkuId.ToString("D3"), Name = s.ItemName, Type = "SKU", Details = s.Uom });
                    break;

                case "Suppliers":
                    var supplierQuery = _context.VendorInfos.Where(v => v.IsArchived);
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        if (searchString.StartsWith("SUP-", StringComparison.OrdinalIgnoreCase) && int.TryParse(searchString.Substring(4), out int id))
                        {
                            supplierQuery = supplierQuery.Where(v => v.VendorName.Contains(searchString) || v.ContactNum.Contains(searchString) || v.VendorId == id);
                        }
                        else
                        {
                            supplierQuery = supplierQuery.Where(v => v.VendorName.Contains(searchString) || v.ContactNum.Contains(searchString));
                        }
                    }
                    query = supplierQuery.Select(v => new ArchiveRow { Id = v.VendorId, FormattedId = "SUP-" + v.VendorId.ToString("D4"), Name = v.VendorName, Type = "Supplier", Details = v.ContactNum });
                    break;

                case "Branches":
                    var branchQuery = _context.BranchLocations.Where(b => b.IsArchived);
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        if (searchString.StartsWith("BRN-", StringComparison.OrdinalIgnoreCase) && int.TryParse(searchString.Substring(4), out int id))
                        {
                            branchQuery = branchQuery.Where(b => b.BranchName.Contains(searchString) || b.CityMunicipality.Contains(searchString) || b.BranchId == id);
                        }
                        else
                        {
                            branchQuery = branchQuery.Where(b => b.BranchName.Contains(searchString) || b.CityMunicipality.Contains(searchString));
                        }
                    }
                    query = branchQuery.Select(b => new ArchiveRow { Id = b.BranchId, FormattedId = "BRN-" + b.BranchId.ToString("D3"), Name = b.BranchName, Type = "Branch", Details = b.CityMunicipality });
                    break;

                case "Orders":
                    var orderQuery = _context.OrderInfos.Where(o => o.IsArchived);
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        orderQuery = orderQuery.Where(o => o.Status.Contains(searchString) || o.OrderId.ToString().Contains(searchString));
                    }
                    query = orderQuery.Select(o => new ArchiveRow { Id = o.OrderId, FormattedId = o.OrderDate.Year + "-" + o.OrderId.ToString("D4"), Name = "Order #" + o.OrderId, Type = "Branch Order", Details = o.Status });
                    break;

                case "SupplyOrders":
                    var supplyQuery = _context.SupplyOrders.Where(s => s.IsArchived);
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        supplyQuery = supplyQuery.Where(s => s.Status.Contains(searchString) || s.SoaId.ToString().Contains(searchString));
                    }
                    query = supplyQuery.Select(s => new ArchiveRow { Id = s.SoaId, FormattedId = s.SupplyDate.Year + "-" + s.SoaId.ToString("D4"), Name = "Supply Order #" + s.SoaId, Type = "Stock Receipt", Details = s.Status });
                    break;

                case "Categories":
                    var catQuery = _context.Categories.Where(c => c.IsArchived);
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        catQuery = catQuery.Where(c => c.CategoryName.Contains(searchString) || c.SubCategoryNames.Contains(searchString));
                    }
                    query = catQuery.Select(c => new ArchiveRow { Id = c.CategoryId, FormattedId = "CAT-" + c.CategoryId.ToString("D3"), Name = c.CategoryName, Type = "Category", Details = c.SubCategoryNames ?? "-" });
                    break;

                default:
                    query = _context.CommissaryInventories
                        .Where(i => i.IsArchived)
                        .Select(i => new ArchiveRow { Id = i.ComId, FormattedId = "ING-" + i.ComId.ToString("D3"), Name = i.ItemName, Type = "Ingredient", Details = i.Uom });
                    break;
            }

            ArchivedItems = await PaginatedList<ArchiveRow>.CreateAsync(query.AsNoTracking(), pageIndex ?? 1, pageSize);
        }

        public async Task<IActionResult> OnPostRestoreAsync(int id, string tab, string subType)
        {
            switch (tab)
            {
                case "Items":
                    var item = await _context.CommissaryInventories.FindAsync(id);
                    if (item != null) item.IsArchived = false;
                    break;
                case "SKU":
                    var sku = await _context.SkuHeaders
                        .Include(s => s.CommissaryInventory)
                        .FirstOrDefaultAsync(s => s.SkuId == id);
                    if (sku != null)
                    {
                        sku.IsArchived = false;
                        if (sku.CommissaryInventory != null)
                        {
                            sku.CommissaryInventory.IsArchived = false;
                        }
                    }
                    break;
                case "Users":
                    if (subType == "Admin")
                    {
                        var admin = await _context.AdminAccounts.FindAsync(id);
                        if (admin != null) admin.IsArchived = false;
                    }
                    else if (subType == "Manager")
                    {
                        var manager = await _context.BranchManagers.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.BManagerId == id);
                        if (manager != null)
                        {
                            var existing = await _context.BranchManagers.FirstOrDefaultAsync(m => m.BranchId == manager.BranchId && !m.IsArchived && m.BManagerId != manager.BManagerId);
                            if (existing != null)
                            {
                                TempData["ErrorMessage"] = "Cannot restore this manager because the branch already has an active manager.";
                                return RedirectToPage(new { ActiveTab = tab });
                            }
                            manager.IsArchived = false;
                            TempData["SuccessMessage"] = "Manager restored successfully.";
                        }
                    }
                    else if (subType == "Staff")
                    {
                        var staff = await _context.StaffInformations.FindAsync(id);
                        if (staff != null) staff.IsArchived = false;
                    }
                    break;
                case "Suppliers":
                    var vendor = await _context.VendorInfos.FindAsync(id);
                    if (vendor != null) vendor.IsArchived = false;
                    break;
                case "Branches":
                    var branch = await _context.BranchLocations.FindAsync(id);
                    if (branch != null) branch.IsArchived = false;
                    break;
                case "Orders":
                    var order = await _context.OrderInfos.FindAsync(id);
                    if (order != null) order.IsArchived = false;
                    break;
                case "SupplyOrders":
                    var sOrder = await _context.SupplyOrders.FindAsync(id);
                    if (sOrder != null) sOrder.IsArchived = false;
                    break;
                case "Categories":
                    var cat = await _context.Categories.FindAsync(id);
                    if (cat != null) cat.IsArchived = false;
                    break;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { ActiveTab = tab });
        }

        public async Task<IActionResult> OnPostBulkRestoreAsync(string ids, string tab)
        {
            if (string.IsNullOrEmpty(ids)) return RedirectToPage(new { ActiveTab = tab });

            var idList = ids.Split(',').Select(int.Parse).ToList();

            switch (tab)
            {
                case "Items":
                    var items = await _context.CommissaryInventories.Where(i => idList.Contains(i.ComId)).ToListAsync();
                    items.ForEach(i => i.IsArchived = false);
                    break;
                case "SKU":
                    var skus = await _context.SkuHeaders
                        .Include(s => s.CommissaryInventory)
                        .Where(s => idList.Contains(s.SkuId)).ToListAsync();
                    foreach (var s in skus)
                    {
                        s.IsArchived = false;
                        if (s.CommissaryInventory != null)
                        {
                            s.CommissaryInventory.IsArchived = false;
                        }
                    }
                    break;
                case "Users":
                    var admins = await _context.AdminAccounts.Where(a => idList.Contains(a.ManagerId)).ToListAsync();
                    admins.ForEach(a => a.IsArchived = false);
                    var managers = await _context.BranchManagers.Where(m => idList.Contains(m.BManagerId)).ToListAsync();
                    managers.ForEach(m => m.IsArchived = false);
                    var staff = await _context.StaffInformations.Where(s => idList.Contains(s.StaffId)).ToListAsync();
                    staff.ForEach(s => s.IsArchived = false);
                    break;
                case "Suppliers":
                    var vendors = await _context.VendorInfos.Where(v => idList.Contains(v.VendorId)).ToListAsync();
                    vendors.ForEach(v => v.IsArchived = false);
                    break;
                case "Branches":
                    var branches = await _context.BranchLocations.Where(b => idList.Contains(b.BranchId)).ToListAsync();
                    branches.ForEach(b => b.IsArchived = false);
                    break;
                case "Orders":
                    var orders = await _context.OrderInfos.Where(o => idList.Contains(o.OrderId)).ToListAsync();
                    orders.ForEach(o => o.IsArchived = false);
                    break;
                case "SupplyOrders":
                    var sOrders = await _context.SupplyOrders.Where(s => idList.Contains(s.SoaId)).ToListAsync();
                    sOrders.ForEach(s => s.IsArchived = false);
                    break;
                case "Categories":
                    var cats = await _context.Categories.Where(c => idList.Contains(c.CategoryId)).ToListAsync();
                    cats.ForEach(c => c.IsArchived = false);
                    break;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { ActiveTab = tab });
        }
    }
}

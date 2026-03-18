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
            public string Name { get; set; }
            public string Type { get; set; }
            public string Details { get; set; }
            public DateTime ArchivedDate { get; set; }
        }

        public async Task OnGetAsync(int? pageIndex)
        {
            int pageSize = 10;
            IQueryable<ArchiveRow> query = null;

            switch (ActiveTab)
            {
                case "Items":
                    query = _context.CommissaryInventories
                        .Where(i => i.IsArchived)
                        .Select(i => new ArchiveRow { Id = i.ComId, Name = i.ItemName, Type = "Ingredient", Details = i.Uom });
                    break;
                case "SKU":
                    query = _context.SkuHeaders
                        .Where(s => s.IsArchived)
                        .Select(s => new ArchiveRow { Id = s.SkuId, Name = s.ItemName, Type = "SKU", Details = s.Uom });
                    break;
                case "Users":
                    var admins = _context.AdminAccounts.Where(a => a.IsArchived)
                        .Select(a => new ArchiveRow { Id = a.ManagerId, Name = a.FirstName + " " + a.LastName, Type = "Admin", Details = a.Role });
                    var managers = _context.BranchManagers.Where(m => m.IsArchived)
                        .Select(m => new ArchiveRow { Id = m.BManagerId, Name = m.FirstName + " " + m.LastName, Type = "Manager", Details = "Branch Manager" });
                    var staff = _context.StaffInformations.Where(s => s.IsArchived)
                        .Select(s => new ArchiveRow { Id = s.StaffId, Name = s.FirstName + " " + s.LastName, Type = "Staff", Details = "Staff" });
                    query = admins.Union(managers).Union(staff);
                    break;
                case "Suppliers":
                    query = _context.VendorInfos
                        .Where(v => v.IsArchived)
                        .Select(v => new ArchiveRow { Id = v.VendorId, Name = v.VendorName, Type = "Supplier", Details = v.ContactNum });
                    break;
                case "Branches":
                    query = _context.BranchLocations
                        .Where(b => b.IsArchived)
                        .Select(b => new ArchiveRow { Id = b.BranchId, Name = b.BranchName, Type = "Branch", Details = b.CityMunicipality });
                    break;
                case "Orders":
                    query = _context.OrderInfos
                        .Where(o => o.IsArchived)
                        .Select(o => new ArchiveRow { Id = o.OrderId, Name = "Order #" + o.OrderId, Type = "Branch Order", Details = o.Status });
                    break;
                case "SupplyOrders":
                    query = _context.SupplyOrders
                        .Where(s => s.IsArchived)
                        .Select(s => new ArchiveRow { Id = s.SoaId, Name = "Supply Order #" + s.SoaId, Type = "Stock Receipt", Details = s.Status });
                    break;
                default:
                    query = _context.CommissaryInventories
                        .Where(i => i.IsArchived)
                        .Select(i => new ArchiveRow { Id = i.ComId, Name = i.ItemName, Type = "Ingredient", Details = i.Uom });
                    break;
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(r => r.Name.Contains(SearchTerm) || r.Type.Contains(SearchTerm));
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
                    var sku = await _context.SkuHeaders.FindAsync(id);
                    if (sku != null) sku.IsArchived = false;
                    break;
                case "Users":
                    if (subType == "Admin")
                    {
                        var admin = await _context.AdminAccounts.FindAsync(id);
                        if (admin != null) admin.IsArchived = false;
                    }
                    else if (subType == "Manager")
                    {
                        var manager = await _context.BranchManagers.FindAsync(id);
                        if (manager != null) manager.IsArchived = false;
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
                    var skus = await _context.SkuHeaders.Where(s => idList.Contains(s.SkuId)).ToListAsync();
                    skus.ForEach(s => s.IsArchived = false);
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
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { ActiveTab = tab });
        }
    }
}

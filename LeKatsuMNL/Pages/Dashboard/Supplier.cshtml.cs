using System;
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
    public class SupplierModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public SupplierModel(LeKatsuDb context)
        {
            _context = context;
        }

        public PaginatedList<VendorInfo> Vendors { get; set; }

        [BindProperty]
        public VendorInfo NewVendor { get; set; }

        [BindProperty]
        public VendorInfo EditVendor { get; set; }

        public string SearchString { get; set; }

        public async Task OnGetAsync(int? pageIndex, string searchString, int? pageSize)
        {
            SearchString = searchString;
            IQueryable<VendorInfo> query = _context.VendorInfos
                .Include(v => v.CommissaryInventories)
                .OrderByDescending(v => v.CreatedAt);

            if (!string.IsNullOrEmpty(SearchString))
            {
                var search = SearchString.ToLower();
                query = query.Where(v => v.VendorName.ToLower().Contains(search) || 
                                       v.ContactNum.ToLower().Contains(search) ||
                                       v.CommissaryInventories.Any(ci => ci.ItemName.ToLower().Contains(search)));
            }

            Vendors = await PaginatedList<VendorInfo>.CreateAsync(query, pageIndex ?? 1, pageSize ?? 10);
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!PermissionHelper.HasPermission(User, "Supplier", 'C'))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(NewVendor.VendorName))
            {
                return RedirectToPage(); // Failed basic validation
            }

            var vendor = new VendorInfo
            {
                VendorName = NewVendor.VendorName,
                ContactNum = NewVendor.ContactNum ?? "",
                SecondVendorName = NewVendor.SecondVendorName ?? "",
                SecondVendorCn = NewVendor.SecondVendorCn ?? "",
                SupplierType = NewVendor.SupplierType ?? "Main",
                CreatedAt = DateTime.Now
            };

            _context.VendorInfos.Add(vendor);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!PermissionHelper.HasPermission(User, "Supplier", 'U'))
            {
                return Forbid();
            }

            var vendorToUpdate = await _context.VendorInfos.FindAsync(EditVendor.VendorId);

            if (vendorToUpdate == null)
            {
                return NotFound();
            }

            vendorToUpdate.VendorName = EditVendor.VendorName;
            vendorToUpdate.ContactNum = EditVendor.ContactNum ?? "";
            vendorToUpdate.SupplierType = EditVendor.SupplierType ?? "Main";

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!PermissionHelper.HasPermission(User, "Supplier", 'D'))
            {
                return Forbid();
            }

            var vendorToDelete = await _context.VendorInfos.FindAsync(id);

            if (vendorToDelete != null)
            {
                _context.VendorInfos.Remove(vendorToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}

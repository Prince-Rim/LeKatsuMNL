using System;
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
    public class SupplierModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public SupplierModel(LeKatsuDb context)
        {
            _context = context;
        }

        public IList<VendorInfo> Vendors { get; set; }

        [BindProperty]
        public VendorInfo NewVendor { get; set; }

        [BindProperty]
        public VendorInfo EditVendor { get; set; }

        public async Task OnGetAsync()
        {
            Vendors = await _context.VendorInfos
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
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
                CreatedAt = DateTime.Now
            };

            _context.VendorInfos.Add(vendor);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var vendorToUpdate = await _context.VendorInfos.FindAsync(EditVendor.VendorId);

            if (vendorToUpdate == null)
            {
                return NotFound();
            }

            vendorToUpdate.VendorName = EditVendor.VendorName;
            vendorToUpdate.ContactNum = EditVendor.ContactNum ?? "";

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
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

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
    public class CategoryModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public CategoryModel(LeKatsuDb context)
        {
            _context = context;
        }

        public PaginatedList<Category> Categories { get; set; } = default!;

        [BindProperty]
        public Category NewCategory { get; set; } = default!;

        [BindProperty]
        public Category EditCategory { get; set; } = default!;

        public async Task OnGetAsync(int? pageIndex)
        {
            var query = _context.Categories.OrderBy(c => c.CategoryName);
            Categories = await PaginatedList<Category>.CreateAsync(query, pageIndex ?? 1, 10);
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!PermissionHelper.HasPermission(User, "Category", 'C'))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(NewCategory.CategoryName))
            {
                var query = _context.Categories.OrderBy(c => c.CategoryName);
                Categories = await PaginatedList<Category>.CreateAsync(query, 1, 10);
                return Page();
            }

            _context.Categories.Add(NewCategory);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!PermissionHelper.HasPermission(User, "Category", 'U'))
            {
                return Forbid();
            }

            var categoryToUpdate = await _context.Categories.FindAsync(EditCategory.CategoryId);

            if (categoryToUpdate == null)
            {
                return NotFound();
            }

            categoryToUpdate.CategoryName = EditCategory.CategoryName;
            categoryToUpdate.Description = EditCategory.Description;

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!PermissionHelper.HasPermission(User, "Category", 'D'))
            {
                return Forbid();
            }

            var categoryToDelete = await _context.Categories.FindAsync(id);

            if (categoryToDelete != null)
            {
                _context.Categories.Remove(categoryToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}

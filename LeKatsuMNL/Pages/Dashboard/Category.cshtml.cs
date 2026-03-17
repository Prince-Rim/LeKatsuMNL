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

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync(int? pageIndex)
        {
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(c => c.CategoryName.Contains(SearchTerm) || (c.SubCategoryNames != null && c.SubCategoryNames.Contains(SearchTerm)));
            }

            query = query.OrderBy(c => c.CategoryName);
            int pageSize = PageSize > 0 ? PageSize : 10;
            Categories = await PaginatedList<Category>.CreateAsync(query, pageIndex ?? 1, pageSize);
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

            await SyncSubCategories(NewCategory.CategoryId, NewCategory.SubCategoryNames);

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
            categoryToUpdate.SubCategoryNames = EditCategory.SubCategoryNames;

            await _context.SaveChangesAsync();

            await SyncSubCategories(categoryToUpdate.CategoryId, categoryToUpdate.SubCategoryNames);

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

        private async Task SyncSubCategories(int categoryId, string subCategoryNamesString)
        {
            if (string.IsNullOrWhiteSpace(subCategoryNamesString))
            {
                // Remove all subcategories for this category if the string is empty
                var existing = await _context.SubCategories.Where(sc => sc.CategoryId == categoryId).ToListAsync();
                _context.SubCategories.RemoveRange(existing);
                await _context.SaveChangesAsync();
                return;
            }

            var nameList = subCategoryNamesString.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                                                .Select(s => s.Trim())
                                                .Where(s => !string.IsNullOrEmpty(s))
                                                .Distinct()
                                                .ToList();

            var existingSubCats = await _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .ToListAsync();

            // Remove subcategories that are no longer in the list
            foreach (var existing in existingSubCats)
            {
                if (!nameList.Any(n => n.Equals(existing.SubCategoryName, System.StringComparison.OrdinalIgnoreCase)))
                {
                    _context.SubCategories.Remove(existing);
                }
            }

            // Add new subcategories
            foreach (var name in nameList)
            {
                if (!existingSubCats.Any(sc => sc.SubCategoryName.Equals(name, System.StringComparison.OrdinalIgnoreCase)))
                {
                    _context.SubCategories.Add(new SubCategory
                    {
                        CategoryId = categoryId,
                        SubCategoryName = name
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

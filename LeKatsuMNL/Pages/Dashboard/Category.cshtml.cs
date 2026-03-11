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
    public class CategoryModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public CategoryModel(LeKatsuDb context)
        {
            _context = context;
        }

        public IList<Category> Categories { get; set; } = default!;

        [BindProperty]
        public Category NewCategory { get; set; } = default!;

        [BindProperty]
        public Category EditCategory { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories.ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (string.IsNullOrEmpty(NewCategory.CategoryName))
            {
                Categories = await _context.Categories.ToListAsync();
                return Page();
            }

            _context.Categories.Add(NewCategory);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
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

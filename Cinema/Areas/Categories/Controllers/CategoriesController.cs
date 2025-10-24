using Microsoft.AspNetCore.Mvc;
using Cinema.Models;
using Cinema.Repositories;

namespace Cinema.Areas.Categories.Controllers
{
    [Area("Categories")]
    public class CategoriesController : Controller
    {
        private readonly Repository<Category> _categoryRepository;
        private readonly IWebHostEnvironment _env;

        public CategoriesController(Repository<Category> categoryRepository, IWebHostEnvironment env)
        {
            _categoryRepository = categoryRepository;
            _env = env;
        }

        // 📋 Index
        public async Task<IActionResult> Index(string? search, string sortOrder = "asc")
        {
            var categories = await _categoryRepository.GetAsync();

            if (!string.IsNullOrEmpty(search))
            {
                categories = categories.Where(c => c.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));
                ViewBag.Search = search;
            }

            categories = sortOrder == "desc"
                ? categories.OrderByDescending(c => c.DisplayOrder)
                : categories.OrderBy(c => c.DisplayOrder);

            ViewBag.SortOrder = sortOrder;
            return View(categories.ToList());
        }

        // ➕ Create (GET)
        public IActionResult Create() => View();

        // ➕ Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, IFormFile? img)
        {
            if (ModelState.IsValid)
            {
                if (img != null && img.Length > 0)
                {
                    var folderPath = Path.Combine(_env.WebRootPath, "images/categories");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(folderPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await img.CopyToAsync(stream);

                    category.ImageUrl = fileName;
                }

                category.CreatedAt = DateTime.Now;
                await _categoryRepository.CreateAsync(category);

                TempData["Success"] = "Category added successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // ✏️ Edit (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // ✏️ Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category, IFormFile? img)
        {
            if (id != category.Id) return NotFound();

            var catInDb = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (catInDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                catInDb.Name = category.Name;
                catInDb.Description = category.Description;
                catInDb.DisplayOrder = category.DisplayOrder;
                catInDb.UpdatedAt = DateTime.Now;

                // ✅ تعديل الصورة (إن وُجدت)
                if (img != null && img.Length > 0)
                {
                    var folderPath = Path.Combine(_env.WebRootPath, "images/categories");

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    if (!string.IsNullOrEmpty(catInDb.ImageUrl))
                    {
                        var oldPath = Path.Combine(folderPath, catInDb.ImageUrl);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(folderPath, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await img.CopyToAsync(stream);

                    catInDb.ImageUrl = fileName;
                }

                await _categoryRepository.UpdateAsync(catInDb);
                TempData["Success"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // 🔍 Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // 🗑 Delete (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // 🗑 Delete (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryRepository.DeleteAsync(id);
            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

using Cinema.Models;
using Cinema.Repositories.IRepositories;
using Cinema.Services.IServices;
using Cinema.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Categories.Controllers
{
    [Area("Categories")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.USER_ROLE}")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IImageService _imageService;

        public CategoriesController(ICategoryRepository categoryRepository, IImageService imageService)
        {
            _categoryRepository = categoryRepository;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index(string? search, string sortOrder = "asc")
        {
            var categories = (await _categoryRepository.GetAsync()).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                categories = categories.Where(c => c.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));
                ViewBag.Search = search;
            }

            sortOrder = sortOrder?.ToLower() == "desc" ? "desc" : "asc";
            categories = sortOrder == "desc"
                ? categories.OrderByDescending(c => c.DisplayOrder)
                : categories.OrderBy(c => c.DisplayOrder);

            ViewBag.SortOrder = sortOrder;
            return View(categories.ToList());
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, IFormFile? img)
        {
            if (!ModelState.IsValid) return View(category);

            category.ImageUrl = await _imageService.UploadAsync(img, "categories");
            category.CreatedAt = DateTime.Now;

            await _categoryRepository.CreateAsync(category);
            TempData["Success"] = "Category added successfully!";
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category, IFormFile? img)
        {
            if (id != category.Id) return NotFound();
            var catInDb = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (catInDb == null) return NotFound();

            if (!ModelState.IsValid) return View(category);

            catInDb.Name = category.Name;
            catInDb.Description = category.Description;
            catInDb.DisplayOrder = category.DisplayOrder;
            catInDb.UpdatedAt = DateTime.Now;

            catInDb.ImageUrl = await _imageService.UploadAsync(img, "categories", catInDb.ImageUrl);

            await _categoryRepository.UpdateAsync(catInDb);
            TempData["Success"] = "Category updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetOneAsync(c => c.Id == id);
            if (category != null)
            {
                await _imageService.DeleteAsync(category.ImageUrl, "categories");
                await _categoryRepository.DeleteAsync(id);
            }

            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

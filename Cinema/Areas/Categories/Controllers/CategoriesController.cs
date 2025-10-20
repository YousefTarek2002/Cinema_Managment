using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cinema.Data;
using Cinema.Models;

namespace Cinema.Areas.Categories.Controllers
{
    [Area("Categories")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(string? search, string sortOrder = "asc")
        {
            var categories = _db.Categories
                .Include(c => c.Movies)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                categories = categories.Where(c => c.Name.Contains(search.Trim()));
                ViewBag.Search = search;
            }

            categories = sortOrder == "desc"
                ? categories.OrderByDescending(c => c.DisplayOrder)
                : categories.OrderBy(c => c.DisplayOrder);

            ViewBag.SortOrder = sortOrder;
            return View(categories.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category, IFormFile? img)
        {
            if (ModelState.IsValid)
            {
                if (img != null && img.Length > 0)
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(folderPath, fileName);
                    using (var stream = System.IO.File.Create(filePath))
                        img.CopyTo(stream);

                    category.ImageUrl = fileName;
                }

                category.CreatedAt = DateTime.Now;
                _db.Categories.Add(category);
                _db.SaveChanges();
                TempData["Success"] = "Category added successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = _db.Categories.Find(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Category category, IFormFile? img)
        {
            if (id != category.Id) return NotFound();

            var catInDb = _db.Categories.Find(id);
            if (catInDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                catInDb.Name = category.Name;
                catInDb.Description = category.Description;
                catInDb.DisplayOrder = category.DisplayOrder;
                catInDb.UpdatedAt = DateTime.Now;

                if (img != null && img.Length > 0)
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/categories");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    // حذف الصورة القديمة
                    if (!string.IsNullOrEmpty(catInDb.ImageUrl))
                    {
                        var oldPath = Path.Combine(folderPath, catInDb.ImageUrl);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    // حفظ الجديدة
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(folderPath, fileName);
                    using (var stream = System.IO.File.Create(filePath))
                        img.CopyTo(stream);

                    catInDb.ImageUrl = fileName;
                }

                _db.Update(catInDb);
                _db.SaveChanges();
                TempData["Success"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var category = _db.Categories
                .Include(c => c.Movies)
                .FirstOrDefault(c => c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = _db.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var cat = _db.Categories.Find(id);
            if (cat == null)
                return NotFound();

            _db.Categories.Remove(cat);
            _db.SaveChanges();
            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }

    }
}

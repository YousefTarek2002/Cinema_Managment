using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cinema.Data;
using Cinema.Models;

namespace Cinema.Areas.Cinemas.Controllers
{
    [Area("Cinemas")]
    public class CinemasController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public CinemasController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public IActionResult Index(string? search)
        {
            var query = _db.Cinemas
                .Include(c => c.Movies)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(search) ||
                    (c.Location != null && c.Location.ToLower().Contains(search)));
            }

            var cinemas = query
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.Search = search;
            return View(cinemas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cinemaa cinema, IFormFile? Img, IFormFile? LogoFile)
        {
            if (ModelState.IsValid)
            {
                if (Img != null)
                    cinema.ImgUrl = SaveImage(Img);

                if (LogoFile != null)
                    cinema.Logo = SaveImage(LogoFile);

                _db.Cinemas.Add(cinema);
                _db.SaveChanges();

                TempData["Success"] = " Cinema added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(cinema);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var cinema = _db.Cinemas.Find(id);
            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Cinemaa cinema, IFormFile? Img, IFormFile? LogoFile)
        {
            if (id != cinema.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(cinema);

            var existing = _db.Cinemas.AsNoTracking().FirstOrDefault(c => c.Id == id);
            if (existing == null)
                return NotFound();

            if (Img != null)
                cinema.ImgUrl = SaveImage(Img);
            else
                cinema.ImgUrl = existing.ImgUrl;

            if (LogoFile != null)
                cinema.Logo = SaveImage(LogoFile);
            else
                cinema.Logo = existing.Logo;

            _db.Cinemas.Update(cinema);
            _db.SaveChanges();

            TempData["Success"] = " Cinema updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            var cinema = _db.Cinemas
                .Include(c => c.Movies)
                .FirstOrDefault(c => c.Id == id);

            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var cinema = _db.Cinemas.Find(id);
            if (cinema == null)
                return NotFound();

            return View(cinema);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var cinema = _db.Cinemas.Find(id);
            if (cinema == null)
                return NotFound();

            _db.Cinemas.Remove(cinema);
            _db.SaveChanges();
            return Ok(); 
        }

        private string SaveImage(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFile = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFile);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return "/uploads/" + uniqueFile;
        }
    }
}

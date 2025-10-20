using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cinema.Data;
using Cinema.Models;

namespace Cinema.Areas.Movies.Controllers
{
    [Area("Movies")]
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public MoviesController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public IActionResult Index(string? search)
        {
            var movies = _db.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                movies = movies.Where(m => m.Name.Contains(search));

            return View(movies.ToList());
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = _db.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null) return NotFound();
            return View(movie);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = _db.Categories.ToList();
            ViewBag.Cinemas = _db.Cinemas.ToList();
            ViewBag.Actors = _db.Actors.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Movie movie, IFormFile? MainImage, List<IFormFile>? SubImages, List<int>? SelectedActors)
        {
            if (ModelState.IsValid)
            {
                // حفظ الصورة الرئيسية
                if (MainImage != null)
                {
                    string uploadFolder = Path.Combine(_env.WebRootPath, "uploads/movies");
                    Directory.CreateDirectory(uploadFolder);
                    string fileName = Guid.NewGuid() + Path.GetExtension(MainImage.FileName);
                    string fullPath = Path.Combine(uploadFolder, fileName);
                    using var stream = new FileStream(fullPath, FileMode.Create);
                    MainImage.CopyTo(stream);
                    movie.MainImg = "/uploads/movies/" + fileName;
                }

                // حفظ الصور الفرعية
                if (SubImages != null && SubImages.Count > 0)
                {
                    string subFolder = Path.Combine(_env.WebRootPath, "uploads/movies/sub");
                    Directory.CreateDirectory(subFolder);

                    List<string> savedPaths = new();
                    foreach (var img in SubImages)
                    {
                        string subFile = Guid.NewGuid() + Path.GetExtension(img.FileName);
                        string subFull = Path.Combine(subFolder, subFile);
                        using var stream = new FileStream(subFull, FileMode.Create);
                        img.CopyTo(stream);
                        savedPaths.Add("/uploads/movies/sub/" + subFile);
                    }
                    movie.SubImages = string.Join(",", savedPaths);
                }

                if (SelectedActors != null)
                {
                    movie.MovieActors = SelectedActors.Select(aid => new MovieActor { ActorId = aid }).ToList();
                }

                movie.DateTime = DateTime.UtcNow;
                _db.Movies.Add(movie);
                _db.SaveChanges();

                TempData["Success"] = " Movie added successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _db.Categories.ToList();
            ViewBag.Cinemas = _db.Cinemas.ToList();
            ViewBag.Actors = _db.Actors.ToList();
            return View(movie);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var movie = _db.Movies
                .Include(m => m.MovieActors)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null) return NotFound();

            ViewBag.Categories = _db.Categories.ToList();
            ViewBag.Cinemas = _db.Cinemas.ToList();
            ViewBag.Actors = _db.Actors.ToList();
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Movie movie, IFormFile? MainImage, List<IFormFile>? SubImages, List<int>? SelectedActors)
        {
            if (id != movie.Id) return NotFound();

            var existing = _db.Movies
                .Include(m => m.MovieActors)
                .FirstOrDefault(m => m.Id == id);

            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                existing.Name = movie.Name;
                existing.Description = movie.Description;
                existing.Price = movie.Price;
                existing.Status = movie.Status;
                existing.CategoryId = movie.CategoryId;
                existing.CinemaId = movie.CinemaId;

                if (MainImage != null)
                {
                    string uploadFolder = Path.Combine(_env.WebRootPath, "uploads/movies");
                    Directory.CreateDirectory(uploadFolder);
                    string fileName = Guid.NewGuid() + Path.GetExtension(MainImage.FileName);
                    string fullPath = Path.Combine(uploadFolder, fileName);
                    using var stream = new FileStream(fullPath, FileMode.Create);
                    MainImage.CopyTo(stream);
                    existing.MainImg = "/uploads/movies/" + fileName;
                }

                if (SubImages != null && SubImages.Count > 0)
                {
                    string subFolder = Path.Combine(_env.WebRootPath, "uploads/movies/sub");
                    Directory.CreateDirectory(subFolder);

                    List<string> savedPaths = new();
                    foreach (var img in SubImages)
                    {
                        string subFile = Guid.NewGuid() + Path.GetExtension(img.FileName);
                        string subFull = Path.Combine(subFolder, subFile);
                        using var stream = new FileStream(subFull, FileMode.Create);
                        img.CopyTo(stream);
                        savedPaths.Add("/uploads/movies/sub/" + subFile);
                    }
                    existing.SubImages = string.Join(",", savedPaths);
                }

                existing.MovieActors.Clear();
                if (SelectedActors != null)
                {
                    existing.MovieActors = SelectedActors.Select(aid => new MovieActor { MovieId = existing.Id, ActorId = aid }).ToList();
                }

                _db.Update(existing);
                _db.SaveChanges();
                TempData["Success"] = " Movie updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _db.Categories.ToList();
            ViewBag.Cinemas = _db.Cinemas.ToList();
            ViewBag.Actors = _db.Actors.ToList();
            return View(movie);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var movie = _db.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null) return NotFound();
            return View(movie);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var movie = _db.Movies.Find(id);
                if (movie == null)
                    return NotFound();

                // حذف الصور إن وجدت
                if (!string.IsNullOrEmpty(movie.MainImg))
                {
                    var mainPath = Path.Combine(_env.WebRootPath, movie.MainImg.TrimStart('/'));
                    if (System.IO.File.Exists(mainPath))
                        System.IO.File.Delete(mainPath);
                }

                if (!string.IsNullOrEmpty(movie.SubImages))
                {
                    var subs = movie.SubImages.Split(',');
                    foreach (var img in subs)
                    {
                        var path = Path.Combine(_env.WebRootPath, img.TrimStart('/'));
                        if (System.IO.File.Exists(path))
                            System.IO.File.Delete(path);
                    }
                }

                _db.Movies.Remove(movie);
                _db.SaveChanges();
                TempData["Success"] = "🎬 Movie deleted successfully!";
            }
            catch
            {
                TempData["Error"] = "Something went wrong while deleting.";
            }

            return RedirectToAction(nameof(Index));
        }


    }
}

using Cinema.Models;
using Cinema.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Movies.Controllers
{
    [Area("Movies")]
    public class MoviesController : Controller
    {
        private readonly MovieRepository _movieRepository;

        public MoviesController(MovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        // 📋 Index
        public async Task<IActionResult> Index()
        {
            var movies = await _movieRepository.GetAsync();
            return View(movies);
        }

        // ➕ Create (GET)
        public IActionResult Create() => View();

        // ➕ Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile? img)
        {
            if (ModelState.IsValid)
            {
                if (img != null && img.Length > 0)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/movies");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(folder, fileName);
                    using var stream = System.IO.File.Create(filePath);
                    await img.CopyToAsync(stream);
                    movie.MainImg = fileName;
                }

                movie.DateTime = DateTime.Now;
                await _movieRepository.CreateAsync(movie);
                TempData["Success"] = "Movie added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // ✏️ Edit
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _movieRepository.GetOneAsync(m => m.Id == id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie, IFormFile? img)
        {
            if (id != movie.Id) return NotFound();
            var movieInDb = await _movieRepository.GetOneAsync(m => m.Id == id);
            if (movieInDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                movieInDb.Description = movie.Description;
                movieInDb.DateTime = DateTime.Now;

                if (img != null)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/movies");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    if (!string.IsNullOrEmpty(movieInDb.MainImg))
                    {
                        var oldPath = Path.Combine(folder, movieInDb.MainImg);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(folder, fileName);
                    using var stream = System.IO.File.Create(filePath);
                    await img.CopyToAsync(stream);
                    movieInDb.MainImg = fileName;
                }

                await _movieRepository.UpdateAsync(movieInDb);
                TempData["Success"] = "Movie updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // 🗑 Delete
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _movieRepository.GetOneAsync(m => m.Id == id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _movieRepository.DeleteAsync(id);
            TempData["Success"] = "Movie deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

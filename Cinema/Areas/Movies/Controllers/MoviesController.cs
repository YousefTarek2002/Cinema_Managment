using Cinema.Models;
using Cinema.Repositories.IRepositories;
using Cinema.Services.IServices;
using Cinema.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cinema.Areas.Movies.Controllers
{
    [Area("Movies")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.USER_ROLE}")]

    public class MoviesController : Controller
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICinemaRepository _cinemaRepository;
        private readonly IImageService _imageService;

        public MoviesController(
            IMovieRepository movieRepository,
            ICategoryRepository categoryRepository,
            ICinemaRepository cinemaRepository,
            IImageService imageService)
        {
            _movieRepository = movieRepository;
            _categoryRepository = categoryRepository;
            _cinemaRepository = cinemaRepository;
            _imageService = imageService;
        }

        public IActionResult Index(string? search)
        {
            var movies = _movieRepository.GetFullMovies().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                movies = movies.Where(m =>
                    m.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    (m.Category != null && m.Category.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)) ||
                    (m.Cinema != null && m.Cinema.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)));
                ViewBag.Search = search;
            }

            return View(movies.ToList());
        }

        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile? mainImg, List<IFormFile>? subImgs)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(movie);
            }

            movie.MainImg = await _imageService.UploadAsync(mainImg, "movies/main");
            movie.SubImages = await _imageService.UploadMultipleAsync(subImgs, "movies/sub");
            movie.CreatedAt = DateTime.Now;

            await _movieRepository.CreateAsync(movie);
            TempData["Success"] = "Movie added successfully!";
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]

        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _movieRepository.GetOneAsync(m => m.Id == id);
            if (movie == null) return NotFound();
            await LoadDropdowns();
            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]

        public async Task<IActionResult> Edit(int id, Movie movie, IFormFile? mainImg, List<IFormFile>? subImgs)
        {
            if (id != movie.Id) return NotFound();

            var movieInDb = await _movieRepository.GetOneAsync(m => m.Id == id);
            if (movieInDb == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(movie);
            }

            movieInDb.Name = movie.Name;
            movieInDb.Description = movie.Description;
            movieInDb.Price = movie.Price;
            movieInDb.Status = movie.Status;
            movieInDb.CategoryId = movie.CategoryId;
            movieInDb.CinemaId = movie.CinemaId;
            movieInDb.UpdatedAt = DateTime.Now;

            movieInDb.MainImg = await _imageService.UploadAsync(mainImg, "movies/main", movieInDb.MainImg);
            movieInDb.SubImages = await _imageService.UploadMultipleAsync(subImgs, "movies/sub", movieInDb.SubImages);

            await _movieRepository.UpdateAsync(movieInDb);
            TempData["Success"] = "Movie updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var movie = await _movieRepository.GetOneAsync(m => m.Id == id, includeProperties: "Category,Cinema");
            if (movie == null) return NotFound();
            return View(movie);
        }
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]

        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _movieRepository.GetOneAsync(m => m.Id == id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _movieRepository.GetOneAsync(m => m.Id == id);
            if (movie != null)
            {
                await _imageService.DeleteAsync(movie.MainImg, "movies/main");
                await _imageService.DeleteMultipleAsync(movie.SubImages, "movies/sub");
                await _movieRepository.DeleteAsync(id);
            }

            TempData["Success"] = "Movie deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns()
        {
            ViewBag.Categories = new SelectList(await _categoryRepository.GetAsync(), "Id", "Name");
            ViewBag.Cinemas = new SelectList(await _cinemaRepository.GetAsync(), "Id", "Name");
        }
    }
}

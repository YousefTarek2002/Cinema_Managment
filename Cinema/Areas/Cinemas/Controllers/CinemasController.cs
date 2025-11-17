using Cinema.Models;
using Cinema.Repositories.IRepositories;
using Cinema.Services.IServices;
using Cinema.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Cinemas.Controllers
{
    [Area("Cinemas")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.USER_ROLE}")]

    public class CinemasController : Controller
    {
        private readonly ICinemaRepository _cinemaRepository;
        private readonly IImageService _imageService;

        public CinemasController(ICinemaRepository cinemaRepository, IImageService imageService)
        {
            _cinemaRepository = cinemaRepository;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var cinemas = (await _cinemaRepository.GetAsync()).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                cinemas = cinemas.Where(c =>
                    c.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(c.Location) && c.Location.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)));
                ViewBag.Search = search;
            }
            return View(cinemas.ToList());
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cinemaa cinema, IFormFile? img)
        {
            if (!ModelState.IsValid) return View(cinema);

            cinema.ImgUrl = await _imageService.UploadAsync(img, "cinemas");
            cinema.CreatedAt = DateTime.Now;

            await _cinemaRepository.CreateAsync(cinema);
            TempData["Success"] = "Cinema added successfully!";
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]

        public async Task<IActionResult> Edit(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(c => c.Id == id);
            if (cinema == null) return NotFound();
            return View(cinema);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cinemaa cinema, IFormFile? img)
        {
            if (id != cinema.Id) return NotFound();
            var cinemaInDb = await _cinemaRepository.GetOneAsync(c => c.Id == id);
            if (cinemaInDb == null) return NotFound();

            if (!ModelState.IsValid) return View(cinema);

            cinemaInDb.Name = cinema.Name;
            cinemaInDb.Description = cinema.Description;
            cinemaInDb.Location = cinema.Location;
            cinemaInDb.UpdatedAt = DateTime.Now;

            cinemaInDb.ImgUrl = await _imageService.UploadAsync(img, "cinemas", cinemaInDb.ImgUrl);

            await _cinemaRepository.UpdateAsync(cinemaInDb);
            TempData["Success"] = "Cinema updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(c => c.Id == id);
            if (cinema == null) return NotFound();
            return View(cinema);
        }
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]

        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(c => c.Id == id);
            if (cinema == null) return NotFound();
            return View(cinema);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(c => c.Id == id);
            if (cinema != null)
            {
                await _imageService.DeleteAsync(cinema.ImgUrl, "cinemas");
                await _cinemaRepository.DeleteAsync(id);
            }

            TempData["Success"] = "Cinema deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

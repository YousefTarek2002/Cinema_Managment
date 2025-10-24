using Cinema.Models;
using Cinema.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Cinemas.Controllers
{
    [Area("Cinemas")]
    public class CinemasController : Controller
    {
        private readonly CinemaRepository _cinemaRepository;

        public CinemasController(CinemaRepository cinemaRepository)
        {
            _cinemaRepository = cinemaRepository;
        }

        public async Task<IActionResult> Index()
        {
            var cinemas = await _cinemaRepository.GetAsync();
            return View(cinemas);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cinemaa cinema, IFormFile? img)
        {
            if (ModelState.IsValid)
            {
                if (img != null)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cinemas");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(folder, fileName);
                    using var stream = System.IO.File.Create(filePath);
                    await img.CopyToAsync(stream);
                    cinema.ImgUrl = fileName;
                }

                cinema.CreatedAt = DateTime.Now;
                await _cinemaRepository.CreateAsync(cinema);
                TempData["Success"] = "Cinema added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(cinema);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(c => c.Id == id);
            if (cinema == null) return NotFound();
            return View(cinema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cinemaa cinema, IFormFile? img)
        {
            if (id != cinema.Id) return NotFound();
            var cinemaInDb = await _cinemaRepository.GetOneAsync(c => c.Id == id);
            if (cinemaInDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                cinemaInDb.Name = cinema.Name;
                cinemaInDb.Description = cinema.Description;
                cinemaInDb.Location = cinema.Location;
                cinemaInDb.UpdatedAt = DateTime.Now;

                if (img != null)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cinemas");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    if (!string.IsNullOrEmpty(cinemaInDb.ImgUrl))
                    {
                        var oldPath = Path.Combine(folder, cinemaInDb.ImgUrl);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(folder, fileName);
                    using var stream = System.IO.File.Create(filePath);
                    await img.CopyToAsync(stream);
                    cinemaInDb.ImgUrl = fileName;
                }

                await _cinemaRepository.UpdateAsync(cinemaInDb);
                TempData["Success"] = "Cinema updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(cinema);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _cinemaRepository.GetOneAsync(c => c.Id == id);
            if (cinema == null) return NotFound();
            return View(cinema);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _cinemaRepository.DeleteAsync(id);
            TempData["Success"] = "Cinema deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

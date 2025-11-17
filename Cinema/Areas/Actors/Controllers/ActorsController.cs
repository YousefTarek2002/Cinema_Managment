using Cinema.Models;
using Cinema.Repositories.IRepositories;
using Cinema.Services.IServices;
using Cinema.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Actors.Controllers
{
    [Area("Actors")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.USER_ROLE}")]

    public class ActorsController : Controller
    {
        private readonly IActorRepository _actorRepository;
        private readonly IImageService _imageService;

        public ActorsController(IActorRepository actorRepository, IImageService imageService)
        {
            _actorRepository = actorRepository;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var actors = (await _actorRepository.GetAsync()).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                actors = actors.Where(a => a.FullName.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));
                ViewBag.Search = search;
            }
            return View(actors.ToList());
        }
        [HttpGet]

        public IActionResult Create() => View();

        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Actor actor, IFormFile? img)
        {
            if (!ModelState.IsValid) return View(actor);

            actor.ImgUrl = await _imageService.UploadAsync(img, "actors");
            actor.CreatedAt = DateTime.Now;

            await _actorRepository.CreateAsync(actor);
            TempData["Success"] = "Actor added successfully!";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(int id)
        {
            var actor = await _actorRepository.GetOneAsync(a => a.Id == id);
            if (actor == null) return NotFound();
            return View(actor);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Actor actor, IFormFile? img)
        {
            if (id != actor.Id) return NotFound();
            var actorInDb = await _actorRepository.GetOneAsync(a => a.Id == id);
            if (actorInDb == null) return NotFound();

            if (!ModelState.IsValid) return View(actor);

            actorInDb.FullName = actor.FullName;
            actorInDb.Bio = actor.Bio;
            actorInDb.UpdatedAt = DateTime.Now;

            actorInDb.ImgUrl = await _imageService.UploadAsync(img, "actors", actorInDb.ImgUrl);

            await _actorRepository.UpdateAsync(actorInDb);
            TempData["Success"] = "Actor updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id)
        {
            var actor = await _actorRepository.GetOneAsync(a => a.Id == id);
            if (actor == null) return NotFound();
            return View(actor);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _actorRepository.GetOneAsync(a => a.Id == id);
            if (actor != null)
            {
                await _imageService.DeleteAsync(actor.ImgUrl, "actors");
                await _actorRepository.DeleteAsync(id);
            }

            TempData["Success"] = "Actor deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var actor = await _actorRepository.GetOneAsync(a => a.Id == id);
            if (actor == null) return NotFound();
            return View(actor);
        }
    }
}

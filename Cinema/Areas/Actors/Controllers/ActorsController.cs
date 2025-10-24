using Cinema.Models;
using Cinema.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Actors.Controllers
{
    [Area("Actors")]
    public class ActorsController : Controller
    {
        private readonly ActorRepository _actorRepository;

        public ActorsController(ActorRepository actorRepository)
        {
            _actorRepository = actorRepository;
        }

        // 📋 Index
        public async Task<IActionResult> Index()
        {
            var actors = await _actorRepository.GetAsync();
            return View(actors);
        }

        // ➕ Create (GET)
        public IActionResult Create() => View();

        // ➕ Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Actor actor, IFormFile? img)
        {
            if (ModelState.IsValid)
            {
                if (img != null && img.Length > 0)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/actors");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(folder, fileName);
                    using var stream = System.IO.File.Create(filePath);
                    await img.CopyToAsync(stream);
                    actor.ImgUrl = fileName;
                }

                actor.CreatedAt = DateTime.Now;
                await _actorRepository.CreateAsync(actor);
                TempData["Success"] = "Actor added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // ✏️ Edit (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var actor = await _actorRepository.GetOneAsync(a => a.Id == id);
            if (actor == null) return NotFound();
            return View(actor);
        }

        // ✏️ Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Actor actor, IFormFile? img)
        {
            if (id != actor.Id) return NotFound();
            var actorInDb = await _actorRepository.GetOneAsync(a => a.Id == id);
            if (actorInDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                actorInDb.FullName = actor.FullName;
                actorInDb.Bio = actor.Bio;
                actorInDb.UpdatedAt = DateTime.Now;

                if (img != null)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/actors");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    if (!string.IsNullOrEmpty(actorInDb.ImgUrl))
                    {
                        var oldPath = Path.Combine(folder, actorInDb.ImgUrl);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine(folder, fileName);
                    using var stream = System.IO.File.Create(filePath);
                    await img.CopyToAsync(stream);
                    actorInDb.  ImgUrl = fileName;
                }

                await _actorRepository.UpdateAsync(actorInDb);
                TempData["Success"] = "Actor updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // 🗑 Delete
        public async Task<IActionResult> Delete(int id)
        {
            var actor = await _actorRepository.GetOneAsync(a => a.Id == id);
            if (actor == null) return NotFound();
            return View(actor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _actorRepository.DeleteAsync(id);
            TempData["Success"] = "Actor deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

using Cinema.Models;
using Cinema.Repositories.IRepositories;
using Cinema.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
    public class PromotionsController : Controller
    {
        private readonly IRepository<Promotion> _promotionRepo;
        private readonly IRepository<Movie> _movieRepo;

        public PromotionsController(IRepository<Promotion> promotionRepo, IRepository<Movie> movieRepo)
        {
            _promotionRepo = promotionRepo;
            _movieRepo = movieRepo;
        }

        public async Task<IActionResult> Index()
        {
            var promotions = await _promotionRepo.GetAsync(includeProperties: "Movie");
            return View(promotions);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Movies = await _movieRepo.GetAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Promotion model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Movies = await _movieRepo.GetAsync();
                return View(model);
            }

            await _promotionRepo.CreateAsync(model);
            await _promotionRepo.CommitAsync();

            TempData["success-notification"] = "Promotion created!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var promo = await _promotionRepo.GetOneAsync(p => p.Id == id);
            if (promo == null) return NotFound();

            ViewBag.Movies = await _movieRepo.GetAsync();
            return View(promo);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Promotion model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Movies = await _movieRepo.GetAsync();
                return View(model);
            }

            var promo = await _promotionRepo.GetOneAsync(p => p.Id == model.Id);
            if (promo == null) return NotFound();

            promo.Code = model.Code;
            promo.Discount = model.Discount;
            promo.ValidTo = model.ValidTo;
            promo.MaxUsage = model.MaxUsage;
            promo.MovieId = model.MovieId;
            promo.IsValid = model.IsValid;

            await _promotionRepo.CommitAsync();

            TempData["success-notification"] = "Promotion updated!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var promo = await _promotionRepo.GetOneAsync(p => p.Id == id);
            if (promo == null) return NotFound();

            await _promotionRepo.DeleteAsync(id);
            await _promotionRepo.CommitAsync();

            TempData["success-notification"] = "Promotion deleted!";
            return RedirectToAction("Index");
        }

    }
}

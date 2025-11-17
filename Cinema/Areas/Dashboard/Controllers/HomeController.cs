using Cinema.Data;
using Cinema.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.USER_ROLE}")]

    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewBag.MovieCount = await _db.Movies.CountAsync();
            ViewBag.ActorCount = await _db.Actors.CountAsync();
            ViewBag.CinemaCount = await _db.Cinemas.CountAsync();
            ViewBag.CategoryCount = await _db.Categories.CountAsync();

            var latestMovies = await _db.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .OrderByDescending(m => m.DateTime)
                .Take(5)
                .ToListAsync();

            return View(latestMovies ?? new List<Models.Movie>());
        }
    }
}

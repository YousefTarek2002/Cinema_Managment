using Cinema.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) => _db = db;

        public IActionResult Index()
        {
            ViewBag.MovieCount = _db.Movies.Count();
            ViewBag.ActorCount = _db.Actors.Count();
            ViewBag.CinemaCount = _db.Cinemas.Count();
            ViewBag.CategoryCount = _db.Categories.Count();

            var latestMovies = _db.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .OrderByDescending(m => m.DateTime)
                .Take(5)
                .ToList();

            return View(latestMovies);
        }
    }
}

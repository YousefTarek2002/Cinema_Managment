using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cinema.Data;
using Cinema.Models;

namespace Cinema.Areas.Actors.Controllers
{
    [Area("Actors")]
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ActorsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public IActionResult Index(string? searchName, int? minMovies, int? maxMovies)
        {
            var actors = _db.Actors
                .Include(a => a.MovieActors)
                .ThenInclude(ma => ma.Movie)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
                actors = actors.Where(a => a.FullName.Contains(searchName));

            if (minMovies.HasValue)
                actors = actors.Where(a => a.MovieActors.Count >= minMovies.Value);

            if (maxMovies.HasValue)
                actors = actors.Where(a => a.MovieActors.Count <= maxMovies.Value);

            ViewBag.SearchName = searchName;
            ViewBag.MinMovies = minMovies;
            ViewBag.MaxMovies = maxMovies;

            return View(actors.ToList());
        }


        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var actor = _db.Actors
                .Include(a => a.MovieActors)
                .ThenInclude(ma => ma.Movie)
                .FirstOrDefault(a => a.Id == id);

            if (actor == null) return NotFound();

            return View(actor);
        }

        public IActionResult Create()
        {
            ViewBag.Movies = _db.Movies.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Actor actor, IFormFile? img, List<int>? SelectedMovies)
        {
            if (ModelState.IsValid)
            {
                if (img != null && img.Length > 0)
                {
                    string folder = Path.Combine(_env.WebRootPath, "images/actors");
                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    string path = Path.Combine(folder, fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    img.CopyTo(stream);

                    actor.ImgUrl = "/images/actors/" + fileName;
                }

                _db.Actors.Add(actor);
                _db.SaveChanges();

                if (SelectedMovies != null && SelectedMovies.Count > 0)
                {
                    foreach (var movieId in SelectedMovies)
                    {
                        _db.MovieActors.Add(new MovieActor
                        {
                            ActorId = actor.Id,
                            MovieId = movieId
                        });
                    }
                    _db.SaveChanges();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Movies = _db.Movies.ToList();
            return View(actor);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var actor = _db.Actors
                .Include(a => a.MovieActors)
                .FirstOrDefault(a => a.Id == id);

            if (actor == null) return NotFound();

            ViewBag.Movies = _db.Movies.ToList();
            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Actor actor, IFormFile? img, List<int>? SelectedMovies)
        {
            if (id != actor.Id) return NotFound();

            var actorInDb = _db.Actors
                .Include(a => a.MovieActors)
                .FirstOrDefault(a => a.Id == id);

            if (actorInDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                actorInDb.FullName = actor.FullName;
                actorInDb.Bio = actor.Bio;

                if (img != null && img.Length > 0)
                {
                    string folder = Path.Combine(_env.WebRootPath, "images/actors");
                    Directory.CreateDirectory(folder);

                    if (!string.IsNullOrEmpty(actorInDb.ImgUrl))
                    {
                        string oldPath = Path.Combine(_env.WebRootPath, actorInDb.ImgUrl.TrimStart('/').Replace("/", "\\"));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    string fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    string path = Path.Combine(folder, fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    img.CopyTo(stream);
                    actorInDb.ImgUrl = "/images/actors/" + fileName;
                }

                var existingMovies = actorInDb.MovieActors.ToList();
                foreach (var ma in existingMovies)
                    _db.MovieActors.Remove(ma);

                if (SelectedMovies != null && SelectedMovies.Count > 0)
                {
                    foreach (var movieId in SelectedMovies)
                    {
                        _db.MovieActors.Add(new MovieActor
                        {
                            ActorId = actorInDb.Id,
                            MovieId = movieId
                        });
                    }
                }

                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Movies = _db.Movies.ToList();
            return View(actorInDb);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var actor = _db.Actors.Find(id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var actor = _db.Actors
                .Include(a => a.MovieActors)
                .FirstOrDefault(a => a.Id == id);

            if (actor != null)
            {
                if (!string.IsNullOrEmpty(actor.ImgUrl))
                {
                    string path = Path.Combine(_env.WebRootPath, actor.ImgUrl.TrimStart('/').Replace("/", "\\"));
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }

                foreach (var ma in actor.MovieActors.ToList())
                    _db.MovieActors.Remove(ma);

                _db.Actors.Remove(actor);
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

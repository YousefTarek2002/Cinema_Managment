using Cinema.Data;
using Cinema.Models;
using Cinema.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Repositories
{
    public class CinemaRepository : Repository<Cinemaa> , ICinemaRepository
    {
        private readonly ApplicationDbContext _context;
        public CinemaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Cinemaa> GetCinemasWithMovies()
        {
            return _context.Cinemas.Include(c => c.Movies);
        }
    }
}

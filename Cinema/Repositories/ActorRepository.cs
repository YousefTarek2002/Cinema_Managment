using Cinema.Data;
using Cinema.Models;
using Cinema.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Repositories
{
    public class ActorRepository : Repository<Actor> , IActorRepository
    {
        private readonly ApplicationDbContext _context;
        public ActorRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Actor> GetAllWithMovies()
        {
            return _context.Actors
                .Include(a => a.MovieActors)
                .ThenInclude(ma => ma.Movie);
        }
    }
}

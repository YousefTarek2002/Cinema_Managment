using Cinema.Models;

namespace Cinema.Repositories.IRepositories
{
    public interface IActorRepository : IRepository<Actor>
    {
        IQueryable<Actor> GetAllWithMovies();
    }
}

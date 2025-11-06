using Cinema.Models;

namespace Cinema.Repositories.IRepositories
{
    public interface ICinemaRepository : IRepository<Cinemaa>
    {
        IQueryable<Cinemaa> GetCinemasWithMovies();
    }
}

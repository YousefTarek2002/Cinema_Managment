using Cinema.Models;

namespace Cinema.Repositories.IRepositories
{
    public interface IMovieRepository : IRepository<Movie>
    {
        IQueryable<Movie> GetFullMovies();
    }
}

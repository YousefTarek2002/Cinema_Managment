using Cinema.Repositories.IRepositories;

public interface IUnitOfWork
{
    IActorRepository Actor { get; }
    IMovieRepository Movie { get; }
    ICinemaRepository Cinema { get; }
    ICategoryRepository Category { get; }
    Task SaveAsync();
}

using Cinema.Data;
using Cinema.Repositories;
using Cinema.Repositories.IRepositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    public IActorRepository Actor { get; }
    public IMovieRepository Movie { get; }
    public ICinemaRepository Cinema { get; }
    public ICategoryRepository Category { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Actor = new ActorRepository(_context);
        Movie = new MovieRepository(_context);
        Cinema = new CinemaRepository(_context);
        Category = new CategoryRepository(_context);
    }

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}

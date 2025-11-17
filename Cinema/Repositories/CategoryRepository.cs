using Cinema.Data;
using Cinema.Models;
using Cinema.Repositories.IRepositories;

namespace Cinema.Repositories
{
    public class CategoryRepository : Repository<Category> , ICategoryRepository
    { 
        public CategoryRepository(ApplicationDbContext context) : base(context) { }
    }
}

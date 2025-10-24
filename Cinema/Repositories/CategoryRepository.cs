using Cinema.Data;
using Cinema.Models;

namespace Cinema.Repositories
{
    public class CategoryRepository : Repository<Category>
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }
    }
}

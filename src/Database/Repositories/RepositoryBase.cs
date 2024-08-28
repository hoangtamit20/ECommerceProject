using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class RepositoryBase<TContext> : IRepositoryBase
        where TContext : DbContext
    {
        private readonly TContext _context;

        public RepositoryBase(TContext context)
        {
            _context = context;
        }


    }
}
using Core.Domain.Interfaces;
using Database.DbContexts;

namespace Database.Repositories
{
    internal class CETRepository : RepositoryBase<CETDbContext>, ICETRepository
    {
        public CETRepository(CETDbContext context) : base(context)
        {
        }
    }
}
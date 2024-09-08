using Core.Domain;
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
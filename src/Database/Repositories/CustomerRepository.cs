using Core.Domain.Interfaces;
using Database.DbContexts;

namespace Database.Repositories
{
    internal class CustomerRepository : RepositoryBase<CustomerDbContext>, ICustomerRepository
    {
        public CustomerRepository(CustomerDbContext context) : base(context)
        {
        }
    }
}
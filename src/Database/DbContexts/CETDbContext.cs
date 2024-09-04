using Core.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Database.DbContexts
{
    internal sealed class CETDbContext : IdentityDbContext<UserEntity, RoleEntity, string>
    {
        public CETDbContext(DbContextOptions<CETDbContext> options) : base(options)
        {
        }

        public DbSet<CustomerEntity> Customers { get; set; }
        public DbSet<LinkHelperEntity> LinkHelpers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Model.GetEntityTypes().ToList().ForEach(entityType => 
            {
                if (entityType.GetTableName()!.StartsWith("AspNet"))
                {
                    entityType.SetTableName($"CET_{entityType.GetTableName()?.Substring(6).TrimEnd('s')}");
                }
            });
        }
    }
}
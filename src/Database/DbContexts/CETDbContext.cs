using Core.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Database.DbContexts
{
    internal class CETDbContext : IdentityDbContext<UserEntity>
    {
        public CETDbContext(DbContextOptions<CETDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Model.GetEntityTypes().ToList().ForEach(entityType => 
            {
                if (entityType.GetTableName()!.StartsWith("AspNet"))
                {
                    entityType.SetTableName($"CET_{entityType.GetTableName()?.Substring(6)}");
                }
            });
        }
    }
}
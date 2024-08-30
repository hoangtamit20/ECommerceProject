using Database.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Database;

public static class DependencyInjection
{
    public static IServiceCollection AddCETDbContext(this IServiceCollection services)
    {
        services.AddDbContext<CETDbContext>(optionsAction: opt => 
        {
            opt.UseSqlServer(connectionString: "");
        });

        return services;
    }
}

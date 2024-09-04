using Core.Domain;
using Core.Domain.Interfaces;
using Database.DbContexts;
using Database.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Database;

public static class DependencyInjection
{
    public static IServiceCollection AddCETDbContext(this IServiceCollection services)
    {
        services.AddDbContext<CETDbContext>(optionsAction: opt => 
        {
            opt.UseSqlServer(connectionString: RuntimeContext.AppSettings.ConnectionStrings.CET_Connection);
        });

        services.AddDataProtection();
        
        services.AddIdentityCore<UserEntity>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddRoles<RoleEntity>()
        .AddEntityFrameworkStores<CETDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<UserManager<UserEntity>>();
        services.AddScoped<RoleManager<RoleEntity>>();
        services.AddScoped<SignInManager<UserEntity>>();

        services.AddScoped<ICETRepository, CETRepository>();

        return services;
    }

    public static IServiceCollection AddCustomerDbContext(this IServiceCollection services)
    {
        services.AddDbContext<CustomerDbContext>(optionsAction: opt =>
        {
            opt.UseSqlServer(connectionString: RuntimeContext.AppSettings.ConnectionStrings.CUS_Connection);
        });
        return services;
    }
}

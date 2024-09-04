using Core.Domain;
using Database;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }
    public static IServiceCollection AddCETCoreService(this IServiceCollection services)
    {
        services.AddCETDbContext();
        return services;
    }

    public static IServiceCollection AddCustomerCoreService(this IServiceCollection services)
    {
        services.AddCustomerDbContext();
        return services;
    }

    public static IServiceCollection AddMailerSendService(this IServiceCollection services)
    {
        
        return services;
    }
}

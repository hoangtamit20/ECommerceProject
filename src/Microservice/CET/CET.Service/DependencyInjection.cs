using CET.Domain;
using Core.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CET.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddCETServices(this IServiceCollection services)
    {
        services.AddCETCoreService();
        services.AddCoreServices();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}

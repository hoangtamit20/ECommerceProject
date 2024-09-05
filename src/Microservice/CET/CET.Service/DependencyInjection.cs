using CET.Domain;
using Core.Service;
using Microsoft.Extensions.DependencyInjection;

namespace CET.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddCETServices(this IServiceCollection services)
    {
        services.AddCETCoreService();
        services.AddCoreServices();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        return services;
    }
}

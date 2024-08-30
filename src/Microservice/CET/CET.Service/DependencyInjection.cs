using Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CET.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddCETServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Tạo IConfigurationBuilder và thêm tệp JSON cấu hình
        var configBuilder = new ConfigurationBuilder()
            .AddConfiguration(configuration) // Thêm cấu hình hiện tại
            .AddJsonFile("config.Development.json", optional: true, reloadOnChange: true); // Thêm tệp JSON

        // Xây dựng cấu hình
        var builtConfig = configBuilder.Build();
        services.AddCETDbContext();
        return services;
    }
}

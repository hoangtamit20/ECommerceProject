using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Core.Domain;

public static class RuntimeContext
{
    private static IHttpContextAccessor? _httpContextAccessor;
    public static AppSettings AppSettings { get; private set; }

    static RuntimeContext()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(Path.Combine(DirectoryHelper.GetSolutionBasePath(), "config.Development.json"), optional: true, reloadOnChange: true);

        var configuration = builder.Build();
        AppSettings = new AppSettings();
        configuration.GetSection("AppSettings").Bind(AppSettings);
    }

    public static string? CurrentIpAddress
    {
        get
        {
            if (_httpContextAccessor!= null && _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress != null)
            {
                return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            return null;
        }
    }
}
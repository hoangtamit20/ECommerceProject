using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Domain
{
    public static class RuntimeContext
    {
        public static AppSettings AppSettings { get; private set; }
        private static AsyncLocal<UserEntity?> _currentUser = new AsyncLocal<UserEntity?>();
        private static AsyncLocal<string?> _currentAccessToken = new AsyncLocal<string?>();
        private static AsyncLocal<string?> _currentUserId = new AsyncLocal<string?>();
        private static AsyncLocal<string?> _endpoint = new AsyncLocal<string?>();
        public static IServiceProvider? ServiceProvider { get; set; }


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
                var httpContextAccessor = ServiceProvider?.GetRequiredService<IHttpContextAccessor>();
                var ipAddress = httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                return ipAddress;
            }
        }

        public static UserEntity? CurrentUser
        {
            get
            {
                return _currentUser.Value;
            }
            set
            {
                _currentUser.Value = value;
            }
        }

        public static string? CurrentUserId
        {
            get
            {
                return _currentUserId.Value;
            }
            set
            {
                _currentUserId.Value = value;
            }
        }

        public static string? Endpoint 
        {
           get
            {
                return _endpoint.Value;
            }
            set
            {
                _endpoint.Value = value;
            }
        }

        public static string? CurrentAccessToken
        {
            get
            {
                return _currentAccessToken.Value;
            }
            set
            {
                _currentAccessToken.Value = value;
            }
        }
    }
}
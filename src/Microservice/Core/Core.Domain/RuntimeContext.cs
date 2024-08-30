using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Core.Domain;

public static class RuntimeContext
{
    private static IHttpContextAccessor? _httpContextAccessor;
    private static UserManager<UserEntity>? _userManager;
    private static ILogger? _logger;
    private static bool _isSimulated;

    public static void Configure(
        IHttpContextAccessor httpContextAccessor,
        UserManager<UserEntity> userManager,
        ILogger logger,
        bool isSimulated = false)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _isSimulated = isSimulated;
        _logger = logger;
    }

    public static UserEntity? CurrentUser
    {
        get
        {
            if (_userManager != null && _httpContextAccessor != null)
                return _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User).Result;
            return null;
        }
    }

    public static bool IsSimulated
    {
        get => _isSimulated;
        set => _isSimulated = value;
    }

    public static ILogger Logger
    {
        get
        {
            if (_logger == null)
            {
                throw new InvalidOperationException("Logger has not been configured.");
            }
            return _logger;
        }
    }
}
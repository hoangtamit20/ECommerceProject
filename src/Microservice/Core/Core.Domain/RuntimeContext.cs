using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Core.Domain;

public static class RuntimeContext
{
    private static IHttpContextAccessor? _httpContextAccessor;
    private static UserManager<UserEntity>? _userManager;

    public static void Configure(IHttpContextAccessor httpContextAccessor, UserManager<UserEntity> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
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
}
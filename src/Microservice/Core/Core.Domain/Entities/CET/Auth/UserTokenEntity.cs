using Microsoft.AspNetCore.Identity;

namespace Core.Domain.Entities.CET.Auth
{
    public class UserTokenEntity : IdentityUserToken<string>
    {
        public CTokenType Type { get; set; }
        public DateTimeOffset TokenExpiration { get; set; }
        public bool IsTokenInvoked { get; set; } = false;


    }

    #region Token info property
    public class TokenProperty
    {
        public string Token { get; set; } = string.Empty;
        public CTokenType TokenType { get; set; }
        public bool IsTokenInvoked { get; set; } = false;
        public DateTimeOffset TokenExpiration { get; set; }
    }
    #endregion Token info property
}
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain
{
    [Table(name: "CET_UserRefreshToken")]
    public class UserRefreshTokenEntity
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string RefreshToken { get; private set; } = Guid.NewGuid().ToString();
        public string AccessToken { get; set; } = string.Empty;
        public DateTimeOffset ExpireTime { get; set; }
        public bool Active => DateTimeOffset.UtcNow <= ExpireTime;
        public DateTimeOffset LastRevoked { get; set; }
        public bool IsRevoked { get; set; } = false;
        public string RemoteIpAddress { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        [InverseProperty("UserRefreshTokens")]
        public virtual UserEntity User { get; set; } = null!;
    }
}
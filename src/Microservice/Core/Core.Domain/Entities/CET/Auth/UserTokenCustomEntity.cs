using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain
{
    [Table(name: "CET_UserTokenCustom")]
    public class UserTokenCustomEntity
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string Token { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public CTokenType Type { get; set; }
        public CTokenProviderType TokenProviderType { get; set; }
        public string TokenProviderName { get; set; } = string.Empty;
        public DateTimeOffset TokenExpiration { get; set; }
        public bool IsTokenInvoked { get; set; } = false;
        private DateTimeOffset _createdDate = DateTimeOffset.UtcNow;
        public DateTimeOffset CreatedDate
        {
            get => _createdDate.ToLocalTime();
            private set => _createdDate = value;
        }
        public string UserId { get; set; } = string.Empty;
        [ForeignKey(name: "UserId")]
        [InverseProperty(property: "UserTokenCustoms")]
        public virtual UserEntity User { get; set; } = null!;
    }
}
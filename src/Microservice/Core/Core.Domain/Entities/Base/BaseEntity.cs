namespace Core.Domain
{
    public class BaseEntity
    {
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTimeOffset? ModifiedDate { get; set; }
        public DateTimeOffset CreatedDate { get; private set; } = DateTimeOffset.UtcNow;
    }
}
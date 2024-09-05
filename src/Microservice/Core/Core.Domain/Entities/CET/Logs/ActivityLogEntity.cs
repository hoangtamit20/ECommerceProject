using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain
{
    [Table(name: "CET_ActivityLog")]
    public class ActivityLogEntity
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string UserName { get; private set; } = RuntimeContext.CurrentUser?.FullName ?? string.Empty;
        public string Email { get; private set; } = RuntimeContext.CurrentUser?.Email ?? string.Empty;
        public CActivityLogType ActionType { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string IpAddress { get; private set; } = RuntimeContext.CurrentIpAddress ?? string.Empty;
        private DateTimeOffset _createdDate = DateTimeOffset.UtcNow;
        public DateTimeOffset CreatedDate
        {
            get => _createdDate.ToLocalTime();
            private set => _createdDate = value;
        }
    }
}
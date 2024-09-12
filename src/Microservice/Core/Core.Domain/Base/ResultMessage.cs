namespace Core.Domain
{
    public class ResultMessage
    {
        public CNotificationLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public CNotificationType NotificationType { get; set; }
    }
}
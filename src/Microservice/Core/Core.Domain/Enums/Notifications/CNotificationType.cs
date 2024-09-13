using System.ComponentModel;

namespace Core.Domain
{
    public enum CNotificationType
    {
        None = 0,
        Normal = 1,
        [Description(description: "Send email")]
        Email = 2,
        Order = 3,
        [Description(description: "Registration account")]
        Register = 4,
    }

    public enum CNotificationLevel
    {
        None = 0,
        [Description(description: "")]
        Info = 1,
        [Description(description: "")]
        Warning = 2,
        [Description(description: "Falied")]
        Error = 3,
        [Description(description: "Successfully")]
        Success = 4
    }
}
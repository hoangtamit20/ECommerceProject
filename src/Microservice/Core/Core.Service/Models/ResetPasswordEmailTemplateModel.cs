namespace Core.Service.Models
{
    public class ResetPasswordEmailTemplateModel : BaseEmailTemplateModel
    {
        public string ConfirmationLink { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ExpiredTime { get; private set; } = DateTimeOffset.UtcNow.AddMinutes(5).ToLocalTime().ToString();
    }
}
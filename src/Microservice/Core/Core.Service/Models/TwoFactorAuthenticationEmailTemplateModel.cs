namespace Core.Service
{
    public class TwoFactorAuthenticationEmailTemplateModel : BaseEmailTemplateModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpiredTime { get; private set; } = DateTimeOffset.UtcNow.AddMinutes(5).ToLocalTime();
    }
}
namespace Core.Domain
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlTemplate,
            string? fileTemplateName = null,
            object? replaceProperty = null,
            CEmailProviderType emailProviderType = CEmailProviderType.Gmail);
    }
}
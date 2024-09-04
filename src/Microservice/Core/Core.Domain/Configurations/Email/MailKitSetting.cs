using Core.Domain.Configurations.Email;

namespace Core.Domain
{
    public class MailKitSetting
    {
        public GmailSmtp GmailSmtp { get; set; } = null!;
        public OutlookSmtp OutlookSmtp { get; set; } = null!;
        public ProtonMailSmtp ProtonMailSmtp { get; set; } = null!;
        public BrevoSmtp BrevoSmtp { get; set; } = null!;
        public ElasticeSmtp ElasticeSmtp { get; set; } = null!;
    }
}
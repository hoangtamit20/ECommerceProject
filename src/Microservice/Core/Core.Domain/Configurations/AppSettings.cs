namespace Core.Domain
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
        public ClientApp ClientApp { get; set; } = null!;
        public EmailSetting EmailSetting { get; set; } = null!;
    }
}
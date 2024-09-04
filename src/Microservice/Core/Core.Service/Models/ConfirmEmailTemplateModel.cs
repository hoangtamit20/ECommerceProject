namespace Core.Service.Models
{
    public class ConfirmEmailTemplateModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string YourCompany { get; set; } = string.Empty;
        public string ReceiverEmail { get; set; } = string.Empty;
        public string ConfirmationLink { get; set; } = string.Empty;
    }
}
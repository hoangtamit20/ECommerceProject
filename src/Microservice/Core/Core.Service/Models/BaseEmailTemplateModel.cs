using Core.Domain;

namespace Core.Service
{
    public class BaseEmailTemplateModel : ClientApp
    {
        public string ReceiverEmail { get; set; } = string.Empty;
    }
}
using Core.Domain;

namespace Blazor.WebApp.Models
{
    public class TwoFactorModel : ConfirmTwoFactorAuthenticationRequestDto
    {
        public string Digit1 { get; set; } = string.Empty;
        public string Digit2 { get; set; } = string.Empty;
        public string Digit3 { get; set; } = string.Empty;
        public string Digit4 { get; set; } = string.Empty;
        public string Digit5 { get; set; } = string.Empty;
        public string Digit6 { get; set; } = string.Empty;
        public string GetFullCode()
        {
            return $"{Digit1}{Digit2}{Digit3}{Digit4}{Digit5}{Digit6}";
        }
    }
}
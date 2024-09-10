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

        public string GetDigit(int index)
        {
            return index switch
            {
                1 => Digit1,
                2 => Digit2,
                3 => Digit3,
                4 => Digit4,
                5 => Digit5,
                6 => Digit6,
                _ => string.Empty
            };
        }

        public void UpdateDigit(int index, char value)
        {
            switch (index)
            {
                case 1: Digit1 = value.ToString(); break;
                case 2: Digit2 = value.ToString(); break;
                case 3: Digit3 = value.ToString(); break;
                case 4: Digit4 = value.ToString(); break;
                case 5: Digit5 = value.ToString(); break;
                case 6: Digit6 = value.ToString(); break;
            }
        }

        public string GetFullCode()
        {
            return $"{Digit1}{Digit2}{Digit3}{Digit4}{Digit5}{Digit6}";
        }
    }
}
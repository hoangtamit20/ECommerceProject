namespace Core.Domain
{
    public class ConfirmTwoFactorAuthenticationRequestDto
    {
        [EmailFormat(ErrorMessage = "{0} is invalid email format.")]
        public string Email { get; set; } = string.Empty;
        [SixDigitCode(ErrorMessage = "Code must be exactly 6 digits.")]
        public string Code { get; set; } = string.Empty;
    }
}
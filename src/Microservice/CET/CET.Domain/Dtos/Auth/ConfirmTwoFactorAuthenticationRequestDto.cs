namespace CET.Domain
{
    public class ConfirmTwoFactorAuthenticationRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
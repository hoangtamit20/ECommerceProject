namespace Core.Domain
{
    public class LoginResponseDto
    {
        public bool TwoFactorEnabled { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;

    }
}
namespace Core.Domain
{
    public class CreateUserResponseDto
    {
        public string Email { get; set; } = string.Empty;
        public bool NeedEmailConfirm { get; set; } = false;
        public string Message { get; set; } = string.Empty;
    }
}
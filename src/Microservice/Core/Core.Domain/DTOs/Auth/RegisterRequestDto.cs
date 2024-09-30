using System.ComponentModel.DataAnnotations;
using Core.Domain;

namespace Core.Domain
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "{0} is required.")]
        public string FullName { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} is required.")]
        [EmailAddress(ErrorMessage = "{0} is invalid email format.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} is required.")]
        [PasswordValidation(MinimumLength = 8, MaximumLength = 50)]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} is required.")]
        [PasswordValidation(MinimumLength = 8, MaximumLength = 50)]
        [ComparePasswords(nameof(RegisterRequestDto.Password), ErrorMessage = "Password and ConfirmPassword do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
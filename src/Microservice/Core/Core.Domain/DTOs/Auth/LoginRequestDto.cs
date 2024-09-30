using System.ComponentModel.DataAnnotations;
using Core.Domain;

namespace Core.Domain
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "{0} is required")]
        [EmailAddress(ErrorMessage = "{0} is invalid email format.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} is required")]
        [PasswordValidation(MinimumLength = 8, MaximumLength = 50)]
        public string Password { get; set; } = string.Empty;
    }
}
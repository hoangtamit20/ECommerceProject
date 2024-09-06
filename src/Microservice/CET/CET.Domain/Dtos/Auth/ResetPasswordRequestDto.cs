using System.ComponentModel.DataAnnotations;
using Core.Domain;

namespace CET.Domain
{
    public class ResetPasswordRequestDto
    {
        [EmailAddress(ErrorMessage = "{0} is invalid Email format.")]
        public string Email { get; set; } = string.Empty;
    }

    public class ConfirmResetPasswordRequestDto
    {
        [GuidFormat(ErrorMessage = "{0} is invalid Guid format.")]
        public string UserId { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} is required")]
        public string Token { get; set; } = string.Empty;
        [PasswordValidation]
        public string NewPassword { get; set; } = string.Empty;
        [PasswordValidation]
        [ComparePasswords(comparisonProperty: nameof(ConfirmResetPasswordRequestDto.NewPassword))]
        public string NewPasswordConfirm { get; set; } = string.Empty;
    }
}
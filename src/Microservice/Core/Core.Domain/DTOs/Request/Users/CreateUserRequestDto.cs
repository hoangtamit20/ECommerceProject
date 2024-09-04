using System.ComponentModel.DataAnnotations;
using Core.Domain.Enums.Roles;

namespace Core.Domain
{
    public class CreateUserRequestDto
    {
        [EmailAddress(ErrorMessage = "{0} is invalid email format.")]
        [Required(ErrorMessage = "{0} is required.")]
        [Length(minimumLength: 8, maximumLength: 50, ErrorMessage = "{0} must be beetween {1} and {2} characters.")]
        public string Email { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string UserName { get; set; } = string.Empty;
        [PasswordValidation(MinimumLength = 8, MaximumLength = 20)]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} is required.")]
        [Length(minimumLength: 4, maximumLength: 200, ErrorMessage = "{0} must be beetween {1} and {2} characters.")]
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public string CountryId { get; set; } = string.Empty;
        public string ProvinceId { get; set; } = string.Empty;
        public string DistrictId { get; set; } = string.Empty;
        public string WardId { get; set; } = string.Empty;
        public List<CRoleType>? Roles { get; set; } = null;
    }
}
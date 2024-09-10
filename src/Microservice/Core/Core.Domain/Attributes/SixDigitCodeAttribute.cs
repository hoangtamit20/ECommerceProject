using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Core.Domain
{
    public class SixDigitCodeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult("Code is required.");
        }

        var code = value.ToString() ?? string.Empty;
        var regex = new Regex(@"^\d{6}$");

        if (!regex.IsMatch(code))
        {
            return new ValidationResult("Code must be exactly 6 digits.");
        }

        return ValidationResult.Success;
    }
}
}
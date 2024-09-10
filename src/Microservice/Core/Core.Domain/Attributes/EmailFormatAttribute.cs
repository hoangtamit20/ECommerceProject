using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Core.Domain
{
    public class EmailFormatAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult("Email address is required.");
        }

        string email = value.ToString() ?? string.Empty;
        var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        if (!regex.IsMatch(email))
        {
            return new ValidationResult("Invalid email address format.");
        }

        return ValidationResult.Success;
    }
}
}
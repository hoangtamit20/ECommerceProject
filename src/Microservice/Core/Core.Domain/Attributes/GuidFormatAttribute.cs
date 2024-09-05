using System.ComponentModel.DataAnnotations;

namespace Core.Domain
{
    public class GuidFormatAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string stringValue && Guid.TryParse(stringValue, out _))
        {
            return ValidationResult.Success;
        }
        return new ValidationResult("The field must be a valid GUID.");
    }
}
}
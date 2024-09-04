using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Core.Domain
{
    // public class PasswordValidationAttribute : ValidationAttribute
    // {
    //     protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    //     {
    //         var password = value as string;
    //         if (string.IsNullOrEmpty(password))
    //         {
    //             return new ValidationResult("Password is required.");
    //         }

    //         // Example: Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character.
    //         var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
    //         if (!regex.IsMatch(password))
    //         {
    //             return new ValidationResult("Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
    //         }

    //         return ValidationResult.Success;
    //     }
    // }

    public class PasswordValidationAttribute : ValidationAttribute
    {
        public int MinimumLength { get; set; } = 8;
        public int MaximumLength { get; set; } = 50;
        public new string ErrorMessage { get; set; } = "Password must be between {0} and {1} characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character.";

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var password = value as string;
            if (string.IsNullOrEmpty(password))
            {
                return new ValidationResult("Password is required.");
            }

            if (password.Length < MinimumLength || password.Length > MaximumLength)
            {
                return new ValidationResult(string.Format(ErrorMessage, MinimumLength, MaximumLength));
            }

            var regex = new Regex($@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{{{MinimumLength},{MaximumLength}}}$");
            if (!regex.IsMatch(password))
            {
                return new ValidationResult("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
            }

            return ValidationResult.Success;
        }
    }
}
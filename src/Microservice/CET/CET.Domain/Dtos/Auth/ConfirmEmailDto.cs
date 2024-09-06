using System.ComponentModel.DataAnnotations;
using Core.Domain;

namespace CET.Domain
{
    public class ConfirmEmailDto
    {
        [Required(ErrorMessage = "{0} is required.")]
        [GuidFormat]
        public string UserId { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} is required.")]
        public string Token { get; set; } = string.Empty;
    }

    
}
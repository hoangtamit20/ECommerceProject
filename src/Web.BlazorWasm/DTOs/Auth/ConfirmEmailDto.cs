using System.ComponentModel.DataAnnotations;

namespace Core.Domain
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
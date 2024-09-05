
using System.ComponentModel.DataAnnotations;
using Core.Domain;

namespace CET.Domain
{
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "{0} is required.")]
        [GuidFormat]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
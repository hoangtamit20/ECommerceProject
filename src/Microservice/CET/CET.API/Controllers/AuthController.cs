using CET.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CET.API.Controllers
{
    [ApiController]
    [Route("/api/v1/cet/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }


        [HttpPost("systemlogin")]
        public async Task<IActionResult> SystemLogin(LoginRequestDto loginDto)
        {
            var result = await _authService.SystemLoginAsync(loginDto: loginDto, modelState: ModelState);
            return StatusCode(statusCode: result.StatusCode, value: result.Result);
        }

        [HttpPost("confirmtwofactor")]
        public async Task<IActionResult> ConfirmTwoFactor(ConfirmTwoFactorAuthenticationRequestDto twoFactorDto)
        {
            var result = await _authService.ConfirmTwoFactorAuthenticationAsync(twoFactorDto: twoFactorDto, modelState: ModelState);
            return StatusCode(statusCode: result.StatusCode, value: result.Result);
        }
    }
}
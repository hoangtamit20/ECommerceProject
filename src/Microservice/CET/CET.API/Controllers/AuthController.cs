using CET.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CET.API.Controllers
{
    [ApiController]
    [Route("/api/v1/cet/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, IUserService userService)
        {
            _authService = authService;
            _logger = logger;
            _userService = userService;
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

        [HttpPost("refreshtoken")]
        [Authorize]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto refreshTokenDto)
        {
            var result = await _authService.RefreshTokenAsync(refreshTokenDto: refreshTokenDto, modelState: ModelState);
            return StatusCode(statusCode: result.StatusCode, value: result.Result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto registerRequestDto)
        {
            var result = await _authService.RegisterAsync(registerDto: registerRequestDto, modelState: ModelState);
            return StatusCode(result.StatusCode, value: result.Result);
        }

        [HttpGet("emailconfirm")]
        public async Task<IActionResult> ConfirmEmailRegistration([FromQuery]ConfirmEmailDto confirmEmailDto)
        {
            var result = await _userService.ConfirmedEmailAsync(confirmEmailDto: confirmEmailDto, modelState: ModelState);
            return StatusCode(statusCode: result.StatusCode, value: result.Result);
        }
    }
}
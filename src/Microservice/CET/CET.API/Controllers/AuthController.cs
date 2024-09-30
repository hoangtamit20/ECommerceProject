using Core.Domain;
using CET.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpGet("authentication")]
        [Authorize]
        public async Task<IActionResult> IsAuthentication()
        {
            return Ok(await Task.FromResult(new ResponseResult<string>()
            {
                Data = $"You are already authentication.",
                Success = true
            }));
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
        public async Task<IActionResult> ConfirmEmailRegistration([FromQuery] ConfirmEmailDto confirmEmailDto)
        {
            var clientEndpoint = RuntimeContext.AppSettings.ClientApp.ClientEndpoint;
            var resultMessage = await _authService.ConfirmRegisterAsync(confirmEmailDto: confirmEmailDto, modelState: ModelState);
            var queryString = LinkHelper.ToQueryString<ResultMessage>(obj: resultMessage);
            var url = $"{clientEndpoint.TrimEnd('/')}/notification-summary/?{queryString}";
            return Redirect(url: url);
        }


        [HttpPost("requestresetpassword")]
        public async Task<IActionResult> RequestResetPassword(ResetPasswordRequestDto passwordRequestDto)
        {
            var result = await _authService.RequestResetPasswordAsync(resetPasswordDto: passwordRequestDto, modelState: ModelState);
            return StatusCode(statusCode: result.StatusCode, value: result.Result);
        }

        [HttpPost("confirmpasswordreset")]
        public async Task<IActionResult> ConfirmResetPassword(ConfirmResetPasswordRequestDto confirmResetDto)
        {
            var result = await _authService.ConfirmResetPasswordAsync(confirmDto: confirmResetDto, modelState: ModelState);
            return StatusCode(statusCode: result.StatusCode, value: result.Result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(bool areAllDevices = false)
        {
            var result = await _authService.LogOutAsync(areAllDevices: areAllDevices);
            return StatusCode(statusCode: result.StatusCode, value: result.Result);
        }
    }
}
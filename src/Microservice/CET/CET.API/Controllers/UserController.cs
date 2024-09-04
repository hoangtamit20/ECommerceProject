using CET.Domain;
using Core.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CET.API.Controllers
{
    [ApiController]
    [Route("/api/v1/cet")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("createuser")]
        public async Task<IActionResult> AddUser(CreateUserRequestDto requestDto)
        {
            try
            {
                var result = await _userService.CreateUserAsync(requestDto: requestDto, isAdminRequest: false, modelState: ModelState);
                return StatusCode(statusCode: result.StatusCode, value: result.Result);
            }
            catch (Exception ex)
            {
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError,
                    value: $"{ex.Message}");
            }
        }


        [HttpGet("createuser/emailconfirm")]
        public async Task<IActionResult> ConfirmEmailRegistration(string userId, string token)
        {
            try
            {
                var result = await _userService.ConfirmedEmailAsync(userId: userId, token: token);
                return StatusCode(statusCode: result.StatusCode, value: result.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError,
                    value: new ResponseResult<string>()
                    {
                        Success = false,
                        Errors = new List<ErrorDetail>()
                        {
                            new ErrorDetail()
                            {
                                Error = $"An error occured while confirmation email.",
                                ErrorScope = CErrorScope.Global,
                                Field = "InternalServerError"
                            }
                        }
                    });
            }

        }
    }
}
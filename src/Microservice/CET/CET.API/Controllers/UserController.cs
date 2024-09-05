using CET.Domain;
using Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CET.API.Controllers
{
    [ApiController]
    [Route("/api/v1/cet/user")]
    [Authorize(Roles = RoleDescription.Admin)]
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
    }
}
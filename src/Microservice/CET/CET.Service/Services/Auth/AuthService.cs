using CET.Domain;
using Core.Domain;
using Core.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace CET.Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<RoleEntity> _roleManager;
        private readonly ILogger<AuthService> _logger;
        private readonly ICETRepository _cetRepository;

        public AuthService(
            ICETRepository cetRepository,
            UserManager<UserEntity> userManager,
            RoleManager<RoleEntity> roleManager,
            ILogger<AuthService> logger)
        {
            _cetRepository = cetRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        #region Login
        public async Task<ApiResponse<LoginResponseDto>> SystemLoginAsync(LoginRequestDto loginDto, ModelStateDictionary? modelState = null)
        {
            var errors = ErrorHelper.GetModelStateError(modelState: modelState);
            var response = new ApiResponse<LoginResponseDto>();
            if (errors.IsNullOrEmpty())
            {
                var userExist = await _cetRepository.FindAsync<UserEntity>(us => us.Email == loginDto.Email || us.UserName == loginDto.Email);
                if (userExist == null)
                {
                    errors.Add(new ErrorDetail(){ Error = $"UserName or Email '{loginDto.Email}' doesn't exist", Field = $"{nameof(LoginRequestDto.Email)}_Error", ErrorScope = CErrorScope.Field});
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Result.Success = false;
                    response.Result.Errors = errors;
                    return response;
                }
                if (await _userManager.CheckPasswordAsync(user: userExist, password: loginDto.Password))
                {

                    errors.Add(new ErrorDetail(){ Error = $"Password not correct.", Field = $"{nameof(LoginRequestDto.Password)}_Error", ErrorScope = CErrorScope.Field});
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Result.Success = false;
                    response.Result.Errors = errors;
                    return response;
                }
                if (!userExist.EmailConfirmed)
                {
                    errors.Add(new ErrorDetail(){ 
                        Error = $"The user '{loginDto.Email}' has not yet confirmed their email. Please check your email to confirm your account.",
                        ErrorScope = CErrorScope.FormSummary});
                    response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Result.Errors = errors;
                }
                if (userExist.LockoutEnabled && userExist.LockoutEnd.HasValue 
                    && userExist.LockoutEnd.Value <= DateTimeOffset.UtcNow)
                {
                    errors.Add(new ErrorDetail()
                    {
                        Error = $"The user '{loginDto.Email}' is currently locked. Please try again after {userExist.LockoutEnd.Value.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")}",
                        ErrorScope = CErrorScope.FormSummary
                    });
                    await _userManager.
                }
            }
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Result.Success = false;
            response.Result.Errors = errors;
            return response;
        }
        #endregion Login

    }
}
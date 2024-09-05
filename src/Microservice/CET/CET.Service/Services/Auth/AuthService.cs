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
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;
        private readonly ICETRepository _cetRepository;

        public AuthService(
            ICETRepository cetRepository,
            UserManager<UserEntity> userManager,
            RoleManager<RoleEntity> roleManager,
            SignInManager<UserEntity> signInManager,
            IEmailService emailService,
            IJwtService jwtService,
            ILogger<AuthService> logger)
        {
            _cetRepository = cetRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _jwtService = jwtService;
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
                    errors.Add(new ErrorDetail() { Error = $"UserName or Email '{loginDto.Email}' doesn't exist", Field = $"{nameof(LoginRequestDto.Email)}_Error", ErrorScope = CErrorScope.Field });
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Result.Success = false;
                    response.Result.Errors = errors;
                    return response;
                }
                if (!await _userManager.CheckPasswordAsync(user: userExist, password: loginDto.Password))
                {

                    errors.Add(new ErrorDetail() { Error = $"Password not correct.", Field = $"{nameof(LoginRequestDto.Password)}_Error", ErrorScope = CErrorScope.Field });
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Result.Success = false;
                    response.Result.Errors = errors;
                    return response;
                }
                if (!userExist.EmailConfirmed)
                {
                    errors.Add(new ErrorDetail()
                    {
                        Error = $"The user '{loginDto.Email}' has not yet confirmed their email. Please check your email to confirm your account.",
                        ErrorScope = CErrorScope.FormSummary
                    });
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
                }
                if (userExist.TwoFactorEnabled)
                {
                    // send request to verify two factor.
                    try
                    {
                        var token = await _userManager.GenerateTwoFactorTokenAsync(user: userExist, tokenProvider: "Email");
                        userExist.TwoFactorTokenProperty = new TwoFactorTokenProperty()
                        {
                            IsTokenInvoked = false,
                            TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(minutes: 5),
                            TwoFactorToken = token
                        };
                        await _userManager.UpdateAsync(user: userExist);
                        await _emailService.SendEmailAsync(email: userExist.Email ?? string.Empty, subject: "TWO FACTOR AUTHENTICATION",
                            htmlTemplate: $"Your code here : {token}", emailProviderType: CEmailProviderType.Gmail);
                        response.StatusCode = StatusCodes.Status203NonAuthoritative;
                        response.Result.Data = new LoginResponseDto()
                        {
                            TwoFactorEnabled = true,
                            Message = $"The request for two factor authenticaion. Please enter the code was send to email '{userExist.Email}'. Code has expired after 5 minutes."
                        };
                        response.Result.Success = true;
                        return response;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"{ex.Message}");
                        errors.Add(new ErrorDetail()
                        {
                            Error = $"An error occured while generate token for two factor authentication and send email",
                            ErrorScope = CErrorScope.FormSummary,
                        });
                        response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        response.Result.Success = false;
                        response.Result.Errors = errors;
                        return response;
                    }
                }
                // need generate accesstoken and refreshtoken here
                var jwtTokenModel = await _jwtService.GenerateJwtTokenAsync(userEntity: userExist, ipAddress: RuntimeContext.CurrentIpAddress ?? string.Empty);
                response.StatusCode = StatusCodes.Status200OK;
                response.Result.Data = new LoginResponseDto()
                {
                    AccessToken = jwtTokenModel.AccessToken,
                    RefreshToken = jwtTokenModel.RefreshToken,
                    TwoFactorEnabled = false,
                    Message = "Login successfully"
                };
                response.Result.Success = true;
                return response;
            }
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Result.Success = false;
            response.Result.Errors = errors;
            return response;
        }
        #endregion Login


        #region Confirm two factor authentication
        public async Task<ApiResponse<LoginResponseDto>> ConfirmTwoFactorAuthenticationAsync(
            ConfirmTwoFactorAuthenticationRequestDto twoFactorDto, ModelStateDictionary? modelState = null)
        {
            var errors = ErrorHelper.GetModelStateError(modelState: modelState);
            var response = new ApiResponse<LoginResponseDto>();
            if (errors.IsNullOrEmpty())
            {
                var userExist = await _userManager.FindByEmailAsync(email: twoFactorDto.Email);
                if (userExist == null)
                {
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Result.Success = false;
                    errors.Add(new ErrorDetail()
                    {
                        Error = $"Cannot found any user with email '{twoFactorDto.Email}'",
                        ErrorScope = CErrorScope.PageSumarry
                    });
                    return response;
                }
                var confirmTwofactorResult = await _userManager.VerifyTwoFactorTokenAsync(user: userExist, tokenProvider: "Email", token: twoFactorDto.Code);
                if (confirmTwofactorResult)
                {
                    if (userExist.TwoFactorTokenProperty == null)
                    {
                        userExist.TwoFactorTokenProperty = new TwoFactorTokenProperty()
                        {
                            IsTokenInvoked = true,
                            TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(minutes: -1),
                            TwoFactorToken = twoFactorDto.Code
                        };
                    }
                    else
                    {
                        var twoFactorProperty = new TwoFactorTokenProperty()
                        {
                            IsTokenInvoked = true,
                            TokenExpiration = userExist.TwoFactorTokenProperty.TokenExpiration,
                            TwoFactorToken = twoFactorDto.Code
                        };
                        userExist.TwoFactorTokenProperty = twoFactorProperty;
                    }
                    await _userManager.UpdateAsync(user: userExist);

                    // need generate accesstoken and refresh token here
                    var jwtTokenModel = await _jwtService.GenerateJwtTokenAsync(userEntity: userExist, ipAddress: RuntimeContext.CurrentIpAddress ?? string.Empty);
                    response.StatusCode = StatusCodes.Status200OK;
                    response.Result.Data = new LoginResponseDto()
                    {
                        AccessToken = jwtTokenModel.AccessToken,
                        RefreshToken = jwtTokenModel.RefreshToken,
                        TwoFactorEnabled = false,
                        Message = "Login successfully"
                    };
                    response.Result.Success = true;
                    return response;
                }
                else
                {
                    string errorMessage = "Code is invalid.";
                    if (userExist.TwoFactorTokenProperty != null)
                    {
                        if (userExist.TwoFactorTokenProperty.TwoFactorToken != twoFactorDto.Code)
                        {
                            errorMessage = $"Code is invalid";
                        }
                        else if (userExist.TwoFactorTokenProperty.TokenExpiration < DateTimeOffset.UtcNow)
                        {
                            errorMessage = $"Token has expired";
                        }
                        else
                        {
                            errorMessage = $"Token has been used";
                        }
                    }
                    errors.Add(new ErrorDetail()
                    {
                        Error = errorMessage,
                        ErrorScope = CErrorScope.FormSummary
                    });
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Result.Success = false;
                    response.Result.Errors = errors;
                    return response;
                }

            }
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Result.Errors = errors;
            response.Result.Success = false;
            return response;
        }
        #endregion confirm two factor authentication
    }
}
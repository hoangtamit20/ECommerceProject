using CET.Domain;
using Core.Domain;
using Core.Domain.Entities.CET.Auth;
using Core.Domain.Enums.Roles;
using Core.Domain.Interfaces;
using Core.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
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
                            htmlTemplate: $"Your code here : {token}", emailProviderType: CEmailProviderType.Brevo);
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
                var jwtTokenModel = await _jwtService.GenerateJwtTokenAsync(userEntity: userExist,
                    ipAddress: RuntimeContext.CurrentIpAddress ?? string.Empty);
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

        #region refresh token
        public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenDto,
            ModelStateDictionary? modelState = null)
        {
            var response = new ApiResponse<LoginResponseDto>();
            var errors = ErrorHelper.GetModelStateError(modelState: modelState);
            if (!errors.IsNullOrEmpty())
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }
            var currentUser = RuntimeContext.CurrentUser;
            if (currentUser == null)
            {
                errors.Add(new ErrorDetail()
                {
                    Error = $"You don't have permission to request refresh token.",
                    ErrorScope = CErrorScope.Global
                });
                response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }
            var userRefreshTokenExist = await _cetRepository.GetSet<UserRefreshTokenEntity>(usr => 
                usr.UserId == currentUser.Id && usr.RefreshToken == refreshTokenDto.RefreshToken).FirstOrDefaultAsync();
            if (userRefreshTokenExist == null)
            {
                errors.Add(new ErrorDetail()
                {
                    Error = $"Refresh token '{refreshTokenDto.RefreshToken}' of user '{currentUser.Email}' not found.",
                    ErrorScope = CErrorScope.PageSumarry
                });
                response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }
            if (userRefreshTokenExist.IsRevoked || !userRefreshTokenExist.Active)
            {
                errors.Add(new ErrorDetail()
                {
                    Error = $"Refresh token '{refreshTokenDto.RefreshToken}' has expired or revoked",
                    ErrorScope = CErrorScope.PageSumarry
                });
                response.StatusCode = StatusCodes.Status419AuthenticationTimeout;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }
            // generate new accesstoken
            try
            {
                var accessToken = await _jwtService.GenerateJwtAccessTokenAsync(userEntity: currentUser);
                userRefreshTokenExist.AccessToken = accessToken;
                await _cetRepository.UpdateAsync<UserRefreshTokenEntity>(entity: userRefreshTokenExist);
                response.StatusCode = StatusCodes.Status200OK;
                response.Result.Success = true;
                response.Result.Data = new LoginResponseDto()
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshTokenDto.RefreshToken,
                    Message = $"Refresh new access token successfully.",
                    TwoFactorEnabled = false
                };
                return response;
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                errors.Add(new ErrorDetail()
                {
                    Error = $"An error occured while generate access token for user : '{currentUser.Email}'",
                    ErrorScope = CErrorScope.Global
                });
                response.Result.Success = false;
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Result.Errors = errors;
                return response;
            }
        }
        #endregion refresh token

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

        #region register
        public async Task<ApiResponse<RegisterResponsetDto>> RegisterAsync(RegisterRequestDto registerDto,
            ModelStateDictionary? modelState = null)
        {
            var errors = ErrorHelper.GetModelStateError(modelState: modelState);
            var response = new ApiResponse<RegisterResponsetDto>();
            if (!errors.IsNullOrEmpty())
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }
            var userExist = await _userManager.FindByEmailAsync(email: registerDto.Email);
            if (userExist != null)
            {
                errors.Add(new ErrorDetail()
                {
                    Error = $"User '{registerDto.Email}' already taken",
                    ErrorScope = CErrorScope.Field,
                    Field = $"{nameof(RegisterRequestDto.Email)}_Error"
                });
                response.StatusCode = StatusCodes.Status409Conflict;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }
            var roleExist = await _roleManager.FindByNameAsync(roleName: CRoleType.NormalUser.ToString());
            if (roleExist == null)
            {
                // create role :
                await _roleManager.CreateAsync(role: new RoleEntity()
                {
                    Name = CRoleType.NormalUser.ToString()
                });
            }
            // create user
            userExist = new UserEntity()
            {
                Email = registerDto.Email,
                UserName = registerDto.Email
            };
            using (var dbTransaction = await _cetRepository.BeginTransactionAsync())
            {
                var createUserResult = await _userManager.CreateAsync(user: userExist, password: registerDto.Password);
                if (!createUserResult.Succeeded)
                {
                    await dbTransaction.RollbackAsync();
                    errors.Add(new ErrorDetail()
                    {
                        Error = string.Join(Environment.NewLine, createUserResult.Errors.Select(err => err.Description).ToList()),
                        ErrorScope = CErrorScope.FormSummary,
                    });
                    response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    response.Result.Success = false;
                    response.Result.Errors = errors;
                    return response;
                }
                // assign role
                var addRoleResult = await _userManager.AddToRoleAsync(user: userExist, role: CRoleType.NormalUser.ToString());
                if (!addRoleResult.Succeeded)
                {
                    await dbTransaction.RollbackAsync();
                    errors.Add(new ErrorDetail()
                    {
                        Error = string.Join(Environment.NewLine, addRoleResult.Errors.Select(err => err.Description).ToList()),
                        ErrorScope = CErrorScope.FormSummary,
                    });
                    response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    response.Result.Success = false;
                    response.Result.Errors = errors;
                    return response;
                }
                // send email to user:
                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user: userExist);
                    var confirmationLink = LinkHelper.GenerateEmailConfirmationUrl(endpoint: RuntimeContext.Endpoint ?? string.Empty,
                        relatedUrl: EmailEndpoint.REGISTRAION_CONFIRM_ENDPOINT,
                        userId: userExist.Id, token: token).ToString();

                    var emailReplaceProperty = new ConfirmEmailTemplateModel
                    {
                        ConfirmationLink = confirmationLink,
                        CustomerName = registerDto.FullName,
                        ReceiverEmail = userExist.Email ?? string.Empty,
                        CompanyName = RuntimeContext.AppSettings.ClientApp.CompanyName,
                        Address = RuntimeContext.AppSettings.ClientApp.Address,
                        OwnerName = RuntimeContext.AppSettings.ClientApp.OwnerName,
                        OwnerPhone = RuntimeContext.AppSettings.ClientApp.OwnerPhone
                    };
                    await _emailService.SendEmailAsync(userExist.Email ?? string.Empty,
                        "Confirm your email to complete your registration",
                        string.Empty, "ConfirmEmailTemplate.html",
                        emailReplaceProperty,
                        CEmailProviderType.Brevo);

                    await dbTransaction.CommitAsync();
                    response.StatusCode = StatusCodes.Status202Accepted;
                    response.Result.Success = true;
                    response.Result.Data = new RegisterResponsetDto()
                    {
                        Email = userExist.Email ?? string.Empty,
                        Message = "Your account registration is complete! Please check your email to confirm your registration."
                    };
                    return response;
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                    errors.Add(new ErrorDetail()
                    {
                        Error = "An error occured while send email to confirmation registration.",
                        ErrorScope = CErrorScope.PageSumarry
                    });
                    await dbTransaction.RollbackAsync();
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Result.Errors = errors;
                    response.Result.Success = false;
                    return response;
                }
            }
        }
        #endregion register

        #region Reset password
        public async Task<ApiResponse<ResultMessage>> RequestResetPasswordAsync(
            ResetPasswordRequestDto resetPasswordDto, ModelStateDictionary? modelState = null)
        {
            var errors = ErrorHelper.GetModelStateError(modelState: modelState);
            var response = new ApiResponse<ResultMessage>();
            if (!errors.IsNullOrEmpty())
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }
            var userExist = await _userManager.FindByEmailAsync(email: resetPasswordDto.Email);
            if (userExist == null)
            {
                errors.Add(new ErrorDetail()
                {
                    Error = $"User '{resetPasswordDto.Email}' not exist.",
                    ErrorScope = CErrorScope.Field,
                    Field = $"{nameof(ResetPasswordRequestDto.Email)}_Error"
                });
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }
            try
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user: userExist);
                var confirmationLink = LinkHelper.GenerateEmailConfirmationUrl(endpoint: RuntimeContext.Endpoint ?? string.Empty,
                        relatedUrl: EmailEndpoint.RESET_PASSWORD_CONFIRM_ENDPOINT,
                        userId: userExist.Id, token: token).ToString();
                var appInfo = RuntimeContext.AppSettings.ClientApp;
                var emailReplaceProperty = new ResetPasswordEmailTemplateModel()
                {
                    Address = appInfo.Address,
                    CompanyName = appInfo.CompanyName,
                    CustomerName = userExist.FullName,
                    Email = appInfo.Email,
                    ReceiverEmail = userExist.Email ?? string.Empty,
                    OwnerName = appInfo.OwnerName,
                    OwnerPhone = appInfo.OwnerPhone
                };
                await _emailService.SendEmailAsync(email: userExist.Email ?? string.Empty,
                    subject: "RESET PASSWORD REQUEST",
                    htmlTemplate: string.Empty,
                    fileTemplateName: "ResetPasswordTemplate.html",
                    replaceProperty: emailReplaceProperty,
                    emailProviderType: CEmailProviderType.Brevo);
                var userTokenEntity = new UserTokenEntity()
                {
                    Value = token,
                    Type = CTokenType.PasswordReset,
                    TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(5),
                    IsTokenInvoked = false,
                    Name = CTokenType.PasswordReset.ToDescription(),
                    UserId = userExist.Id
                };
                await _cetRepository.UpdateAsync<UserTokenEntity>(entity: userTokenEntity);

                response.StatusCode = StatusCodes.Status202Accepted;
                response.Result.Success = true;
                response.Result.Data = new ResultMessage()
                {
                    Success = true,
                    Message = $"A password reset link has been sent to your email address. Please check your inbox and follow the instructions to reset your password."
                };
                return response;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                errors.Add(new ErrorDetail()
                {
                    Error = $"An error ocurred while generate token and send email confirm reset password.",
                    ErrorScope = CErrorScope.Global
                });
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }
        }

        public async Task<ApiResponse<ResultMessage>> ConfirmResetPasswordAsync(ConfirmResetPasswordRequestDto confirmDto, ModelStateDictionary? modelState = null)
        {
            var errors = ErrorHelper.GetModelStateError(modelState: modelState);
            var response = new ApiResponse<ResultMessage>();
            if (!errors.IsNullOrEmpty())
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }
            var userExist = await _userManager.FindByIdAsync(userId: confirmDto.UserId);
            if (userExist == null)
            {
                errors.Add(new ErrorDetail()
                {
                    Error = "User not found's",
                    ErrorScope = CErrorScope.PageSumarry,
                });
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Result.Errors = errors;
                response.Result.Success = false;
                return response;
            }
            var checkTokenResult = await _userManager.ResetPasswordAsync(user: userExist, token: confirmDto.Token, newPassword: confirmDto.NewPassword);
            var userToken = await _cetRepository.GetSet<UserTokenEntity>(ut => ut.Value == confirmDto.Token && ut.UserId == userExist.Id).FirstOrDefaultAsync();
            if (!checkTokenResult.Succeeded)
            {
                string errorMessage = string.Empty;
                if (userToken == null)
                {
                    errorMessage = "Token is invalid";
                }
                else if (userToken != null && userToken.IsTokenInvoked)
                {
                    errorMessage = "Token has been used.";
                }
                else
                {
                    errorMessage = "Token expired";
                }
                errors.Add(new ErrorDetail()
                {
                    Error = errorMessage,
                    ErrorScope = CErrorScope.PageSumarry
                });
                return response;
            }
            userToken!.IsTokenInvoked = true;
            await _cetRepository.UpdateAsync<UserTokenEntity>(entity: userToken);
            response.Result.Success = true;
            response.Result.Data = new ResultMessage()
            {
                Success = true,
                Message = "Reset password reset successfully"
            };
            response.StatusCode = StatusCodes.Status200OK;
            return response;
        }
        #endregion Reset password
    }
}
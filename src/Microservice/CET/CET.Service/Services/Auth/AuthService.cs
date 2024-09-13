using CET.Domain;
using Core.Domain;
using Core.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

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
                    var userToken = await _cetRepository.GetSet<UserTokenCustomEntity>(ut => ut.UserId == userExist.Id
                        && ut.Type == CTokenType.EmailConfirmation).OrderByDescending(ut => ut.TokenExpiration).FirstOrDefaultAsync();
                    if (userToken == null || (userToken != null && (userToken.TokenExpiration < DateTimeOffset.UtcNow || userToken.IsTokenInvoked)))
                    {
                        // send email
                        try
                        {
                            await SendEmailConfirmRegistrationAsync(userExist: userExist);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.Message);
                            errors.Add(new ErrorDetail()
                            {
                                Error = "An error occured while send email to confirmation account.",
                                ErrorScope = CErrorScope.PageSumarry
                            });
                            response.StatusCode = StatusCodes.Status500InternalServerError;
                            response.Result.Errors = errors;
                            response.Result.Success = false;
                            return response;
                        }
                    }
                    errors.Add(new ErrorDetail()
                    {
                        Error = $"The user '{loginDto.Email}' has not yet confirmed their email. Please check your email to confirm your account.",
                        ErrorScope = CErrorScope.FormSummary
                    });
                    response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Result.Errors = errors;
                    return response;
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
                        var token = await _userManager.GenerateTwoFactorTokenAsync(user: userExist, tokenProvider: CTokenProviderType.Email.ToString());
                        var clientInfo = RuntimeContext.AppSettings.ClientApp;
                        var emailReplaceProperty = new TwoFactorAuthenticationEmailTemplateModel()
                        {
                            Address = clientInfo.Address,
                            CompanyName = clientInfo.CompanyName,
                            OwnerName = clientInfo.OwnerName,
                            Email = clientInfo.Email,
                            ReceiverEmail = userExist.Email ?? string.Empty,
                            OwnerPhone = clientInfo.OwnerPhone,
                            Token = token,
                            CustomerName = userExist.FullName ?? string.Empty
                        };
                        await _emailService.SendEmailAsync(email: userExist.Email ?? string.Empty, subject: "2FA Authentication Code",
                            htmlTemplate: string.Empty,
                            fileTemplateName: "TwoFactorAuthenticationCode.html",
                            replaceProperty: emailReplaceProperty,
                            emailProviderType: CEmailProviderType.Gmail);

                        var userTokenEntity = new UserTokenCustomEntity()
                        {
                            Name = CTokenType.TwoFactor.ToDescription(),
                            Token = token,
                            IsTokenInvoked = false,
                            TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(5),
                            Type = CTokenType.TwoFactor,
                            UserId = userExist.Id,
                            TokenProviderName = CTokenProviderType.Email.ToString(),
                            TokenProviderType = CTokenProviderType.Email
                        };
                        await _cetRepository.AddAsync<UserTokenCustomEntity>(entity: userTokenEntity);
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
            var userRefreshTokenExist = await _cetRepository.GetSet<UserRefreshTokenEntity>(usr =>
                    usr.RefreshToken == refreshTokenDto.RefreshToken
                    && usr.AccessToken == refreshTokenDto.AccessToken)
                .FirstOrDefaultAsync();
            if (userRefreshTokenExist == null)
            {
                errors.Add(new ErrorDetail()
                {
                    Error = $"Refresh token is invalid or expired",
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
            var currentUser = await _userManager.FindByIdAsync(userId: userRefreshTokenExist.UserId);
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
            catch (Exception ex)
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
                    response.Result.Errors = errors;
                    return response;
                }
                var userTokenEntity = await _cetRepository.GetSet<UserTokenCustomEntity>(utc => utc.Token == twoFactorDto.Code
                    && utc.UserId == userExist.Id).FirstOrDefaultAsync();
                if (userTokenEntity == null)
                {
                    errors.Add(new ErrorDetail() { Error = "Token is invalid.", ErrorScope = CErrorScope.PageSumarry });
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Result.Success = false;
                    response.Result.Errors = errors;
                    return response;
                }
                else if (userTokenEntity.IsTokenInvoked || userTokenEntity.TokenExpiration < DateTimeOffset.UtcNow)
                {
                    string errorMessage = string.Empty;
                    if (userTokenEntity.IsTokenInvoked)
                    {
                        errorMessage = "Token has been revoked";
                    }
                    else
                    {
                        errorMessage = "Token has been expired";
                    }
                    errors.Add(new ErrorDetail() { Error = errorMessage, ErrorScope = CErrorScope.PageSumarry });
                    response.Result.Success = false;
                    response.Result.Errors = errors;
                    response.StatusCode = StatusCodes.Status406NotAcceptable;
                    return response;
                }
                var confirmTwofactorResult = await _userManager.VerifyTwoFactorTokenAsync(
                    user: userExist,
                    tokenProvider: userTokenEntity.TokenProviderName,
                    token: twoFactorDto.Code);
                if (confirmTwofactorResult)
                {
                    userTokenEntity.IsTokenInvoked = true;
                    await _cetRepository.UpdateAsync<UserTokenCustomEntity>(entity: userTokenEntity);
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
                    errors.Add(new ErrorDetail()
                    {
                        Error = "Token is invalid",
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
        public async Task<ApiResponse<ResultMessage>> RegisterAsync(RegisterRequestDto registerDto,
            ModelStateDictionary? modelState = null)
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
                UserName = registerDto.Email,
                FullName = registerDto.FullName
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
                    await SendEmailConfirmRegistrationAsync(userExist: userExist);
                    await dbTransaction.CommitAsync();
                    response.StatusCode = StatusCodes.Status202Accepted;
                    response.Result.Success = true;
                    response.Result.Data = new ResultMessage()
                    {
                        Level = CNotificationLevel.Info,
                        Message = $"Your account registration is complete! Please check your email '{userExist.Email}' to confirm your registration.",
                        NotificationType = CNotificationType.Email
                    };
                    return response;
                }
                catch (Exception ex)
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


        public async Task<ResultMessage> ConfirmRegisterAsync(ConfirmEmailDto confirmEmailDto, ModelStateDictionary? modelState)
        {
            var response = new ResultMessage();
            response.NotificationType = CNotificationType.Register;

            var errors = ErrorHelper.GetModelStateError(modelState: modelState);
            if (errors.Any())
            {
                response.Level = CNotificationLevel.Error;
                response.Message = errors.Select(e => e.Error).ToList().ToMultilineString();
                return response;
            }

            var userExist = await _userManager.FindByIdAsync(confirmEmailDto.UserId);
            if (userExist == null)
            {
                response.Level = CNotificationLevel.Error;
                response.Message = $"Cannot found anny user for this request.";
                return response;
            }

            var userTokenEntity = await _cetRepository.GetSet<UserTokenCustomEntity>(utc =>
                utc.UserId == userExist.Id && utc.Token == confirmEmailDto.Token).FirstOrDefaultAsync();

            if (userTokenEntity != null)
            {
                if (userTokenEntity.IsTokenInvoked)
                {
                    response.Level = CNotificationLevel.Error;
                    response.Message = $"The token has already been used. Please request a new one.";
                    return response;
                }
                if (userTokenEntity.TokenExpiration < DateTimeOffset.UtcNow)
                {
                    response.Level = CNotificationLevel.Error;
                    response.Message = $"The token has expired. Please request a new one.";
                    return response;
                }
            }

            var result = await _userManager.ConfirmEmailAsync(userExist, confirmEmailDto.Token);
            if (!result.Succeeded)
            {
                response.Message = result.Errors.Select(err => err.Description).ToList().ToMultilineString();
                response.Level = CNotificationLevel.Error;
                return response;
            }

            if (userTokenEntity == null)
            {
                userTokenEntity = new UserTokenCustomEntity
                {
                    IsTokenInvoked = true,
                    Name = CTokenType.EmailConfirmation.ToDescription(),
                    Token = confirmEmailDto.Token,
                    TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(-1),
                    Type = CTokenType.EmailConfirmation,
                    UserId = userExist.Id
                };
                await _cetRepository.AddAsync(userTokenEntity);
            }
            else
            {
                userTokenEntity.IsTokenInvoked = true;
                await _cetRepository.UpdateAsync(userTokenEntity);
            }

            response.Level = CNotificationLevel.Success;
            response.Message = "Your email has been successfully confirmed, and your account registration is complete. Congratulations!";
            return response;
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
            using (var dbTransaction = await _cetRepository.BeginTransactionAsync())
            {
                try
                {
                    // need check if already has request still not expire.
                    var tokenExpire = await _cetRepository.GetSet<UserTokenCustomEntity>(utc =>
                            utc.UserId == userExist.Id
                            && utc.Type == CTokenType.PasswordReset
                            && utc.TokenExpiration > DateTimeOffset.UtcNow)
                        .OrderByDescending(utc => utc.TokenExpiration)
                        .FirstOrDefaultAsync();
                    if (tokenExpire != null)
                    {
                        await dbTransaction.RollbackAsync();
                        errors.Add(new ErrorDetail()
                        {
                            Error = $"You have recently made a password reset request. Please check your email to confirm the previous request. You cannot make a new request until the current request expires.",
                            ErrorScope = CErrorScope.PageSumarry
                        });
                        response.StatusCode = StatusCodes.Status409Conflict;
                        response.Result.Success = false;
                        response.Result.Errors = errors;
                        return response;
                    }
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user: userExist);
                    var confirmationLink = LinkHelper.GenerateEmailConfirmationUrl(endpoint: RuntimeContext.AppSettings.ClientApp.ClientEndpoint ?? string.Empty,
                            relatedUrl: ClientEndpoint.Confirm_Reset_Password,
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
                        OwnerPhone = appInfo.OwnerPhone,
                        ConfirmationLink = confirmationLink
                    };
                    await _emailService.SendEmailAsync(email: userExist.Email ?? string.Empty,
                        subject: "RESET PASSWORD REQUEST",
                        htmlTemplate: string.Empty,
                        fileTemplateName: "ResetPasswordTemplate.html",
                        replaceProperty: emailReplaceProperty,
                        emailProviderType: CEmailProviderType.Gmail);
                    var userTokenEntity = new UserTokenCustomEntity()
                    {
                        Token = token,
                        Type = CTokenType.PasswordReset,
                        TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(5),
                        IsTokenInvoked = false,
                        Name = CTokenType.PasswordReset.ToDescription(),
                        UserId = userExist.Id,
                        TokenProviderName = CTokenProviderType.Email.ToString(),
                        TokenProviderType = CTokenProviderType.Email
                    };
                    await _cetRepository.AddAsync<UserTokenCustomEntity>(entity: userTokenEntity);

                    response.StatusCode = StatusCodes.Status202Accepted;
                    response.Result.Success = true;
                    response.Result.Data = new ResultMessage()
                    {
                        Level = CNotificationLevel.Success,
                        NotificationType = CNotificationType.Email,
                        Message = $"A password reset link has been sent to your email address. Please check your inbox and follow the instructions to reset your password."
                    };
                    await dbTransaction.CommitAsync();
                    return response;
                }
                catch (Exception ex)
                {
                    await dbTransaction.RollbackAsync();
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
        }

        public async Task<ApiResponse<ResultMessage>> ConfirmResetPasswordAsync(ConfirmResetPasswordRequestDto confirmDto,
            ModelStateDictionary? modelState = null)
        {
            var errors = ErrorHelper.GetModelStateError(modelState: modelState);
            var response = new ApiResponse<ResultMessage>();
            var parseTokenResult = LinkHelper.DecodeTokenFromUrl(tokenFromUrl: confirmDto.Token);
            if (parseTokenResult.IsNullOrEmpty())
            {
                errors.Add(new ErrorDetail()
                {
                    Error = "Token invalid",
                    ErrorScope = CErrorScope.PageSumarry
                });
                response.StatusCode = StatusCodes.Status406NotAcceptable;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }
            confirmDto.Token = parseTokenResult ?? string.Empty;
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
            var userToken = await _cetRepository.GetSet<UserTokenCustomEntity>(ut =>
                ut.Token == confirmDto.Token && ut.UserId == userExist.Id).FirstOrDefaultAsync();
            if (userToken == null)
            {
                errors.Add(new ErrorDetail()
                {
                    Error = "Token invalid",
                    ErrorScope = CErrorScope.PageSumarry
                });
                response.StatusCode = StatusCodes.Status406NotAcceptable;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }
            else if (userToken.IsTokenInvoked || userToken.TokenExpiration < DateTimeOffset.UtcNow)
            {
                string errorMessage = string.Empty;
                if (userToken.IsTokenInvoked)
                {
                    errorMessage = "Token has been revoked.";
                }
                else
                {
                    errorMessage = "Token has beeen expired";
                }
                errors.Add(new ErrorDetail()
                {
                    Error = errorMessage,
                    ErrorScope = CErrorScope.PageSumarry
                });
                response.StatusCode = StatusCodes.Status406NotAcceptable;
                response.Result.Success = false;
                response.Result.Errors = errors;
                return response;
            }

            var checkTokenResult = await _userManager.ResetPasswordAsync(user: userExist,
                token: confirmDto.Token, newPassword: confirmDto.NewPassword);
            if (!checkTokenResult.Succeeded)
            {
                string errorMessage = string.Empty;
                if (userToken.IsTokenInvoked)
                {
                    errorMessage = "Token has been used.";
                }
                else if (userToken.TokenExpiration < DateTimeOffset.UtcNow)
                {
                    errorMessage = "Token expired";
                }
                else
                {
                    errorMessage = String.Join(Environment.NewLine, checkTokenResult.Errors.Select(err => err.Description));
                }
                errors.Add(new ErrorDetail()
                {
                    Error = errorMessage,
                    ErrorScope = CErrorScope.PageSumarry
                });
                return response;
            }
            userToken.IsTokenInvoked = true;
            await _cetRepository.UpdateAsync<UserTokenCustomEntity>(entity: userToken);
            response.Result.Success = true;
            response.Result.Data = new ResultMessage()
            {
                Level = CNotificationLevel.Success,
                NotificationType = CNotificationType.Normal,
                Message = "Reset password reset successfully"
            };
            response.StatusCode = StatusCodes.Status200OK;
            return response;
        }
        #endregion Reset password

        #region Logout
        public async Task<ApiResponse<ResultMessage>> LogOutAsync(bool areAllDevices = false)
        {
            var currentUserId = RuntimeContext.CurrentUserId;
            var currentAccessToken = RuntimeContext.CurrentAccessToken;
            var errors = new List<ErrorDetail>();
            var response = new ApiResponse<ResultMessage>();
            if (string.IsNullOrEmpty(currentUserId) || currentAccessToken == null)
            {
                errors.Add(new ErrorDetail() { Error = "Authentication failed!", ErrorScope = CErrorScope.PageSumarry });
                response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Result.Errors = errors;
                response.Result.Success = false;
                return response;
            }
            var userRefreshTokenEntity = await _cetRepository.GetSet<UserRefreshTokenEntity>(urt => urt.UserId == currentUserId
                && urt.AccessToken == currentAccessToken).FirstOrDefaultAsync();
            if (userRefreshTokenEntity == null)
            {
                response.StatusCode = StatusCodes.Status200OK;
                response.Result.Success = true;
                response.Result.Data = new ResultMessage()
                {
                    Message = "Logout successfully",
                    Level = CNotificationLevel.Info,
                    NotificationType = CNotificationType.Normal
                };
                return response;
            }
            var revokedUserRefreshTokens = areAllDevices ? await _cetRepository.GetSet<UserRefreshTokenEntity>(urt => urt.UserId == currentUserId
                && !urt.IsRevoked && urt.Active).ToListAsync() : new List<UserRefreshTokenEntity>() { userRefreshTokenEntity };
            var now = DateTimeOffset.UtcNow;
            revokedUserRefreshTokens.ForEach(item =>
            {
                item.IsRevoked = true;
                item.ExpireTime = now;
                item.LastRevoked = now;
            });
            using (var dbTransaction = await _cetRepository.BeginTransactionAsync())
            {
                try
                {
                    await _cetRepository.UpdateRangeAsync(entities: revokedUserRefreshTokens);
                    await dbTransaction.CommitAsync();
                    response.Result.Success = true;
                    response.Result.Data = new ResultMessage()
                    {
                        Message = "Logout successfully",
                        Level = CNotificationLevel.Info,
                        NotificationType = CNotificationType.Normal
                    };
                    response.StatusCode = StatusCodes.Status200OK;
                    return response;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    await dbTransaction.RollbackAsync();
                    errors.Add(new ErrorDetail() { Error = "An error occured while revoked token.", ErrorScope = CErrorScope.PageSumarry });
                    response.Result.Errors = errors;
                    response.Result.Success = false;
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    return response;
                }
            }
        }
        #endregion Logout

        #region Send Email
        private async Task SendEmailConfirmRegistrationAsync(UserEntity userExist)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user: userExist);
            var confirmationLink = LinkHelper.GenerateEmailConfirmationUrl(endpoint: RuntimeContext.Endpoint ?? string.Empty,
                relatedUrl: EmailEndpoint.REGISTRAION_CONFIRM_ENDPOINT,
                userId: userExist.Id, token: token).ToString();
            var clientInfo = RuntimeContext.AppSettings.ClientApp;
            var emailReplaceProperty = new ConfirmEmailTemplateModel
            {
                ConfirmationLink = confirmationLink,
                CustomerName = userExist.FullName,
                ReceiverEmail = userExist.Email ?? string.Empty,
                CompanyName = clientInfo.CompanyName,
                Address = clientInfo.Address,
                OwnerName = clientInfo.OwnerName,
                OwnerPhone = clientInfo.OwnerPhone
            };
            await _emailService.SendEmailAsync(userExist.Email ?? string.Empty,
                "Confirm your email to complete your registration",
                string.Empty, "ConfirmEmailTemplate.html",
                emailReplaceProperty,
                CEmailProviderType.Gmail);

            // save userToken
            var userTokenEntity = new UserTokenCustomEntity()
            {
                IsTokenInvoked = false,
                Name = CTokenType.EmailConfirmation.ToDescription(),
                Type = CTokenType.EmailConfirmation,
                TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(5),
                UserId = userExist.Id,
                Token = token,
                TokenProviderName = CTokenProviderType.Email.ToString(),
                TokenProviderType = CTokenProviderType.Email
            };
            await _cetRepository.AddAsync<UserTokenCustomEntity>(entity: userTokenEntity);
        }
        #endregion Send Email
    }
}
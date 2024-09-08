using CET.Domain;
using Core.Domain;
using Core.Service;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CET.Service
{
    public class UserService : IUserService
    {
        private readonly ICETRepository _cetRepository;
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<RoleEntity> _roleManager;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailService _emailService;


        public UserService(
            ICETRepository cetRepository,
            UserManager<UserEntity> userManager,
            IEmailService emailService,
            ILogger<UserService> logger,
            IHostEnvironment env,
            RoleManager<RoleEntity> roleManager)
        {
            _cetRepository = cetRepository;
            _userManager = userManager;
            _logger = logger;
            _emailService = emailService;
            _roleManager = roleManager;
        }


        public async Task<UserDetailDto?> GetUserDetailByIdAsync(string userId)
        {
            UserEntity? userExist = await _userManager.FindByIdAsync(userId: userId);
            if (userExist == null)
            {
                return null;
            }
            return userExist.Adapt<UserDetailDto>();
        }

        public async Task<ApiResponse<CreateUserResponseDto>> CreateUserAsync(CreateUserRequestDto requestDto,
            bool isAdminRequest = false,
            ModelStateDictionary? modelState = null)
        {
            var errorList = new List<ErrorDetail>();
            errorList = ErrorHelper.GetModelStateError(modelState: modelState);
            if (errorList.IsNullOrEmpty())
            {
                var response = new ApiResponse<CreateUserResponseDto>();
                _logger.LogInformation($"Start create user : Email = {requestDto.Email}");

                await CheckUserExistenceAsync(requestDto, errorList);

                var userEntity = new UserEntity
                {
                    UserName = !string.IsNullOrEmpty(requestDto.UserName) ? requestDto.UserName : requestDto.Email,
                    Email = requestDto.Email,
                    FullName = requestDto.FullName,
                    DateOfBirth = requestDto.DateOfBirth,
                    PhoneNumber = requestDto.PhoneNumber
                };

                using (var dbTransaction = await _cetRepository.BeginTransactionAsync())
                {
                    try
                    {
                        var result = await _userManager.CreateAsync(userEntity, requestDto.Password);

                        if (result.Succeeded)
                        {
                            await AssignRolesAsync(userEntity, requestDto, isAdminRequest, errorList);
                            if (!errorList.IsNullOrEmpty())
                            {
                                response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                                response.Result.Success = false;
                                response.Result.Errors = errorList;
                                return response;
                            }
                            if (!isAdminRequest)
                            {
                                await SendEmailConfirmationAsync(userEntity, requestDto, errorList);
                                if (!errorList.IsNullOrEmpty())
                                {
                                    await dbTransaction.RollbackAsync();
                                    response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                                    response.Result.Errors = errorList;
                                    response.Result.Success = false;
                                    return response;
                                }
                                await dbTransaction.CommitAsync();
                                response.StatusCode = StatusCodes.Status202Accepted;
                                response.Result.Data = new CreateUserResponseDto()
                                {
                                    Email = userEntity.Email,
                                    NeedEmailConfirm = true,
                                    Message = "Your account registration is complete! Please check your email to confirm your registration."
                                };
                                response.Result.Success = true;
                                return response;
                            }
                            await dbTransaction.CommitAsync();
                        }
                        else
                        {
                            await dbTransaction.RollbackAsync();
                            errorList.Add(new ErrorDetail()
                            {
                                Error = string.Join(Environment.NewLine, result.Errors.Select(err => err.Description).ToList()),
                                ErrorScope = CErrorScope.FormSummary,
                            });
                            response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                            response.Result.Success = false;
                            response.Result.Errors = errorList;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"{ex.Message}");
                        await dbTransaction.RollbackAsync();
                        errorList.Add(new ErrorDetail()
                        {
                            Error = $"An error occured while create user",
                            ErrorScope = CErrorScope.PageSumarry,
                        });
                        response.StatusCode = StatusCodes.Status500InternalServerError;
                        response.Result.Success = false;
                        response.Result.Errors = errorList;
                    }
                }
                response.StatusCode = StatusCodes.Status201Created;
                response.Result.Data = new CreateUserResponseDto()
                {
                    Email = userEntity.Email,
                    NeedEmailConfirm = false,
                    Message = $"Account has been create successfully"
                };
                response.Result.Success = true;
                return response;
            }
            return new ApiResponse<CreateUserResponseDto>()
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Result = new ResponseResult<CreateUserResponseDto>()
                {
                    Errors = errorList,
                    Success = false
                }
            };
        }


        public async Task<ApiResponse<string>> ConfirmedEmailAsync(ConfirmEmailDto confirmEmailDto, ModelStateDictionary? modelState = null)
        {
            var response = new ApiResponse<string>();

            var errors = ErrorHelper.GetModelStateError(modelState: modelState);
            if (errors.Any())
            {
                return GenerateErrorResponse(response, StatusCodes.Status400BadRequest, errors);
            }

            var userExist = await _userManager.FindByIdAsync(confirmEmailDto.UserId);
            if (userExist == null)
            {
                var userNotFoundError = new ErrorDetail { Field = "UserId", Error = "User doesn't exist", ErrorScope = CErrorScope.PageSumarry };
                return GenerateErrorResponse(response, StatusCodes.Status404NotFound, new List<ErrorDetail> { userNotFoundError });
            }

            var userTokenEntity = await _cetRepository.GetSet<UserTokenCustomEntity>(utc =>
                utc.UserId == userExist.Id && utc.Token == confirmEmailDto.Token).FirstOrDefaultAsync();

            if (userTokenEntity != null)
            {
                if (userTokenEntity.IsTokenInvoked)
                {
                    return GenerateErrorResponse(response, StatusCodes.Status422UnprocessableEntity,
                        new List<ErrorDetail> { new ErrorDetail { Error = "Token has been invoked", ErrorScope = CErrorScope.PageSumarry } });
                }
                if (userTokenEntity.TokenExpiration < DateTimeOffset.UtcNow)
                {
                    return GenerateErrorResponse(response, StatusCodes.Status422UnprocessableEntity,
                        new List<ErrorDetail> { new ErrorDetail { Error = "Token has expired", ErrorScope = CErrorScope.PageSumarry } });
                }
            }

            var result = await _userManager.ConfirmEmailAsync(userExist, confirmEmailDto.Token);
            if (!result.Succeeded)
            {
                var errorsList = result.Errors.Select(err => new ErrorDetail
                {
                    ErrorScope = CErrorScope.PageSumarry,
                    Error = err.Description
                }).ToList();
                return GenerateErrorResponse(response, StatusCodes.Status422UnprocessableEntity, errorsList);
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

            response.Result.Success = true;
            response.Result.Data = "Your email has been confirmed successfully.";
            return response;
        }

        private ApiResponse<string> GenerateErrorResponse(ApiResponse<string> response, int statusCode, List<ErrorDetail> errors)
        {
            response.Result.Success = false;
            response.StatusCode = statusCode;
            response.Result.Errors = errors;
            return response;
        }

        private async Task CheckUserExistenceAsync(CreateUserRequestDto requestDto, List<ErrorDetail> errors)
        {

            if (await _userManager.FindByEmailAsync(requestDto.Email) != null)
            {
                errors.Add(new ErrorDetail()
                {
                    Error = $"Email = '{requestDto.Email}' already taken.",
                    ErrorScope = CErrorScope.Field,
                    Field = $"{nameof(CreateUserRequestDto.Email)}_Error"
                });
            }

            if (await _userManager.FindByNameAsync(requestDto.UserName) != null)
            {
                errors.Add(new ErrorDetail()
                {
                    Error = $"UserName = '{requestDto.UserName}' already taken.",
                    ErrorScope = CErrorScope.Field,
                    Field = $"{nameof(CreateUserRequestDto.UserName)}_Error"
                });
            }

            if (await _cetRepository.GetSet<UserEntity>(us => us.PhoneNumber == requestDto.PhoneNumber).FirstOrDefaultAsync() != null)
            {
                errors.Add(new ErrorDetail()
                {
                    Error = $"Phone number = '{requestDto.PhoneNumber}' already taken.",
                    ErrorScope = CErrorScope.Field,
                    Field = $"{nameof(CreateUserRequestDto.PhoneNumber)}_Error"
                });
            }
        }

        private async Task AssignRolesAsync(UserEntity userEntity, CreateUserRequestDto requestDto,
            bool isAdminRequest, List<ErrorDetail> errors)
        {
            var roles = isAdminRequest ? requestDto.Roles?.Select(r => r.ToString())
                : new List<string> { CRoleType.NormalUser.ToString() };
            try
            {
                foreach (var role in roles ?? new List<string>())
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        var createRoleResult = await _roleManager.CreateAsync(new RoleEntity { Name = role });
                        if (!createRoleResult.Succeeded)
                        {
                            errors.AddRange(createRoleResult.Errors.Select(err => new ErrorDetail()
                            {
                                Error = err.Description,
                                ErrorScope = CErrorScope.PageSumarry
                            }).ToList());
                            return;
                        }
                    }

                    if (!await _userManager.IsInRoleAsync(userEntity, role))
                    {
                        await _userManager.AddToRoleAsync(userEntity, role);
                    }
                }

                if (isAdminRequest)
                {
                    userEntity.EmailConfirmed = true;
                    await _userManager.UpdateAsync(userEntity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                errors.Add(new ErrorDetail()
                {
                    Error = $"An error occured while assign role for user",
                    ErrorScope = CErrorScope.PageSumarry
                });
                return;
            }
        }

        private async Task SendEmailConfirmationAsync(UserEntity userEntity, CreateUserRequestDto requestDto,
            List<ErrorDetail> errors)
        {
            var endpoint = RuntimeContext.Endpoint;
            if (!string.IsNullOrEmpty(endpoint))
            {
                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(userEntity);
                    var confirmationLink = LinkHelper.GenerateEmailConfirmationUrl(endpoint: RuntimeContext.Endpoint ?? string.Empty,
                        relatedUrl: EmailEndpoint.REGISTRAION_CONFIRM_ENDPOINT,
                        userId: userEntity.Id, token: token).ToString();
                    var clientInfo = RuntimeContext.AppSettings.ClientApp;
                    var emailReplaceProperty = new ConfirmEmailTemplateModel
                    {
                        ConfirmationLink = confirmationLink,
                        CustomerName = requestDto.FullName,
                        ReceiverEmail = requestDto.Email ?? string.Empty,
                        CompanyName = clientInfo.CompanyName,
                        Address = clientInfo.Address,
                        OwnerPhone = clientInfo.OwnerPhone
                    };

                    await _emailService.SendEmailAsync(userEntity.Email ?? string.Empty,
                        "Confirm your email to complete your registration",
                        string.Empty, "ConfirmEmailTemplate.html",
                        emailReplaceProperty,
                        CEmailProviderType.Gmail);
                    var userTokenEntity = new UserTokenCustomEntity()
                    {
                        Type = CTokenType.EmailConfirmation,
                        Name = CTokenType.EmailConfirmation.ToDescription(),
                        Token = token,
                        TokenExpiration = DateTimeOffset.UtcNow.AddMinutes(5),
                        IsTokenInvoked = false,
                        UserId = userEntity.Id,
                        TokenProviderName = CTokenProviderType.Email.ToString(),
                        TokenProviderType = CTokenProviderType.Email
                    };
                    await _cetRepository.AddAsync<UserTokenCustomEntity>(entity: userTokenEntity);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    errors.Add(new ErrorDetail()
                    {
                        Error = $"{ex.Message}",
                        ErrorScope = CErrorScope.PageSumarry
                    });
                }
            }
            errors.Add(new ErrorDetail()
            {
                Error = $"Cannot found the url to set for registration email confirm.",
                ErrorScope = CErrorScope.PageSumarry,
            });
        }
    }
}
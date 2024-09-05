using CET.Domain;
using Core.Domain;
using Core.Domain.Enums.Roles;
using Core.Domain.Extensions.JsonSerialized;
using Core.Domain.Interfaces;
using Core.Service.Models;
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
        private readonly IHostEnvironment _env;
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
            _env = env;
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
                                    Message = "Your account registration is complete! Please check your email to confirm your registration"
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


        public async Task<ApiResponse<string>> ConfirmedEmailAsync(string userId, string token)
        {
            var response = new ApiResponse<string>();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                response.Result.Success = false;
                response.Result.Errors.Add(new ErrorDetail { Field = "UserId", Error = "User ID is required", ErrorScope = CErrorScope.PageSumarry });
                response.Result.Errors.Add(new ErrorDetail { Field = "Token", Error = "Token is required", ErrorScope = CErrorScope.PageSumarry });
                response.StatusCode = StatusCodes.Status400BadRequest;
                return response;
            }

            var userExist = await _userManager.FindByIdAsync(userId);
            if (userExist == null)
            {
                response.Result.Success = false;
                response.Result.Errors.Add(new ErrorDetail { Field = "UserId", Error = "User doesn't exist", ErrorScope = CErrorScope.PageSumarry });
                response.StatusCode = StatusCodes.Status404NotFound;
                return response;
            }

            var result = await _userManager.ConfirmEmailAsync(userExist, token);
            if (!result.Succeeded)
            {
                response.Result.Success = false;
                response.Result.Errors = result.Errors.Select(err => new ErrorDetail
                {
                    ErrorScope = CErrorScope.PageSumarry,
                    Field = nameof(UserEntity.Email),
                    Error = err.Description
                }).ToList();
                response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return response;
            }

            response.Result.Success = true;
            response.Result.Data = $"Your email has been confirmed successfully.";
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
            var links = _cetRepository.GetSet<LinkHelperEntity>().Select(item => _env.IsDevelopment() ? item.DevelopmentEndpoint : item.ProductionEndpoint).ToList();
            if (!links.IsNullOrEmpty())
            {
                try
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(userEntity);
                    var confirmationLink = LinkHelper.GenerateEmailConfirmationUrl(endpoint: links.First(),
                        relatedUrl: EmailEndpoint.REGISTRAION_CONFIRM_ENDPOINT,
                        userId: userEntity.Id, token: token).ToString();

                    var emailReplaceProperty = new ConfirmEmailTemplateModel
                    {
                        ConfirmationLink = confirmationLink,
                        CustomerName = requestDto.FullName,
                        ReceiverEmail = userEntity.Email ?? string.Empty,
                        YourCompany = "Default company name"
                    };

                    await _emailService.SendEmailAsync(userEntity.Email ?? string.Empty,
                        "Confirm your email to complete your registration",
                        string.Empty, "ConfirmEmailTemplate.html",
                        emailReplaceProperty,
                        CEmailProviderType.Gmail);
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
using Core.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CET.Domain
{
    public interface IUserService
    {
        Task<UserDetailDto?> GetUserDetailByIdAsync(string userId);
        Task<ApiResponse<CreateUserResponseDto>> CreateUserAsync(CreateUserRequestDto requestDto,
            bool isAdminRequest = false,
            ModelStateDictionary? modelState = null);
        Task<ApiResponse<string>> ConfirmedEmailAsync(ConfirmEmailDto confirmEmailDto, ModelStateDictionary? modelState = null);
    }
}
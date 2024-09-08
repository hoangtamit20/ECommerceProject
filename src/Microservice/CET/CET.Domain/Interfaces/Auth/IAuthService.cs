using Core.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CET.Domain
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> SystemLoginAsync(LoginRequestDto loginDto, ModelStateDictionary? modelState = null);
        Task<ApiResponse<LoginResponseDto>> ConfirmTwoFactorAuthenticationAsync(
            ConfirmTwoFactorAuthenticationRequestDto twoFactorDto, ModelStateDictionary? modelState = null);
        Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenDto,
            ModelStateDictionary? modelState = null);
        Task<ApiResponse<RegisterResponsetDto>> RegisterAsync(RegisterRequestDto registerDto,
            ModelStateDictionary? modelState = null);
        Task<ApiResponse<ResultMessage>> RequestResetPasswordAsync(
            ResetPasswordRequestDto resetPasswordDto, ModelStateDictionary? modelState = null);
        Task<ApiResponse<ResultMessage>> ConfirmResetPasswordAsync(ConfirmResetPasswordRequestDto confirmDto,
            ModelStateDictionary? modelState = null);
        Task<ApiResponse<ResultMessage>> LogOutAsync(bool areAllDevices = false);
    }
}
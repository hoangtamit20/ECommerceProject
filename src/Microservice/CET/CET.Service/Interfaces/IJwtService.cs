using CET.Domain;
using Core.Domain;

namespace CET.Service
{
    public interface IJwtService
    {
        Task<JwtTokenDto> GenerateJwtTokenAsync(UserEntity userEntity, string ipAddress);
        Task<string> GenerateJwtAccessTokenAsync(UserEntity userEntity);
        Task<string> GenerateJwtRefreshTokenAsync(UserEntity userEntity, string ipAddress, string accessToken);
    }
}
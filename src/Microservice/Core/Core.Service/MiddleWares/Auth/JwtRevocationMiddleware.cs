using System.Security.Claims;
using Core.Domain;
using Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Core.Service
{
    public class JwtRevocationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICETRepository _cetRepository;

        // Constructor that takes the next middleware in the pipeline.
        public JwtRevocationMiddleware(RequestDelegate next, ICETRepository cetRepository)
        {
            _cetRepository = cetRepository;
            _next = next;
        }

        // This method is invoked for each HTTP request.
        public async Task InvokeAsync(
            HttpContext context,
            UserManager<UserEntity> userManager,
            ICETRepository cetRepository)
        {
            // Authenticate the user.
            var authResult = await context.AuthenticateAsync();

            // If authentication is successful, proceed.
            if (authResult.Succeeded)
            {
                // Retrieve the access token from the "Authorization" header.
                var accessToken = GetAccessToken(context);
                if (accessToken == null)
                {
                    // If the access token is invalid or missing, return an error.
                    await WriteErrorResponseAsync(context, StatusCodes.Status401Unauthorized, "Invalid or missing authorization token.");
                    return;
                }

                // Retrieve the user ID claim from the access token.
                var userIdClaim = GetUserIdClaim(accessToken);
                if (userIdClaim == null)
                {
                    // If the user ID claim is missing, return an error.
                    await WriteErrorResponseAsync(context, StatusCodes.Status500InternalServerError, "Internal server error");
                    return;
                }

                // Retrieve the user's refresh token from the database.
                var userRefreshToken = await GetUserRefreshTokenAsync(_cetRepository, accessToken, userIdClaim.Value);
                if (userRefreshToken == null)
                {
                    // If the refresh token is missing, return an error.
                    await WriteErrorResponseAsync(context, StatusCodes.Status401Unauthorized, "Token does not exist");
                    return;
                }

                // Retrieve the user from the database.
                var user = await userManager.FindByIdAsync(userIdClaim.Value);

                // Check if the token has been revoked.
                if (user != null && userRefreshToken.LastRevoked > GetTokenValidFrom(accessToken))
                {
                    // If the token has been revoked, return an error.
                    await WriteErrorResponseAsync(context, StatusCodes.Status401Unauthorized, "Token is revoked");
                    return;
                }
            }

            // Pass control to the next middleware in the pipeline.
            await _next(context);
        }

        // This method retrieves the access token from the "Authorization" header.
        // This method retrieves the access token from the "Authorization" header or query string.
        private string? GetAccessToken(HttpContext context)
        {
            // Attempt to retrieve token from "Authorization" header
            if (context.Request.Headers.TryGetValue("Authorization", out var accessTokenValue)
                && accessTokenValue.ToString().StartsWith("Bearer "))
            {
                return accessTokenValue.ToString().Substring("Bearer ".Length).Trim();
            }

            // Attempt to retrieve token from query string
            var accessTokenFromQuery = context.Request.Query["access_token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(accessTokenFromQuery))
            {
                return accessTokenFromQuery;
            }

            // If neither "Authorization" header nor query string contains token, return null
            return null;
        }


        // This method retrieves the user ID claim from the access token.
        private Claim? GetUserIdClaim(string accessToken)
        {
            var handler = new JsonWebTokenHandler();
            var jsonToken = handler.ReadJsonWebToken(accessToken);
            return jsonToken.Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        }

        // This method retrieves the user's refresh token from the database.
        private async Task<UserRefreshTokenEntity?> GetUserRefreshTokenAsync(
            ICETRepository cetRepository,
            string accessToken, string userId)
        {
            return await cetRepository.GetSet<UserRefreshTokenEntity>()
                .FirstOrDefaultAsync(usr =>
                    usr.AccessToken == accessToken
                    && usr.UserId == userId);
        }

        // This method retrieves the valid from date of the token.
        private DateTime GetTokenValidFrom(string accessToken)
        {
            var handler = new JsonWebTokenHandler();
            var jsonToken = handler.ReadJsonWebToken(accessToken);
            return jsonToken.ValidFrom;
        }

        // This method writes an error response.
        private async Task WriteErrorResponseAsync(
            HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(message);
        }
    }
}
using Blazored.LocalStorage;
using Core.Domain;
using Mapster;

namespace Blazor.WebApp
{
    public class LocalStorageService
    {
        private readonly ILocalStorageService _localStorage;
        private const string StorageKey = "Authentication-Blazor-App-Token";
        public LocalStorageService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<string> GetTokenAsync()
        {
            var token = await _localStorage.GetItemAsStringAsync(StorageKey);
            return token ?? string.Empty;
        }
        public async Task SetTokenAsync(string item)
        {
            await _localStorage.SetItemAsStringAsync(key: StorageKey, data: item);
        }

        public async Task RemoveTokenAsync()
        {
            await _localStorage.RemoveItemAsync(key: StorageKey);
        }

        public async Task<bool> IsRefreshTokenAsync(ApiClient apiClient)
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            var userSession = token.FromJson<UserSession>();
            if (userSession == null || (userSession != null 
                && (string.IsNullOrEmpty(userSession.AccessToken) 
                    || string.IsNullOrEmpty(userSession.RefreshToken))))
            {
                await RemoveTokenAsync();
                return false;
            }
            // call api to request refresh token
            var response = await apiClient.PostAsync<RefreshTokenRequestDto, LoginResponseDto>(
                uri: APIEndpoint.CET_Auth_RefreshToken,
                data: userSession.Adapt<RefreshTokenRequestDto>(),
                requestType: CRequestType.Public);
            if (response != null)
            {
                if (response.Success && response.Data != null)
                {
                    await SetTokenAsync(item: response.Data.Adapt<UserSession>().ToJson());
                    return true;
                };
                await RemoveTokenAsync();
                return false;
            }
            return false;
        }
    }
}
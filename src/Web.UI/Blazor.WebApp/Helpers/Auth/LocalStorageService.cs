using Blazored.LocalStorage;

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
    }
}
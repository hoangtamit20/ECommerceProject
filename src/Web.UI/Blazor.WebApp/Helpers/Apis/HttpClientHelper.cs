using System.Net.Http.Headers;
using Core.Domain;

namespace Blazor.WebApp
{
    public class HttpClientHelper
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly LocalStorageService _localStorage;
        private const string HeaderKey = "Authorization";
        public HttpClientHelper(IHttpClientFactory httpClient, LocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<HttpClient> GetPrivateHttpClientAsync()
        {
            var client = _httpClient.CreateClient("SystemApiClient");
            client.BaseAddress = new Uri("https://localhost:7035");
            var token = await _localStorage.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return client;
            }
            var userSession = token.FromJson<UserSession>();
            if (userSession == null)
            {
                return client;
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer", parameter: userSession.AccessToken);
            return client;
        }

        public async Task<HttpClient> GetPublicHttpClientAsync()
        {
            var client = _httpClient.CreateClient("SystemApiClient");
            client.BaseAddress = new Uri("https://localhost:7035");
            client.DefaultRequestHeaders.Remove(name: HeaderKey);
            return await Task.FromResult(client);
        }
    }
}
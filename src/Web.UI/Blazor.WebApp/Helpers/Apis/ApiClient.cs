using System.Text;
using Core.Domain;

namespace Blazor.WebApp
{
    public class ApiClient
    {
        private readonly HttpClientHelper _httpClientHelper;

        public ApiClient(HttpClientHelper httpClientHelper)
        {
            _httpClientHelper = httpClientHelper;
        }


        public async Task<ResponseResult<TResponse>?> PostAsync<TRequest, TResponse>(string uri, TRequest data,
            CRequestType requestType = CRequestType.Private)
        {
            var httpClient = requestType == CRequestType.Private ? await _httpClientHelper.GetPrivateHttpClientAsync() 
                : await _httpClientHelper.GetPublicHttpClientAsync();
            var jsonContent = data?.ToJson() ?? string.Empty;
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(uri, content);
            var responseData = await response.Content.ReadAsStringAsync();

            // Deserialize into ApiResponse<TResponse>
            var result = responseData.FromJson<ResponseResult<TResponse>>();
            return result;
        }



        public async Task<ResponseResult<TResponse>?> GetAsync<TResponse>(string uri, CRequestType requestType)
        {
            var httpClient = requestType == CRequestType.Private ? await _httpClientHelper.GetPrivateHttpClientAsync() 
                : await _httpClientHelper.GetPublicHttpClientAsync();
            var response = await httpClient.GetAsync(uri);
            var responseData = await response.Content.ReadAsStringAsync();

            return responseData.FromJson<ResponseResult<TResponse>>();
        }

        public async Task<bool> IsAuthentication()
        {
            var response = await GetAsync<string>(uri: APIEndpoint.CET_Auth_Authentication,
                requestType: CRequestType.Private);
            if (response != null)
            {
                if (response.Data == null)
                    return false;
                return true;
            }
            return false;
        }
    }
}
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
            var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(uri, content);
            var responseData = await response.Content.ReadAsStringAsync();

            // Deserialize into ApiResponse<TResponse>
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseResult<TResponse>>(responseData);
            return result;
        }



        // public async Task<ApiResponse<TResponse>?> GetAsync<TResponse>(string uri)
        // {
        //     var response = await _httpClient.GetAsync(uri);
        //     var responseData = await response.Content.ReadAsStringAsync();

        //     if (response.IsSuccessStatusCode)
        //     {
        //         return JsonSerializer.Deserialize<ApiResponse<TResponse>>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        //     }

        //     return new ApiResponse<TResponse>
        //     {
        //         StatusCode = (int)response.StatusCode,
        //         Result = new ResponseResult<TResponse>
        //         {
        //             Success = false,
        //             Errors = JsonSerializer.Deserialize<List<ErrorDetail>>(responseData) ?? new()
        //         }
        //     };
        // }
    }
}
using System.Reflection;
using System.Text;
using System.Web;

namespace Core.Domain
{
    public class LinkHelper
    {
        public static Uri GenerateEmailConfirmationUrl(string endpoint, string relatedUrl, string userId, string token)
        {
            // Combine the endpoint and relatedUrl
            Uri baseUri = new Uri(endpoint);
            Uri fullUri = new Uri(baseUri, relatedUrl);

            // Create UriBuilder with the combined URL
            var uriBuilder = new UriBuilder(fullUri);
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["userId"] = userId;
            parameters["token"] = token;
            uriBuilder.Query = parameters.ToString();
            return uriBuilder.Uri;
        }

        public static string? DecodeTokenFromUrl(string tokenFromUrl)
        {
            try
            {
                // Check if the token is not null or empty
                if (string.IsNullOrEmpty(tokenFromUrl))
                {
                    return null;
                }

                // Check if the token contains any special characters
                if (tokenFromUrl.IndexOfAny(new char[] { '+', '%', '&' }) >= 0)
                {
                    // Decode the token using HttpUtility.UrlDecode
                    // string decodedToken = HttpUtility.UrlDecode(tokenFromUrl);
                    string decodedToken = Uri.UnescapeDataString(tokenFromUrl);
                    System.Console.WriteLine($"TOKEN HERE : {tokenFromUrl}");
                    return decodedToken;
                }
                return tokenFromUrl;
            }
            catch (Exception ex)
            {
                // Log the error message
                System.Console.WriteLine(ex.Message);
                return null;
            }
        }


        public static string ToQueryString<T>(T obj) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var queryString = new StringBuilder();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                if (value != null)
                {
                    var encodedKey = Uri.EscapeDataString(property.Name);
                    var encodedValue = Uri.EscapeDataString(value.ToString() ?? string.Empty);
                    if (queryString.Length > 0)
                    {
                        queryString.Append("&");
                    }
                    queryString.Append($"{encodedKey}={encodedValue}");
                }
            }

            return queryString.ToString();
        }

    }
}
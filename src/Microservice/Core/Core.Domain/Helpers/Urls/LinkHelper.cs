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

    }
}
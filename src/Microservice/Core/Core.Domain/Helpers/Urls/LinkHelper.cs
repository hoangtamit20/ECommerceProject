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

    }
}
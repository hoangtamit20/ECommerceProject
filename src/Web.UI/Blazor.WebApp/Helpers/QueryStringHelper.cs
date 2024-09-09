using Microsoft.AspNetCore.WebUtilities;

namespace Blazor.WebApp
{
    public static class QueryStringHelper
    {
        public static T? GetQueryParameter<T>(string url, string parameterName, Func<string, T> parser)
        {
            var uri = new Uri(url);
            var query = QueryHelpers.ParseQuery(uri.Query);

            if (query.TryGetValue(parameterName, out var value))
            {
                return parser(value.ToString());
            }

            return default;
        }
    }
}
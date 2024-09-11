using System.Reflection;
using System.Web;
using Microsoft.AspNetCore.Components;
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


        public static T GetQueryParameters<T>(NavigationManager navigationManager) where T : new()
        {
            var result = new T();
            var uri = new Uri(navigationManager.Uri);
            var query = HttpUtility.ParseQueryString(uri.Query);

            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var queryParamValue = query[property.Name.ToLower()];
                if (!string.IsNullOrEmpty(queryParamValue))
                {
                    try
                    {
                        var convertedValue = Convert.ChangeType(queryParamValue, property.PropertyType);
                        property.SetValue(result, convertedValue);
                    }
                    catch(Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
            return result;
        }
    }
}
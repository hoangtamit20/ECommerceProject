using System.Reflection;
using System.Text;
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
                        if (property.PropertyType.IsEnum)
                        {
                            // Special handling for enum types
                            var enumType = property.PropertyType;
                            var enumValue = Enum.Parse(enumType, queryParamValue, ignoreCase: true);
                            property.SetValue(result, enumValue);
                        }
                        else
                        {
                            // Handle other types
                            var convertedValue = Convert.ChangeType(queryParamValue, property.PropertyType);
                            property.SetValue(result, convertedValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to convert query parameter '{property.Name}' with value '{queryParamValue}' to type '{property.PropertyType}'.", ex);
                    }
                }
            }
            return result;
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
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Core.Domain
{
    public class ErrorHelper
    {
        public static List<ErrorDetail> GetModelStateError(ModelStateDictionary? modelState)
        {
            if (modelState == null)
            {
                return new();
            }
            if (modelState.IsValid)
            {
                return new();
            }
            else
            {
                return modelState.Keys
                    .SelectMany(key => modelState[key]!.Errors.Select(x => new ErrorDetail
                    {
                        ErrorScope = CErrorScope.Field,
                        Field = $"{key}_Error",
                        Error = x.ErrorMessage
                    }))
                    .ToList();
            }
        }
    }
}
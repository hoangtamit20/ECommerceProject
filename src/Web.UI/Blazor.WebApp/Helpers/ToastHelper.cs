using Core.Domain;

namespace Blazor.WebApp
{
    public class ToastHelper
    {
        public static void ToastError<T>(ResponseResult<T>? response,
            ToastModel model, CToastType toastType = CToastType.Error)
        {
            string pageErrors = string.Empty;
            if (response != null)
            {
                var errorList = response.Errors.Where(i => i.ErrorScope != CErrorScope.Field)
                .Select(i => i.Error)
                .ToList();
                if (!errorList.IsNullOrEmpty())
                {
                    pageErrors = errorList.ToMultilineString();
                }
            }
            if (!string.IsNullOrEmpty(pageErrors))
            {
                model.IsVisible = true;
                model.Message = pageErrors;
                model.Status = toastType;
                model.Position = CToastPosition.TopRight;
                model.TimeToClose = 5000;
                model.Title = $"Notification {toastType.ToString()}";
            }
        }
    }
}
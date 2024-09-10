using Core.Domain;
using Microsoft.JSInterop;

namespace Blazor.WebApp
{
    public class ToastHelper
    {
        public static string ErrorMessage<T>(ResponseResult<T>? response)
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
            return pageErrors;
        }
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

        public static void ToastInfo<T>(ToastModel toastModel,
            CToastType toastType = CToastType.Info, string? message = null)
        {
            toastModel.IsVisible = true;
            toastModel.Message = message ?? string.Empty;
            toastModel.Position = CToastPosition.TopRight;
            toastModel.Status = toastType;
            toastModel.TimeToClose = 5;
            toastModel.Title = $"Notification {toastType.ToString()}";
        }
    }
}
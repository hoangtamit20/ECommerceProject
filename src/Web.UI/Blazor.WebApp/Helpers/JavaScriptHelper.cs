using Core.Domain;
using Microsoft.JSInterop;

namespace Blazor.WebApp
{
    public class JavaScriptHelper
    {
        public static async Task CloseErrorAsync(IJSRuntime jSRuntime, string idFrame, int duration)
        {
            await jSRuntime.InvokeVoidAsync("hideElementsAfterDelay", idFrame, duration);
        }

        public static async Task AnimationIconResultPageAsync(ResultMessage message, IJSRuntime jsRuntime)
        {
            if (message != null && !string.IsNullOrEmpty(message.Message)
                && message.Level != CNotificationLevel.None
                && message.NotificationType != CNotificationType.None)
            {
                string templateJsonFile = string.Empty;
                templateJsonFile = $"{message.NotificationType.ToString()}_Notification_Template_{message.Level}.json";
                await jsRuntime.InvokeVoidAsync(identifier: "loadLottieAnimation", args: templateJsonFile);
                await Task.CompletedTask;
            }
        }

        public static async Task AnimationIconPopupAsync(IJSRuntime jsRuntime, CNotificationLevel level = CNotificationLevel.Info)
        {
            string templateJsonFile = string.Empty;
            templateJsonFile = $"Pop_Up_{level.ToString()}.json";
            await jsRuntime.InvokeVoidAsync(identifier: "loadLottieAnimation", args: templateJsonFile);
            await Task.CompletedTask;
        }


        public static async Task<string> GetCurrentUrlAsync(IJSRuntime jsRuntime)
        {
            var currentUrl = await jsRuntime.InvokeAsync<string>(identifier: "getCurrentUrl");
            return currentUrl;
        }
    }
}
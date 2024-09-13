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
                // handle for success
                templateJsonFile = $"{message.NotificationType.ToString()}_Notification_Template_{message.Level}.json";
                await jsRuntime.InvokeVoidAsync(identifier: "loadLottieAnimation", args: templateJsonFile);
                await Task.CompletedTask;
            }
        }
    }
}
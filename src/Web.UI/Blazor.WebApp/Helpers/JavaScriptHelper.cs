using Microsoft.JSInterop;

namespace Blazor.WebApp
{
    public class JavaScriptHelper
    {
        public static async Task CloseErrorAsync(IJSRuntime jSRuntime, string idFrame, int duration)
        {
            await jSRuntime.InvokeVoidAsync("hideElementsAfterDelay", idFrame, duration);
        }
    }
}
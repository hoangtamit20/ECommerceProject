using System.ComponentModel;

namespace Blazor.WebApp
{
    public class ToastModel
    {
        public CToastType Status { get; set; }
        public CToastPosition Position { get; set; }
        public bool IsVisible { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int TimeToClose { get; set; } = 0;
    }

    public enum CToastType
    {
        None = 0,
        [Description(description: "success")]
        Success = 1,
        [Description(description: "warning")]
        Warning = 2,
        [Description(description: "danger")]
        Error = 3,
        [Description(description: "info")]
        Info = 4
    }

    public enum CToastPosition
    {
        None = 0,
        [Description("top-0 end-0")]
        TopRight = 1,
        [Description("top-0 start-0")]
        TopLeft = 2,
        [Description("bottom-0 end-0")]
        BottomLeft = 3,
        [Description("bottom-0 end-0")]
        BottomRight = 4,
        Center = 5
    }
}
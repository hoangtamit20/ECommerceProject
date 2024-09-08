using System.ComponentModel;

namespace Web.BlazorWasm
{
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
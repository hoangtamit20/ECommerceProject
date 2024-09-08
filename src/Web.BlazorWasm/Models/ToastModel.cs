namespace Web.BlazorWasm
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
}
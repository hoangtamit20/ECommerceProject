namespace Blazor.WebApp
{
    public class StateContainer
    {
        public string Message { get; set; } = string.Empty;

        public void SetMessage(string message)
        {
            Message = message;
        }

        public string GetMessage()
        {
            return Message;
        }
    }
}
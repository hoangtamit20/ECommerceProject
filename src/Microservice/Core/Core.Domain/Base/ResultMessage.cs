namespace Core.Domain
{
    public class ResultMessage
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
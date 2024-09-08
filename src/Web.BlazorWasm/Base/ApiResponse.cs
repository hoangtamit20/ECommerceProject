namespace Core.Domain
{
    public enum CErrorScope
    {
        None = 0,
        Field = 1,
        FormSummary = 2,
        PageSumarry = 3,
        RedirectPage = 4,
        Global = 5
    }

    public class ErrorDetail
    {
        public CErrorScope ErrorScope { get; set; }
        public string Field { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public ResponseResult<T> Result { get; set; } = new ResponseResult<T>();
    }

    public class ResponseResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public List<ErrorDetail> Errors { get; set; } = new List<ErrorDetail>();
    }
}
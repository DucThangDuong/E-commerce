namespace API.DTOs
{
    public class ApiSuccessResponse<T>
    {
        public bool Success { get; } = true;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class ApiErrorResponse
    {
        public bool Success { get; } = false;
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public object? Errors { get; set; }
        public string? TraceId { get; set; }
    }
}

namespace backend.Utilities;

public class Response {
    public bool Success { get; set; }
    public Object? Value { get; set; }
    public string? Error { get; set; }
    public string? ErrorCode { get; set; }

    public Response(Object value, string? errorCode = null, bool isError = false)
    {
        Success = !isError;

        if (isError) {
            Error = value?.ToString() ?? ErrorMessage.ErrorMessages["Error"];
            ErrorCode = errorCode;
        }
        else Value = value;
    }
}
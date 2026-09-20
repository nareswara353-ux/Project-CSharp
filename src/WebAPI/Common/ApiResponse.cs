using System.Text.Json.Serialization;

namespace WebAPI.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public string? ErrorCode { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }

    [JsonIgnore]
    public int StatusCode { get; init; } = 200;

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Message = message,
        Data = data,
        StatusCode = 200
    };

    public static ApiResponse<T> Created(T data, string? message = null) => new()
    {
        Success = true,
        Message = message ?? "Resource created successfully.",
        Data = data,
        StatusCode = 201
    };

    public static ApiResponse<T> Fail(string error, string? errorCode = null, int statusCode = 400) => new()
    {
        Success = false,
        Message = error,
        ErrorCode = errorCode,
        StatusCode = statusCode
    };

    public static ApiResponse<T> Fail(IReadOnlyList<string> errors, string? errorCode = null, int statusCode = 400) => new()
    {
        Success = false,
        Message = "One or more validation errors occurred.",
        Errors = errors,
        ErrorCode = errorCode,
        StatusCode = statusCode
    };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Success(string? message = null) => new()
    {
        Success = true,
        Message = message,
        StatusCode = 200
    };

    public static ApiResponse Failure(string error, string? errorCode = null, int statusCode = 400) => new()
    {
        Success = false,
        Message = error,
        ErrorCode = errorCode,
        StatusCode = statusCode
    };
}

namespace PropertyPulse.App.Services;

public record ApiResult<T>(T? Value, ApiError Error, string? Message = null)
{
	public bool IsSuccess => Error == ApiError.None;

	public static ApiResult<T> Success(T value) => new(value, ApiError.None);

	public static ApiResult<T> Failure(ApiError error, string? message = null) => new(default, error, message);
}

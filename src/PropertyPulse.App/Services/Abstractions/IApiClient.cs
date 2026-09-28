namespace PropertyPulse.App.Services;

public interface IApiClient
{
	Task<ApiResult<T>> GetAsync<T>(string path, CancellationToken cancellationToken = default);

	Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
		string path,
		TRequest body,
		bool authenticated = true,
		CancellationToken cancellationToken = default);
}

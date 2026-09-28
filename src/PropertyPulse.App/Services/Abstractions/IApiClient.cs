namespace PropertyPulse.App.Services;

public interface IApiClient
{
	Task<ApiResult<T>> GetAsync<T>(string path, CancellationToken cancellationToken = default);

	Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
		string path,
		TRequest body,
		bool authenticated = true,
		CancellationToken cancellationToken = default);

	Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(
		string path,
		TRequest body,
		CancellationToken cancellationToken = default);

	Task<ApiResult<Unit>> DeleteAsync(string path, CancellationToken cancellationToken = default);

	Task<ApiResult<TResponse>> SendMultipartAsync<TResponse>(
		HttpMethod method,
		string path,
		IReadOnlyDictionary<string, string> fields,
		IReadOnlyCollection<PendingPhoto> photos,
		CancellationToken cancellationToken = default);
}

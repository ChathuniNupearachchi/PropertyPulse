using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PropertyPulse.App.Services;

public class ApiClient(HttpClient httpClient, IApiSettings apiSettings, ISessionService sessionService) : IApiClient
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

	public Task<ApiResult<T>> GetAsync<T>(string path, CancellationToken cancellationToken = default) =>
		SendAsync<T>(HttpMethod.Get, path, content: null, authenticated: true, cancellationToken);

	public Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
		string path,
		TRequest body,
		bool authenticated = true,
		CancellationToken cancellationToken = default) =>
		SendAsync<TResponse>(HttpMethod.Post, path, JsonContent.Create(body, options: JsonOptions), authenticated, cancellationToken);

	private async Task<ApiResult<T>> SendAsync<T>(
		HttpMethod method,
		string path,
		HttpContent? content,
		bool authenticated,
		CancellationToken cancellationToken)
	{
		if (!TryBuildUri(path, out var uri))
		{
			return ApiResult<T>.Failure(ApiError.Network, "The server address is not valid.");
		}

		using var request = new HttpRequestMessage(method, uri) { Content = content };
		if (authenticated && sessionService.Current is { } session)
		{
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
		}

		HttpResponseMessage response;
		try
		{
			response = await httpClient.SendAsync(request, cancellationToken);
		}
		catch (HttpRequestException)
		{
			return ApiResult<T>.Failure(ApiError.Network, "Could not reach the server.");
		}
		catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			return ApiResult<T>.Failure(ApiError.Network, "The server took too long to answer.");
		}

		using (response)
		{
			return await ToResultAsync<T>(response, authenticated, cancellationToken);
		}
	}

	private async Task<ApiResult<T>> ToResultAsync<T>(HttpResponseMessage response, bool authenticated, CancellationToken cancellationToken)
	{
		if (response.IsSuccessStatusCode)
		{
			return await ReadValueAsync<T>(response, cancellationToken);
		}

		var message = await ReadProblemMessageAsync(response, cancellationToken);

		switch (response.StatusCode)
		{
			case HttpStatusCode.Unauthorized:
				if (authenticated)
				{
					await sessionService.EndAsync(SessionEndReason.Expired);
				}

				return ApiResult<T>.Failure(ApiError.Unauthorized, message ?? "Not authorized.");

			case HttpStatusCode.Forbidden:
				return ApiResult<T>.Failure(ApiError.Forbidden, "You do not have permission to do that.");

			default:
				var error = (int)response.StatusCode >= 500 ? ApiError.Server : ApiError.Rejected;
				return ApiResult<T>.Failure(error, message ?? "The request failed.");
		}
	}

	private static async Task<ApiResult<T>> ReadValueAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
	{
		try
		{
			var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
			return value is null
				? ApiResult<T>.Failure(ApiError.Server, "The server returned an empty response.")
				: ApiResult<T>.Success(value);
		}
		catch (JsonException)
		{
			return ApiResult<T>.Failure(ApiError.Server, "The server returned an unreadable response.");
		}
	}

	private static async Task<string?> ReadProblemMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
	{
		try
		{
			await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

			if (document.RootElement.ValueKind != JsonValueKind.Object)
			{
				return null;
			}

			foreach (var name in new[] { "detail", "title" })
			{
				if (document.RootElement.TryGetProperty(name, out var property) &&
					property.ValueKind == JsonValueKind.String &&
					!string.IsNullOrWhiteSpace(property.GetString()))
				{
					return property.GetString();
				}
			}

			return null;
		}
		catch (JsonException)
		{
			return null;
		}
	}

	private bool TryBuildUri(string path, out Uri uri)
	{
		var baseUrl = apiSettings.BaseUrl.TrimEnd('/') + "/";

		if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) &&
			Uri.TryCreate(baseUri, path.TrimStart('/'), out var result))
		{
			uri = result;
			return true;
		}

		uri = baseUri!;
		return false;
	}
}

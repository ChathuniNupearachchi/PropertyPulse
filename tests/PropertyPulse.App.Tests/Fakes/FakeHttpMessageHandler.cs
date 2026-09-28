using System.Net;
using System.Text;

namespace PropertyPulse.App.Tests;

public record RecordedRequest(Uri? Uri, string? Authorization, string? Body);

public class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
	public List<RecordedRequest> Requests { get; } = [];

	public static FakeHttpMessageHandler Returning(HttpStatusCode statusCode, string? json = null) =>
		new(_ => new HttpResponseMessage(statusCode)
		{
			Content = json is null ? null : new StringContent(json, Encoding.UTF8, "application/json")
		});

	public static FakeHttpMessageHandler Throwing(Exception exception) =>
		new(_ => throw exception);

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
		Requests.Add(new RecordedRequest(request.RequestUri, request.Headers.Authorization?.ToString(), body));

		return respond(request);
	}
}

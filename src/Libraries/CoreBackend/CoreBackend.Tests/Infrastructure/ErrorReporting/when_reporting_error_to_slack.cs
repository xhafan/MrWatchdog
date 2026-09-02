using System.Net;
using System.Text.Json;
using CoreBackend.Infrastructure.ErrorReporting;
using CoreBackend.TestsShared.HttpClients;
using Microsoft.Extensions.Options;

namespace CoreBackend.Tests.Infrastructure.ErrorReporting;

[TestFixture]
public class when_reporting_error_to_slack
{
    [Test]
    public async Task it_does_not_send_when_the_channel_is_empty()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        var reporter = _createReporter(handler, " ");

        await reporter.Report(_report());

        handler.RequestCount.ShouldBe(0);
    }

    [Test]
    public async Task it_posts_the_error_as_json()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        var reporter = _createReporter(handler, "#web-app-staging-errors");

        await reporter.Report(_report());

        handler.RequestCount.ShouldBe(1);
        handler.Method.ShouldBe(HttpMethod.Post);
        handler.RequestUri.ShouldBe(new Uri("https://hooks.slack.test/services/error-reporter"));
        handler.MediaType.ShouldBe("application/json");
        using var payload = JsonDocument.Parse(handler.Body.ShouldNotBeNull());
        payload.RootElement.GetProperty("channel").GetString().ShouldBe("#web-app-staging-errors");
        payload.RootElement.GetProperty("username").GetString().ShouldBe("SmartGuide web app errors");
        payload.RootElement.GetProperty("icon_emoji").GetString().ShouldBe(":warning:");
        var text = payload.RootElement.GetProperty("text").GetString().ShouldNotBeNull();
        text.ShouldContain("RequestId: request-123");
        text.ShouldContain("TimeStamp: 2026-09-01 10:11:12.345 UTC");
        text.ShouldContain("Message: Failure &lt;script&gt;&amp;&lt;/script&gt;");
        text.ShouldNotContain("<!channel>");
    }

    [Test]
    public async Task it_throws_for_an_unsuccessful_response()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.BadGateway);
        var reporter = _createReporter(handler, "#web-app-staging-errors");

        await Should.ThrowAsync<HttpRequestException>(() => reporter.Report(_report()));
    }

    private static SlackErrorReporter _createReporter(
        RecordingHttpMessageHandler handler,
        string channel
    )
    {
        var factory = new HttpClientFactory(new HttpClient(handler));
        var options = Options.Create(new SlackErrorReportingOptions
        {
            WebHookUrl = "https://hooks.slack.test/services/error-reporter",
            Channel = channel
        });
        return new SlackErrorReporter(factory, options);
    }

    private static ErrorReport _report() => new(
        "Failure <script>&</script><!channel>",
        "request-123",
        new DateTime(2026, 9, 1, 10, 11, 12, 345, DateTimeKind.Utc)
    );

    private sealed class RecordingHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? MediaType { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            RequestCount++;
            Method = request.Method;
            RequestUri = request.RequestUri;
            MediaType = request.Content?.Headers.ContentType?.MediaType;
            Body = request.Content == null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(statusCode);
        }
    }
}

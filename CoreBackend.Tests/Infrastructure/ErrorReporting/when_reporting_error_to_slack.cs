using System.Net;
using System.Text.Json;
using CoreBackend.Infrastructure.Configurations;
using CoreBackend.Infrastructure.ErrorReporting;
using CoreBackend.Infrastructure.Rebus.ErrorHandlers;
using CoreBackend.Messages;
using CoreBackend.TestsShared.HttpClients;
using Microsoft.Extensions.DependencyInjection;
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
        payload.RootElement.GetProperty("username").GetString().ShouldBe("Application errors");
        payload.RootElement.GetProperty("icon_emoji").GetString().ShouldBe(":warning:");
        var text = payload.RootElement.GetProperty("text").GetString().ShouldNotBeNull();
        text.ShouldContain("RequestId: request-123");
        text.ShouldContain("TimeStamp: 2026-09-01 10:11:12.345 UTC");
        text.ShouldContain("Message: Failure &lt;script&gt;&amp;&lt;/script&gt;");
        text.ShouldNotContain("<!channel>");
        text.ShouldNotContain("Job: <");
    }

    [Test]
    public async Task it_throws_for_an_unsuccessful_response()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.BadGateway);
        var reporter = _createReporter(handler, "#web-app-staging-errors");

        await Should.ThrowAsync<HttpRequestException>(() => reporter.Report(_report()));
        handler.RequestCount.ShouldBe(1);
    }

    [Test]
    public async Task it_uses_the_configured_application_identity()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        var reporter = new SlackErrorReporter(new HttpClientFactory(new HttpClient(handler)),
            Options.Create(new SlackErrorReportingOptions
            {
                WebHookUrl = "https://hooks.slack.test/services/another-app",
                Channel = "#backend-errors",
                ApplicationName = "Another backend"
            }), Options.Create(new RuntimeOptions { Url = "https://backend.example.test" }));

        await reporter.Report(_report());

        using var payload = JsonDocument.Parse(handler.Body.ShouldNotBeNull());
        payload.RootElement.GetProperty("username").GetString().ShouldBe("Another backend errors");
    }

    [TestCase("x")]
    [TestCase("&")]
    [TestCase("<")]
    [TestCase(">")]
    [TestCase("🧭")]
    public async Task it_keeps_long_errors_in_one_bounded_message_without_damaging_escaped_text(string character)
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        var report = _report() with { Message = string.Concat(Enumerable.Repeat(character, 5000)) };
        var originalMessage = report.Message;

        await _createReporter(handler, "#web-app-staging-errors").Report(report);

        handler.RequestCount.ShouldBe(1);
        using var payload = JsonDocument.Parse(handler.Body.ShouldNotBeNull());
        var text = payload.RootElement.GetProperty("text").GetString().ShouldNotBeNull();
        text.Length.ShouldBeLessThanOrEqualTo(4000);
        text.ShouldContain("RequestId: request-123");
        text.ShouldContain("TimeStamp: 2026-09-01 10:11:12.345 UTC");
        text.ShouldEndWith("Details truncated; see application logs for the full error.");
        text.ShouldNotMatch("&(?!amp;|lt;|gt;)");
        text.ShouldNotContain("\uFFFD");
        report.Message.ShouldBe(originalMessage);
    }

    [Test]
    public async Task it_counts_escaping_expansion_towards_the_message_limit()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);

        await _createReporter(handler, "#web-app-staging-errors").Report(_report() with { Message = new string('<', 1000) });

        using var payload = JsonDocument.Parse(handler.Body.ShouldNotBeNull());
        var text = payload.RootElement.GetProperty("text").GetString().ShouldNotBeNull();
        text.Length.ShouldBeLessThanOrEqualTo(4000);
        text.ShouldContain("&lt;");
        text.ShouldContain("Details truncated");
    }

    [TestCase(3999)]
    [TestCase(4000)]
    public async Task it_preserves_errors_that_fit_the_complete_message_limit(int length)
    {
        const string prefix = "RequestId: request-123\nTimeStamp: 2026-09-01 10:11:12.345 UTC\nMessage: ";
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        var message = new string('x', length - prefix.Length);

        await _createReporter(handler, "#web-app-staging-errors").Report(_report() with { Message = message });

        using var payload = JsonDocument.Parse(handler.Body.ShouldNotBeNull());
        payload.RootElement.GetProperty("text").GetString().ShouldBe(prefix + message);
    }

    [TestCase("https://backend.example.test", 20)]
    [TestCase("https://backend.example.test/", 20)]
    [TestCase("https://backend.example.test", 6000)]
    [TestCase("https://backend.example.test/", 6000)]
    public async Task it_preserves_the_failed_job_link_and_only_truncates_long_details(string applicationUrl, int detailsLength)
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        using var services = new ServiceCollection()
            .AddSingleton<IHttpClientFactory>(new HttpClientFactory(new HttpClient(handler)))
            .AddSingleton(Options.Create(new SlackErrorReportingOptions
            {
                WebHookUrl = "https://hooks.slack.test/services/error-reporter",
                Channel = "#web-app-staging-errors"
            }))
            .AddSingleton(Options.Create(new RuntimeOptions { Url = applicationUrl }))
            .AddSingleton<SlackErrorReporter>()
            .BuildServiceProvider();
        var report = _report();
        var failure = new FailedMessageReport(Guid.Parse("171b2449-b644-4ae2-ae62-4279bb2ec4bc"),
            typeof(TestCommand), report.RequestId, new string('x', detailsLength), report.OccurredAtUtc);

        await services.GetRequiredService<SlackErrorReporter>().Report(report with
        {
            Message = failure.ErrorDetails,
            Origin = ErrorReportOrigin.FailedJob,
            FailedMessage = failure
        });

        handler.RequestCount.ShouldBe(1);
        using var payload = JsonDocument.Parse(handler.Body.ShouldNotBeNull());
        var text = payload.RootElement.GetProperty("text").GetString().ShouldNotBeNull();
        text.ShouldContain("<https://backend.example.test/api/Jobs/171b2449-b644-4ae2-ae62-4279bb2ec4bc|TestCommand>");
        text.Length.ShouldBeLessThanOrEqualTo(4000);
        text.ShouldContain("RequestId: request-123");
        text.ShouldContain("TimeStamp: 2026-09-01 10:11:12.345 UTC");
        if (detailsLength > 4000)
        {
            text.ShouldEndWith("Details truncated; see application logs for the full error.");
        }
        else
        {
            text.ShouldEndWith(failure.ErrorDetails);
            text.ShouldNotContain("Details truncated");
        }
        failure.ErrorDetails.Length.ShouldBe(detailsLength);
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
        return new SlackErrorReporter(factory, options, Options.Create(new RuntimeOptions { Url = "https://backend.example.test" }));
    }

    private static ErrorReport _report() => new(
        "Failure <script>&</script><!channel>",
        "request-123",
        new DateTime(2026, 9, 1, 10, 11, 12, 345, DateTimeKind.Utc),
        Origin: ErrorReportOrigin.Server
    );

    private sealed record TestCommand : Command;

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

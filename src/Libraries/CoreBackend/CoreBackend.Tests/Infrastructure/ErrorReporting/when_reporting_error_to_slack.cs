using System.Net;
using System.Text.Json;
using CoreBackend.Infrastructure.Configurations;
using CoreBackend.Infrastructure.ErrorReporting;
using CoreBackend.Infrastructure.Rebus.ErrorHandlers;
using CoreBackend.Messages;
using CoreBackend.TestsShared.HttpClients;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CoreBackend.Tests.Infrastructure.ErrorReporting;

[TestFixture]
public class when_reporting_error_to_slack
{
    [TestCase(ErrorReportOrigin.Browser, "#frontend-errors")]
    [TestCase(ErrorReportOrigin.Server, "#backend-errors")]
    [TestCase(ErrorReportOrigin.FailedJob, "#backend-errors")]
    public async Task it_uses_one_legacy_webhook_and_selects_the_channel_from_the_error_origin(ErrorReportOrigin origin, string expectedChannel)
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        var options = new SlackErrorReportingOptions();
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SlackErrorReporting:WebHookUrl"] = "https://hooks.slack.test/services/shared",
            ["SlackErrorReporting:BackendChannel"] = "#backend-errors",
            ["SlackErrorReporting:FrontendChannel"] = "#frontend-errors"
        }).Build().GetSection(SlackErrorReportingOptions.SectionName).Bind(options);
        var reporter = new SlackErrorReporter(new HttpClientFactory(new HttpClient(handler)),
            Options.Create(options), Options.Create(new RuntimeOptions { Url = "https://backend.example.test" }), _environment());

        await reporter.Report(_report() with { Origin = origin });

        handler.RequestCount.ShouldBe(1);
        handler.RequestUri.ShouldBe(new Uri("https://hooks.slack.test/services/shared"));
        using var payload = JsonDocument.Parse(handler.Body.ShouldNotBeNull());
        payload.RootElement.GetProperty("channel").GetString().ShouldBe(expectedChannel);
        payload.RootElement.GetProperty("text").GetString().ShouldNotBeNull()
            .ShouldContain(origin == ErrorReportOrigin.Browser ? "Component: Frontend" : "Component: Backend");
    }

    [Test]
    public async Task it_does_not_send_when_the_shared_webhook_is_empty()
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
        var reporter = _createReporter(handler);

        await reporter.Report(_report());

        handler.RequestCount.ShouldBe(1);
        handler.Method.ShouldBe(HttpMethod.Post);
        handler.RequestUri.ShouldBe(new Uri("https://hooks.slack.test/services/shared"));
        handler.MediaType.ShouldBe("application/json");
        using var payload = JsonDocument.Parse(handler.Body.ShouldNotBeNull());
        payload.RootElement.GetProperty("channel").GetString().ShouldBe("#backend-errors");
        payload.RootElement.GetProperty("username").GetString().ShouldBe("Application errors");
        payload.RootElement.GetProperty("icon_emoji").GetString().ShouldBe(":warning:");
        var text = payload.RootElement.GetProperty("text").GetString().ShouldNotBeNull();
        text.ShouldContain("Application: Application");
        text.ShouldContain("Environment: Staging");
        text.ShouldContain("Component: Backend");
        text.ShouldContain("RequestId: request-123");
        text.ShouldContain("TimeStamp: 2026-09-01 10:11:12.345 UTC");
        text.ShouldContain("Message: Failure &lt;script&gt;&amp;&lt;/script&gt;");
        text.ShouldNotContain("<!channel>");
        text.ShouldNotContain("Job: <");
    }

    [TestCase(ErrorReportOrigin.Browser, "#frontend-errors")]
    [TestCase(ErrorReportOrigin.Server, "#backend-errors")]
    public async Task it_throws_without_rerouting_an_unsuccessful_response(ErrorReportOrigin origin, string expectedChannel)
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.BadGateway);
        var reporter = _createReporter(handler);

        await Should.ThrowAsync<HttpRequestException>(() => reporter.Report(_report() with { Origin = origin }));
        handler.RequestCount.ShouldBe(1);
        handler.RequestUri.ShouldBe(new Uri("https://hooks.slack.test/services/shared"));
        using var payload = JsonDocument.Parse(handler.Body.ShouldNotBeNull());
        payload.RootElement.GetProperty("channel").GetString().ShouldBe(expectedChannel);
    }

    [TestCase(ErrorReportOrigin.Browser)]
    [TestCase(ErrorReportOrigin.Server)]
    public async Task it_honors_cancellation_without_sending_to_either_destination(ErrorReportOrigin origin)
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            _createReporter(handler).Report(_report() with { Origin = origin }, cancellation.Token));

        handler.RequestCount.ShouldBe(0);
    }

    [Test]
    public async Task it_escapes_the_application_and_environment_in_the_message()
    {
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        var reporter = new SlackErrorReporter(new HttpClientFactory(new HttpClient(handler)),
            Options.Create(new SlackErrorReportingOptions
            {
                WebHookUrl = "https://hooks.slack.test/services/another-app",
                BackendChannel = "#backend-errors",
                ApplicationName = "Another <backend>&"
            }), Options.Create(new RuntimeOptions { Url = "https://backend.example.test" }), _environment("Staging<&>"));

        await reporter.Report(_report());

        using var payload = JsonDocument.Parse(handler.Body.ShouldNotBeNull());
        var text = payload.RootElement.GetProperty("text").GetString().ShouldNotBeNull();
        text.ShouldContain("Application: Another &lt;backend&gt;&amp;");
        text.ShouldContain("Environment: Staging&lt;&amp;&gt;");
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

        await _createReporter(handler).Report(report);

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

        await _createReporter(handler).Report(_report() with { Message = new string('<', 1000) });

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
        const string prefix = "Application: Application\nEnvironment: Staging\nComponent: Backend\nRequestId: request-123\nTimeStamp: 2026-09-01 10:11:12.345 UTC\nMessage: ";
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        var message = new string('x', length - prefix.Length);

        await _createReporter(handler).Report(_report() with { Message = message });

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
                WebHookUrl = "https://hooks.slack.test/services/shared",
                BackendChannel = "#backend-errors"
            }))
            .AddSingleton(Options.Create(new RuntimeOptions { Url = applicationUrl }))
            .AddSingleton(_environment())
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
        string webhook = "https://hooks.slack.test/services/shared"
    )
    {
        var factory = new HttpClientFactory(new HttpClient(handler));
        var options = Options.Create(new SlackErrorReportingOptions
        {
            WebHookUrl = webhook,
            BackendChannel = "#backend-errors",
            FrontendChannel = "#frontend-errors"
        });
        return new SlackErrorReporter(factory, options, Options.Create(new RuntimeOptions { Url = "https://backend.example.test" }), _environment());
    }

    private static IHostEnvironment _environment(string name = "Staging")
    {
        var environment = A.Fake<IHostEnvironment>();
        A.CallTo(() => environment.EnvironmentName).Returns(name);
        return environment;
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

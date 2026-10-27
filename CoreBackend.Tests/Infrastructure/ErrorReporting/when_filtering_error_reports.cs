using System.Net;
using CoreBackend.Infrastructure.ErrorReporting;
using CoreBackend.TestsShared.Infrastructure.ErrorReporting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace CoreBackend.Tests.Infrastructure.ErrorReporting;

[TestFixture]
public class when_filtering_error_reports
{
    [TestCase("00:00:00")]
    [TestCase("-01:00:00")]
    public void it_rejects_a_non_positive_suppression_window(string window)
    {
        var services = new ServiceCollection();
        services.AddErrorReporting(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ErrorReporting:SuppressionWindow"] = window
        }).Build());
        using var provider = services.BuildServiceProvider();

        Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IOptions<ErrorReportingOptions>>().Value)
            .Message.ShouldContain("SuppressionWindow must be positive");
    }

    [TestCase(LogEventLevel.Error)]
    [TestCase(LogEventLevel.Fatal)]
    public async Task rate_limited_http_failures_stay_in_logs_without_being_reported(LogEventLevel level)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        var exception = Should.Throw<HttpRequestException>(() => response.EnsureSuccessStatusCode());

        var result = await _logAndDeliver(exception, _ignoreRateLimits, level: level);

        result.Reports.ShouldBeEmpty();
        result.Log.Exception.ShouldBeSameAs(exception);
        result.Log.Level.ShouldBe(level);
    }

    [TestCaseSource(nameof(_exceptionCases))]
    public async Task it_filters_exception_trees_without_hiding_other_failures(Exception? exception, bool shouldReport)
    {
        var result = await _logAndDeliver(exception, _ignoreRateLimits);

        result.Log.Exception.ShouldBeSameAs(exception);
        result.Log.RenderMessage().ShouldBe("Request failed; the text mentions 429.");
        result.Reports.Count.ShouldBe(shouldReport ? 1 : 0);
        if (shouldReport)
        {
            result.Reports[0].Message.ShouldContain("Request failed; the text mentions 429.");
            result.Reports[0].Origin.ShouldBe(ErrorReportOrigin.Server);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task application_configured_rules_ignore_direct_and_wrapped_exceptions(bool wrapped)
    {
        Exception exception = new ExpectedApplicationException();
        if (wrapped) exception = new InvalidOperationException("Wrapped", exception);

        var result = await _logAndDeliver(exception,
            options => options.IgnoredExceptions.Add(candidate => candidate is ExpectedApplicationException));

        result.Reports.ShouldBeEmpty();
        result.Log.Exception.ShouldBeSameAs(exception);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task aggregate_filtering_combines_application_configured_rules(bool containsUnexpectedFailure)
    {
        var failures = new List<Exception> { _rateLimit(), new ExpectedApplicationException() };
        if (containsUnexpectedFailure) failures.Add(new NullReferenceException("Unexpected failure"));

        var result = await _logAndDeliver(new AggregateException(failures), options =>
        {
            _ignoreRateLimits(options);
            options.IgnoredExceptions.Add(candidate => candidate is ExpectedApplicationException);
        });

        result.Reports.Count.ShouldBe(containsUnexpectedFailure ? 1 : 0);
    }

    [Test]
    public async Task no_exceptions_are_ignored_without_application_configuration()
    {
        var result = await _logAndDeliver(_rateLimit());

        result.Reports.ShouldHaveSingleItem();
    }

    [Test]
    public async Task browser_error_text_mentioning_429_is_still_reported()
    {
        var result = await _logAndDeliver(null, browser: true);

        result.Reports.ShouldHaveSingleItem().Origin.ShouldBe(ErrorReportOrigin.Browser);
    }

    private static IEnumerable<TestCaseData> _exceptionCases()
    {
        yield return new TestCaseData(new InvalidOperationException("Wrapped", _rateLimit()), false)
            .SetName("wrapped_429_is_ignored");
        yield return new TestCaseData(new InvalidOperationException("Wrapped", new Exception("Nested", _rateLimit())), false)
            .SetName("deeply_wrapped_429_is_ignored");
        yield return new TestCaseData(new AggregateException(_rateLimit(), _rateLimit()), false)
            .SetName("aggregate_of_429s_is_ignored");
        yield return new TestCaseData(new AggregateException(_rateLimit(), new AggregateException(new Exception("Wrapped", _rateLimit()))), false)
            .SetName("nested_aggregate_of_429s_is_ignored");
        yield return new TestCaseData(new Exception("Wrapped", new AggregateException(_rateLimit(), _rateLimit())), false)
            .SetName("wrapped_aggregate_of_429s_is_ignored");
        yield return new TestCaseData(new AggregateException(_rateLimit(), new HttpRequestException("Backend failed", null, HttpStatusCode.InternalServerError)), true)
            .SetName("aggregate_with_429_and_500_is_reported");
        yield return new TestCaseData(new AggregateException(_rateLimit(), new AggregateException(new NullReferenceException("Unexpected failure"))), true)
            .SetName("nested_aggregate_with_an_unrelated_failure_is_reported");
        yield return new TestCaseData(new Exception("Wrapped", new AggregateException(_rateLimit(), new InvalidOperationException("Unexpected failure"))), true)
            .SetName("wrapped_mixed_aggregate_is_reported");
        yield return new TestCaseData(new AggregateException(), true)
            .SetName("empty_aggregate_is_reported");
        yield return new TestCaseData(new HttpRequestException("Backend failed", null, HttpStatusCode.InternalServerError), true)
            .SetName("http_500_is_reported");
        yield return new TestCaseData(new HttpRequestException("429 Too Many Requests"), true)
            .SetName("http_failure_without_a_status_code_is_reported");
        yield return new TestCaseData(new InvalidOperationException("429 Too Many Requests"), true)
            .SetName("unrelated_exception_mentioning_429_is_reported");
        yield return new TestCaseData(null, true)
            .SetName("log_without_an_exception_mentioning_429_is_reported");
    }

    private static HttpRequestException _rateLimit() => new("Rate limited", null, HttpStatusCode.TooManyRequests);

    private static void _ignoreRateLimits(ErrorReportingOptions options) =>
        options.IgnoredExceptions.Add(static exception => exception is HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests });

    private static async Task<(LogEvent Log, IReadOnlyList<ErrorReport> Reports)> _logAndDeliver(
        Exception? exception,
        Action<ErrorReportingOptions>? configure = null,
        LogEventLevel level = LogEventLevel.Error,
        bool browser = false)
    {
        var reporter = new RecordingReporter();
        var services = new ServiceCollection();
        services.AddSingleton<IErrorReporter>(reporter);
        services.AddErrorReporting(new ConfigurationBuilder().Build());
        services.AddSingleton<IErrorReportGate>(_ => ErrorReportTestServices.CreateGate());
        if (configure != null) services.Configure(configure);
        using var provider = services.BuildServiceProvider();
        var logs = new RecordingSink();
        using var logger = new LoggerConfiguration().Enrich.FromLogContext()
            .WriteTo.Sink(logs)
            .WriteTo.Sink(provider.GetRequiredService<ErrorReportingSink>())
            .CreateLogger();

        var eventId = ErrorReportingEventIds.BrowserError;
        var eventLogger = browser
            ? logger.ForContext("EventId", new { eventId.Id, eventId.Name }, destructureObjects: true)
            : logger;
        eventLogger.Write(level, exception, "Request failed; the text mentions 429.");
        var delivery = provider.GetServices<IHostedService>().Single();
        await delivery.StartAsync(CancellationToken.None);
        await delivery.StopAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5));

        return (logs.Events.ShouldHaveSingleItem(), reporter.Reports);
    }

    private sealed class ExpectedApplicationException : Exception;

    private sealed class RecordingReporter : IErrorReporter
    {
        public List<ErrorReport> Reports { get; } = [];
        public Task Report(ErrorReport report, CancellationToken cancellationToken = default)
        {
            Reports.Add(report);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];
        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }
}

using CoreBackend.Infrastructure.ErrorReporting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace CoreBackend.Tests.Infrastructure.ErrorReporting;

[TestFixture, NonParallelizable]
public class when_forwarding_server_errors
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task it_classifies_browser_errors_by_event_id_and_defaults_other_errors_to_server(bool browserError)
    {
        var queue = _queue();
        var reporter = new RecordingReporter();
        using var logger = _logger(queue);
        var eventId = ErrorReportingEventIds.BrowserError;
        var eventLogger = browserError
            ? logger.ForContext("EventId", new { eventId.Id, eventId.Name }, destructureObjects: true)
            : logger;

        eventLogger.Error("Failure message");
        await _drain(queue, reporter);

        reporter.Reports.ShouldHaveSingleItem().Origin
            .ShouldBe(browserError ? ErrorReportOrigin.Browser : ErrorReportOrigin.Server);
    }

    [Test]
    public async Task it_delivers_errors_and_fatal_events_with_their_original_context()
    {
        var queue = _queue();
        var reporter = new RecordingReporter();
        using var logger = _logger(queue);
        var before = DateTime.UtcNow;

        logger.ForContext("SourceContext", "Example.Worker")
            .ForContext("RequestId", "request-123")
            .ForContext("MessageId", "message-456")
            .ForContext("MessageType", "ExampleCommand")
            .Error(new InvalidOperationException("Database unavailable"), "Cache backend write failure.");
        logger.Fatal("Application stopped");
        logger.Warning("A warning");
        logger.Information("An ordinary request");

        await _drain(queue, reporter);

        reporter.Reports.Count.ShouldBe(2);
        var report = reporter.Reports[0];
        report.Message.ShouldContain("Cache backend write failure.");
        report.Message.ShouldContain("Example.Worker");
        report.Message.ShouldContain("InvalidOperationException: Database unavailable");
        report.Message.ShouldContain("message-456");
        report.Message.ShouldContain("ExampleCommand");
        report.RequestId.ShouldBe("request-123");
        report.OccurredAtUtc.ShouldBeGreaterThanOrEqualTo(before);
        report.OccurredAtUtc.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Test]
    public async Task it_keeps_suppressed_errors_in_ordinary_logs_without_forwarding_them()
    {
        var queue = _queue();
        var reporter = new RecordingReporter();
        var ordinaryLogs = new RecordingSink();
        using var logger = _logger(queue, ordinaryLogs);

        using (ErrorReportingLogPolicy.Suppress())
        {
            logger.Error("A message attempt failed");
        }
        logger.Error("A separate infrastructure failure");

        await _drain(queue, reporter);

        ordinaryLogs.Events.Count.ShouldBe(2);
        reporter.Reports.ShouldHaveSingleItem().Message.ShouldContain("A separate infrastructure failure");
    }

    [Test]
    public async Task it_bounds_the_queue_and_preserves_already_queued_reports()
    {
        var queue = _queue(capacity: 1);

        queue.TryEnqueue(_report("first")).ShouldBeTrue();
        queue.TryEnqueue(_report("overflow")).ShouldBeFalse();

        var reporter = new RecordingReporter();
        await _drain(queue, reporter);

        reporter.Reports.ShouldHaveSingleItem().Message.ShouldBe("first");
        queue.TryEnqueue(_report("after shutdown")).ShouldBeFalse();
    }

    [Test]
    public async Task it_does_not_forward_when_disabled()
    {
        var queue = _queue(enabled: false);
        using var logger = _logger(queue);
        logger.Error("Local development error");
        var reporter = new RecordingReporter();

        await _drain(queue, reporter);

        reporter.Reports.ShouldBeEmpty();
    }

    [Test]
    public async Task it_does_not_wait_for_delivery_when_logging_another_error()
    {
        var queue = _queue();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var unblock = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reporter = new RecordingReporter
        {
            OnReport = _ =>
            {
                entered.TrySetResult();
                return unblock.Task;
            }
        };
        using var logger = _logger(queue);
        using var delivery = new ErrorReportDeliveryBackgroundService(queue, reporter);
        await delivery.StartAsync(CancellationToken.None);
        logger.Error("first");
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        try
        {
            await Task.Run(() => logger.Error("second")).WaitAsync(TimeSpan.FromSeconds(1));
        }
        finally
        {
            unblock.TrySetResult();
            await delivery.StopAsync(CancellationToken.None);
        }
        reporter.Reports.Count.ShouldBe(2);
    }

    [Test]
    public void it_records_overflow_without_including_report_content_or_forwarding_a_diagnostic()
    {
        var queue = _queue(capacity: 1);
        var logs = new RecordingSink();
        using var logger = _logger(queue, logs);
        var previousLogger = Log.Logger;
        Log.Logger = logger;
        try
        {
            queue.TryEnqueue(_report("first")).ShouldBeTrue();
            queue.TryEnqueue(_report("private report content")).ShouldBeFalse();

            var diagnostic = logs.Events.ShouldHaveSingleItem();
            diagnostic.RenderMessage().ShouldContain("queue is full or closed");
            diagnostic.RenderMessage().ShouldNotContain("private report content");
            ErrorReportingLogPolicy.ShouldReport(diagnostic).ShouldBeFalse();
        }
        finally
        {
            Log.Logger = previousLogger;
        }
    }

    [Test]
    public async Task it_continues_after_delivery_fails_without_retrying_or_reporting_delivery_logs()
    {
        var queue = _queue();
        var logs = new RecordingSink();
        using var logger = _logger(queue, logs);
        var previousLogger = Log.Logger;
        Log.Logger = logger;
        try
        {
            var reporter = new RecordingReporter
            {
                OnReport = report =>
                {
                    Log.Error("Reporter internal error");
                    return report.Message == "first"
                        ? Task.FromException(new HttpRequestException("secret-webhook-url"))
                        : Task.CompletedTask;
                }
            };
            queue.TryEnqueue(_report("first"));
            queue.TryEnqueue(_report("second"));

            await _drain(queue, reporter);

            reporter.Reports.Select(report => report.Message).ShouldBe(new[] { "first", "second" });
            logs.Events.ShouldContain(logEvent => logEvent.RenderMessage().Contains("delivery failed", StringComparison.Ordinal));
            logs.Events.ShouldNotContain(logEvent => logEvent.RenderMessage().Contains("secret-webhook-url", StringComparison.Ordinal));
        }
        finally
        {
            Log.Logger = previousLogger;
        }
    }

    [Test]
    public async Task it_returns_from_shutdown_even_if_the_reporter_ignores_cancellation()
    {
        var queue = _queue(shutdownTimeout: TimeSpan.FromMilliseconds(50));
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var blocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reporter = new RecordingReporter
        {
            OnReport = _ =>
            {
                entered.TrySetResult();
                return blocked.Task;
            }
        };
        using var service = new ErrorReportDeliveryBackgroundService(queue, reporter);
        queue.TryEnqueue(_report("pending"));
        await service.StartAsync(CancellationToken.None);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        try
        {
            await service.StopAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            blocked.TrySetResult();
        }
    }

    [Test]
    public async Task it_times_out_delivery_and_continues_to_the_next_report()
    {
        var queue = _queue(deliveryTimeout: TimeSpan.FromMilliseconds(50));
        var blocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reporter = new RecordingReporter
        {
            OnReport = report => report.Message == "first" ? blocked.Task : Task.CompletedTask
        };
        queue.TryEnqueue(_report("first"));
        queue.TryEnqueue(_report("second"));
        try
        {
            await _drain(queue, reporter);
            reporter.Reports.Select(report => report.Message).ShouldBe(new[] { "first", "second" });
        }
        finally
        {
            blocked.TrySetResult();
        }
    }

    private static ErrorReportQueue _queue(
        int capacity = 10,
        bool enabled = true,
        TimeSpan? shutdownTimeout = null,
        TimeSpan? deliveryTimeout = null
    ) => new(Options.Create(new ErrorReportingOptions
    {
        Enabled = enabled,
        QueueCapacity = capacity,
        ShutdownTimeout = shutdownTimeout ?? TimeSpan.FromSeconds(2),
        DeliveryTimeout = deliveryTimeout ?? TimeSpan.FromSeconds(1)
    }));

    private static Logger _logger(ErrorReportQueue queue, RecordingSink? ordinaryLogs = null) => new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.FromLogContext()
        .WriteTo.Sink(new ErrorReportingSink(queue))
        .WriteTo.Sink(ordinaryLogs ?? new RecordingSink())
        .CreateLogger();

    private static ErrorReport _report(string message) => new(message, "request-123", DateTime.UtcNow, Origin: ErrorReportOrigin.Server);

    private static async Task _drain(ErrorReportQueue queue, IErrorReporter reporter)
    {
        using var service = new ErrorReportDeliveryBackgroundService(queue, reporter);
        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5));
    }

    private sealed class RecordingReporter : IErrorReporter
    {
        public List<ErrorReport> Reports { get; } = [];
        public Func<ErrorReport, Task> OnReport { get; init; } = _ => Task.CompletedTask;

        public Task Report(ErrorReport report, CancellationToken cancellationToken = default)
        {
            Reports.Add(report);
            return OnReport(report);
        }
    }

    private sealed class RecordingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];
        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }
}

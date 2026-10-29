using CoreBackend.Infrastructure.ErrorReporting;
using Serilog;
using static CoreBackend.Tests.Infrastructure.ErrorReporting.when_forwarding_server_errors;

namespace CoreBackend.Tests.Infrastructure.ErrorReporting;

[TestFixture, NonParallelizable]
public class when_forwarding_server_errors_through_process_logger
{
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
            ErrorReportingLogPolicy.ShouldReport(diagnostic, new ErrorReportingOptions()).ShouldBeFalse();
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

}

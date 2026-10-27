using System.Net;
using System.Runtime.CompilerServices;
using CoreBackend.Infrastructure.ErrorReporting;
using CoreBackend.Infrastructure.Rebus.ErrorHandlers;
using CoreBackend.TestsShared.Infrastructure.ErrorReporting;
using CoreBackend.Messages;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Rebus.Messages;
using Rebus.Retry;
using Rebus.Serialization;
using Rebus.Transport;
using Serilog;

namespace CoreBackend.Tests.Infrastructure.ErrorReporting;

[TestFixture]
public class when_fingerprinting_error_reports
{
    [Test]
    public void it_groups_the_same_exception_from_different_callers()
    {
        var first = _fromFirstCaller();
        var second = _fromSecondCaller();

        ErrorReportFingerprint.ForException(first).ShouldBe(ErrorReportFingerprint.ForException(second));
    }

    [Test]
    public void it_distinguishes_exception_types_messages_throw_sites_and_inner_exceptions()
    {
        var failures = new Exception[]
        {
            new InvalidOperationException("Failure"),
            new ArgumentException("Failure"),
            new InvalidOperationException("Another failure"),
            _fromFirstCaller(),
            _fromAnotherThrowSite(),
            new InvalidOperationException("Failure", new ArgumentException("Inner failure")),
            new InvalidOperationException("Failure", new ArgumentException("Different inner failure")),
            new AggregateException(new ArgumentException("First"), new ArgumentException("Second")),
            new AggregateException(new ArgumentException("First"), new ArgumentException("Different"))
        };

        failures.Select(ErrorReportFingerprint.ForException).Distinct().Count().ShouldBe(failures.Length);
    }

    [Test]
    public async Task it_preserves_a_stable_fingerprint_separately_from_request_context()
    {
        var queue = new ErrorReportQueue(Options.Create(new ErrorReportingOptions()));
        using var logger = new LoggerConfiguration().WriteTo.Sink(new ErrorReportingSink(queue)).CreateLogger();
        logger.ForContext("RequestId", "request-one").ForContext("SourceContext", "First.Logger")
            .Error(_fromFirstCaller(), "Failed request {RequestNumber}", 1);
        logger.ForContext("RequestId", "request-two").ForContext("SourceContext", "Second.Logger")
            .Error(_fromSecondCaller(), "Failed request {RequestNumber}", 2);
        var reports = new List<ErrorReport>();
        var reporter = new CollectingReporter(reports);
        using var delivery = new ErrorReportDeliveryBackgroundService(queue, reporter, ErrorReportTestServices.CreateGate());
        await delivery.StartAsync(CancellationToken.None);
        await delivery.StopAsync(CancellationToken.None);

        reports.ShouldHaveSingleItem().RequestId.ShouldBe("request-one");
        reports[0].Fingerprint.ShouldNotBeNullOrWhiteSpace();
        reports[0].Fingerprint!.ShouldNotContain("request-one");
    }

    [Test]
    public void it_distinguishes_http_status_codes_even_when_the_message_is_the_same()
    {
        var unauthorized = new HttpRequestException("Authentication failed", null, HttpStatusCode.Unauthorized);
        var forbidden = new HttpRequestException("Authentication failed", null, HttpStatusCode.Forbidden);

        ErrorReportFingerprint.ForException(unauthorized).ShouldNotBe(ErrorReportFingerprint.ForException(forbidden));
    }

    [Test]
    public void it_uses_source_and_message_for_events_without_exceptions()
    {
        var first = ErrorReportFingerprint.ForMessage("Example.Worker", "Unavailable");

        first.ShouldBe(ErrorReportFingerprint.ForMessage("Example.Worker", "Unavailable"));
        first.ShouldNotBe(ErrorReportFingerprint.ForMessage("Another.Worker", "Unavailable"));
        first.ShouldNotBe(ErrorReportFingerprint.ForMessage("Example.Worker", "Invalid response"));
    }

    [Test]
    public async Task it_groups_failed_jobs_without_using_job_or_request_ids()
    {
        var queue = new ErrorReportQueue(Options.Create(new ErrorReportingOptions()));
        var producer = new QueuedFailedMessageReporter(queue);
        await producer.Report(new FailedMessageReport(Guid.NewGuid(), typeof(string), "first", "Handler failed", DateTime.UtcNow));
        await producer.Report(new FailedMessageReport(Guid.NewGuid(), typeof(string), "second", "Handler failed", DateTime.UtcNow));
        var reports = new List<ErrorReport>();
        using var delivery = new ErrorReportDeliveryBackgroundService(queue, new CollectingReporter(reports), ErrorReportTestServices.CreateGate());
        await delivery.StartAsync(CancellationToken.None);
        await delivery.StopAsync(CancellationToken.None);

        reports.ShouldHaveSingleItem().Origin.ShouldBe(ErrorReportOrigin.FailedJob);
    }

    [Test]
    public async Task it_groups_failed_jobs_even_when_rebus_error_timestamps_are_different()
    {
        var queue = new ErrorReportQueue(Options.Create(new ErrorReportingOptions()));
        var serializer = A.Fake<ISerializer>();
        A.CallTo(() => serializer.Deserialize(A<TransportMessage>._)).ReturnsLazily(() => new Message(
            new Dictionary<string, string> { [Headers.MessageId] = Guid.NewGuid().ToString() }, new TestCommand()));
        var handler = new ReportFailedMessageErrorHandler(A.Fake<IErrorHandler>(), serializer, new QueuedFailedMessageReporter(queue));
        foreach (var time in new[] { DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow })
        {
            var original = ExceptionInfo.FromException(_fromFirstCaller());
            var info = new ExceptionInfo(original.Type, original.Message, original.Details, time);
            var transaction = A.Fake<ITransactionContext>();
            Func<ITransactionContext, Task>? acknowledge = null;
            A.CallTo(() => transaction.OnAck(A<Func<ITransactionContext, Task>>._))
                .Invokes((Func<ITransactionContext, Task> callback) => acknowledge = callback);
            await handler.HandlePoisonMessage(new TransportMessage(new Dictionary<string, string>(), []), transaction, info);
            await acknowledge.ShouldNotBeNull()(transaction);
        }
        var reports = new List<ErrorReport>();
        using var delivery = new ErrorReportDeliveryBackgroundService(queue, new CollectingReporter(reports), ErrorReportTestServices.CreateGate());
        await delivery.StartAsync(CancellationToken.None);
        await delivery.StopAsync(CancellationToken.None);

        reports.ShouldHaveSingleItem();
    }

    [Test]
    public void it_ignores_source_paths_and_line_numbers_in_failed_job_stack_traces()
    {
        const string first = "System.InvalidOperationException: Failure\n   at Example.Worker.Run() in /app/Worker.cs:line 12\n   at Example.Program.Main() in /app/Program.cs:line 20";
        const string second = "System.InvalidOperationException: Failure\n   at Example.Worker.Run() in C:\\build\\Worker.cs:line 99\n   at Example.Program.Main() in C:\\build\\Program.cs:line 150";

        ErrorReportFingerprint.ForFailedMessage(typeof(TestCommand), first)
            .ShouldBe(ErrorReportFingerprint.ForFailedMessage(typeof(TestCommand), second));
    }

    private sealed record TestCommand : Command;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Exception _fromFirstCaller() => _throwFailure();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Exception _fromSecondCaller() => _throwFailure();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Exception _throwFailure()
    {
        try { throw new InvalidOperationException("Failure"); }
        catch (Exception exception) { return exception; }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Exception _fromAnotherThrowSite()
    {
        try { throw new InvalidOperationException("Failure"); }
        catch (Exception exception) { return exception; }
    }

    private sealed class CollectingReporter(List<ErrorReport> reports) : IErrorReporter
    {
        public Task Report(ErrorReport report, CancellationToken cancellationToken = default)
        {
            reports.Add(report);
            return Task.CompletedTask;
        }
    }
}

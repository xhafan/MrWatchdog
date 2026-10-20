using CoreBackend.Infrastructure.ErrorReporting;
using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Rebus.ErrorHandlers;
using CoreBackend.Messages;
using FakeItEasy;
using Options = Microsoft.Extensions.Options.Options;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Messages;
using Rebus.Logging;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Retry;
using Rebus.Retry.Simple;
using Rebus.Serialization;
using Rebus.Serialization.Json;
using Rebus.Transport;
using Rebus.Transport.InMem;
using Serilog;
using Serilog.Events;

namespace CoreBackend.Tests.Infrastructure.ErrorReporting;

[TestFixture]
public class when_reporting_rebus_failures
{
    [TestCase(2, 0)]
    [TestCase(3, 1)]
    public async Task it_reports_only_after_retries_are_exhausted(int failures, int expectedReports)
    {
        var queue = new ErrorReportQueue(Options.Create(new ErrorReportingOptions()));
        using var logger = new LoggerConfiguration().Enrich.FromLogContext()
            .WriteTo.Sink(new ErrorReportingSink(queue)).CreateLogger();
        using var activator = new BuiltinHandlerActivator();
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var attempts = 0;
        activator.Handle<TestCommand>(_ =>
        {
            if (Interlocked.Increment(ref attempts) <= failures)
            {
                logger.Error("First attempt error log");
                logger.Error("Second attempt error log");
                throw new InvalidOperationException("Command handler failed");
            }
            completed.TrySetResult();
            return Task.CompletedTask;
        });
        var failedReporter = new SignalingFailedReporter(new QueuedFailedMessageReporter(queue), completed);
        var network = new InMemNetwork();
        using var bus = Configure.With(activator)
            .Logging(logging => logging.Use(new ForwardingRebusLoggerFactory(logger)))
            .Transport(transport => transport.UseInMemoryTransport(network, "commands"))
            .Serialization(serialization => serialization.UseSystemTextJson())
            .Options(options =>
            {
                options.SetNumberOfWorkers(1);
                options.SetMaxParallelism(1);
                options.RetryStrategy(errorQueueName: "errors", maxDeliveryAttempts: 3);
                options.Decorate<IPipeline>(context => new PipelineStepInjector(context.Get<IPipeline>())
                    .OnReceive(new SuppressErrorReportingDuringMessageHandlingStep(), PipelineRelativePosition.Before,
                        typeof(DeserializeIncomingMessageStep)));
                options.Decorate<IErrorHandler>(context => new ReportFailedMessageErrorHandler(
                    new LoggingErrorHandler(context.Get<IErrorHandler>(), logger),
                    context.Get<ISerializer>(), failedReporter));
            }).Start();

        await bus.SendLocal(new TestCommand { RequestId = "request-123" });
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(10));
        bus.Dispose();
        var reporter = new RecordingErrorReporter();
        using var delivery = new ErrorReportDeliveryBackgroundService(queue, reporter);
        await delivery.StartAsync(CancellationToken.None);
        await delivery.StopAsync(CancellationToken.None);

        attempts.ShouldBe(3);
        reporter.Reports.Count.ShouldBe(expectedReports);
        if (expectedReports == 1)
        {
            var report = reporter.Reports.Single();
            report.RequestId.ShouldBe("request-123");
            report.Message.ShouldContain(nameof(TestCommand));
            report.Message.ShouldContain("Command handler failed");
            report.Origin.ShouldBe(ErrorReportOrigin.FailedJob);
            report.FailedMessage.ShouldNotBeNull().FailedMessageType.ShouldBe(typeof(TestCommand));
            network.Count("errors").ShouldBe(1);
        }
    }

    [Test]
    public async Task it_does_not_report_a_terminal_failure_if_forwarding_to_the_error_queue_fails()
    {
        var underlying = A.Fake<IErrorHandler>();
        var exception = new InvalidOperationException("Error queue unavailable");
        A.CallTo(() => underlying.HandlePoisonMessage(A<TransportMessage>._, A<ITransactionContext>._, A<ExceptionInfo>._))
            .ThrowsAsync(exception);
        var reporter = A.Fake<IFailedMessageReporter>();
        var handler = new ReportFailedMessageErrorHandler(underlying, A.Fake<ISerializer>(), reporter);
        var transaction = A.Fake<ITransactionContext>();

        await Should.ThrowAsync<InvalidOperationException>(() => handler.HandlePoisonMessage(
            new TransportMessage(new Dictionary<string, string>(), []), transaction, ExceptionInfo.FromException(exception)));

        A.CallTo(() => reporter.Report(A<FailedMessageReport>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task it_reports_after_acknowledgement_and_does_not_retry_when_reporting_fails()
    {
        var serializer = A.Fake<ISerializer>();
        var messageId = Guid.NewGuid();
        A.CallTo(() => serializer.Deserialize(A<TransportMessage>._)).Returns(new Message(
            new Dictionary<string, string> { [Headers.MessageId] = messageId.ToString() },
            new TestCommand { RequestId = "request-123" }));
        var reporter = A.Fake<IFailedMessageReporter>();
        A.CallTo(() => reporter.Report(A<FailedMessageReport>._)).ThrowsAsync(new HttpRequestException("Slack unavailable"));
        var transaction = A.Fake<ITransactionContext>();
        Func<ITransactionContext, Task>? onAck = null;
        A.CallTo(() => transaction.OnAck(A<Func<ITransactionContext, Task>>._))
            .Invokes((Func<ITransactionContext, Task> callback) => onAck = callback);
        var handler = new ReportFailedMessageErrorHandler(A.Fake<IErrorHandler>(), serializer, reporter);

        await handler.HandlePoisonMessage(new TransportMessage(new Dictionary<string, string>(), []),
            transaction, ExceptionInfo.FromException(new InvalidOperationException("handler failed")));

        A.CallTo(() => serializer.Deserialize(A<TransportMessage>._)).MustNotHaveHappened();
        A.CallTo(() => reporter.Report(A<FailedMessageReport>._)).MustNotHaveHappened();
        await onAck.ShouldNotBeNull()(transaction);
        A.CallTo(() => serializer.Deserialize(A<TransportMessage>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => reporter.Report(A<FailedMessageReport>.That.Matches(report =>
            report.JobGuid == messageId && report.RequestId == "request-123"
            && report.ErrorDetails.Contains("handler failed", StringComparison.Ordinal))))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task it_isolates_deserialization_failures_after_acknowledgement()
    {
        var serializer = A.Fake<ISerializer>();
        A.CallTo(() => serializer.Deserialize(A<TransportMessage>._))
            .ThrowsAsync(new InvalidOperationException("Malformed message"));
        var reporter = A.Fake<IFailedMessageReporter>();
        var transaction = A.Fake<ITransactionContext>();
        Func<ITransactionContext, Task>? onAck = null;
        A.CallTo(() => transaction.OnAck(A<Func<ITransactionContext, Task>>._))
            .Invokes((Func<ITransactionContext, Task> callback) => onAck = callback);
        var handler = new ReportFailedMessageErrorHandler(A.Fake<IErrorHandler>(), serializer, reporter);

        await handler.HandlePoisonMessage(new TransportMessage(new Dictionary<string, string>(), []),
            transaction, ExceptionInfo.FromException(new InvalidOperationException("handler failed")));

        A.CallTo(() => serializer.Deserialize(A<TransportMessage>._)).MustNotHaveHappened();
        await onAck.ShouldNotBeNull()(transaction);
        A.CallTo(() => serializer.Deserialize(A<TransportMessage>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => reporter.Report(A<FailedMessageReport>._)).MustNotHaveHappened();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task it_only_suppresses_failed_emails_that_are_error_notifications(bool isErrorNotification)
    {
        var serializer = A.Fake<ISerializer>();
        A.CallTo(() => serializer.Deserialize(A<TransportMessage>._)).Returns(new Message(
            new Dictionary<string, string> { [Headers.MessageId] = Guid.NewGuid().ToString() },
            new SendEmailCommand("user@example.test", "Email", "Body", IsErrorNotification: isErrorNotification)));
        var reporter = A.Fake<IFailedMessageReporter>();
        var original = A.Fake<IErrorHandler>();
        var transaction = A.Fake<ITransactionContext>();
        Func<ITransactionContext, Task>? onAck = null;
        A.CallTo(() => transaction.OnAck(A<Func<ITransactionContext, Task>>._))
            .Invokes((Func<ITransactionContext, Task> callback) => onAck = callback);

        await new ReportFailedMessageErrorHandler(original, serializer, reporter).HandlePoisonMessage(
            new TransportMessage(new Dictionary<string, string>(), []), transaction,
            ExceptionInfo.FromException(new InvalidOperationException("Email delivery failed")));
        await onAck.ShouldNotBeNull()(transaction);

        A.CallTo(() => original.HandlePoisonMessage(A<TransportMessage>._, transaction, A<ExceptionInfo>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => reporter.Report(A<FailedMessageReport>._))
            .MustHaveHappened(isErrorNotification ? 0 : 1, Times.Exactly);
    }

    public sealed record TestCommand : Command;

    private sealed class LoggingErrorHandler(IErrorHandler inner, Serilog.ILogger logger) : IErrorHandler
    {
        public Task HandlePoisonMessage(TransportMessage message, ITransactionContext transaction, ExceptionInfo exception)
        {
            logger.Error("Moving a failed message to the error queue");
            return inner.HandlePoisonMessage(message, transaction, exception);
        }
    }

    private sealed class SignalingFailedReporter(IFailedMessageReporter inner, TaskCompletionSource completed) : IFailedMessageReporter
    {
        public async Task Report(FailedMessageReport report)
        {
            await inner.Report(report);
            completed.TrySetResult();
        }
    }

    private sealed class RecordingErrorReporter : IErrorReporter
    {
        public List<ErrorReport> Reports { get; } = [];
        public Task Report(ErrorReport report, CancellationToken cancellationToken = default)
        {
            Reports.Add(report);
            return Task.CompletedTask;
        }
    }

    private sealed class ForwardingRebusLoggerFactory(Serilog.ILogger logger) : IRebusLoggerFactory
    {
        public ILog GetLogger<T>() => new ForwardingRebusLogger(logger.ForContext("SourceContext", typeof(T).FullName));
    }

    private sealed class ForwardingRebusLogger(Serilog.ILogger logger) : ILog
    {
        public void Debug(string message, params object[] values) => _write(LogEventLevel.Debug, null, message, values);
        public void Info(string message, params object[] values) => _write(LogEventLevel.Information, null, message, values);
        public void Warn(string message, params object[] values) => _write(LogEventLevel.Warning, null, message, values);
        public void Warn(Exception exception, string message, params object[] values) => _write(LogEventLevel.Warning, exception, message, values);
        public void Error(string message, params object[] values) => _write(LogEventLevel.Error, null, message, values);
        public void Error(Exception exception, string message, params object[] values) => _write(LogEventLevel.Error, exception, message, values);
        private void _write(LogEventLevel level, Exception? exception, string message, object[] values) =>
            logger.Write(level, exception, message, values);
    }
}

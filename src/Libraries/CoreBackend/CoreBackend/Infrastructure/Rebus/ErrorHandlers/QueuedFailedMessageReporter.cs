using CoreBackend.Infrastructure.ErrorReporting;

namespace CoreBackend.Infrastructure.Rebus.ErrorHandlers;

public sealed class QueuedFailedMessageReporter(ErrorReportQueue queue) : IFailedMessageReporter
{
    public Task Report(FailedMessageReport report)
    {
        queue.TryEnqueue(new ErrorReport(
            $"""
             Message handling failed after retries were exhausted.
             MessageId: {report.JobGuid}
             MessageType: {report.FailedMessageType.FullName}
             {report.ErrorDetails}
             """,
            report.RequestId ?? string.Empty,
            report.OccurredAtUtc,
            Origin: ErrorReportOrigin.FailedJob,
            FailedMessage: report,
            Fingerprint: report.Fingerprint ?? ErrorReportFingerprint.ForFailedMessage(report.FailedMessageType, report.ErrorDetails)
        ));
        return Task.CompletedTask;
    }
}

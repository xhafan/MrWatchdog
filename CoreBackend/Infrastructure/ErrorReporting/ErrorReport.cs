using CoreBackend.Infrastructure.Rebus.ErrorHandlers;

namespace CoreBackend.Infrastructure.ErrorReporting;

public sealed record ErrorReport(
    string Message,
    string RequestId,
    DateTime OccurredAtUtc,
    ErrorReportOrigin Origin,
    FailedMessageReport? FailedMessage = null
);

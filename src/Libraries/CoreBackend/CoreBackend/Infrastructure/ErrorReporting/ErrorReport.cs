namespace CoreBackend.Infrastructure.ErrorReporting;

public sealed record ErrorReport(
    string Message,
    string RequestId,
    DateTime OccurredAtUtc
);

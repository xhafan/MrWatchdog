namespace CoreBackend.Infrastructure.Rebus.ErrorHandlers;

public sealed record FailedMessageReport(
    Guid JobGuid,
    Type FailedMessageType,
    string? RequestId,
    string ErrorDetails,
    DateTime OccurredAtUtc
);

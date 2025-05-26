namespace MrWatchdog.Core.Features.Jobs.Queries;

public record JobHandlingAttemptDto(
    DateTime StartedOn,
    DateTime? EndedOn,
    string? Exception
);
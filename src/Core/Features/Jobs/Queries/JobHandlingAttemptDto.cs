using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Jobs.Queries;

[TsInterface]
public record JobHandlingAttemptDto(
    DateTime StartedOn,
    DateTime? EndedOn,
    string? Exception
);
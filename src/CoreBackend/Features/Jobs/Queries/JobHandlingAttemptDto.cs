using Reinforced.Typings.Attributes;

namespace CoreBackend.Features.Jobs.Queries;

[TsInterface]
public record JobHandlingAttemptDto(
    DateTime StartedOn,
    DateTime? EndedOn,
    string? Exception
);
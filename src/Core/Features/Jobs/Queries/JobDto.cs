using MrWatchdog.Core.Features.Jobs.Domain;

namespace MrWatchdog.Core.Features.Jobs.Queries;

public record JobDto(
    Guid Guid,
    DateTime CreatedOn,
    DateTime? CompletedOn,
    string Type,
    string InputData,
    JobKind Kind,
    int NumberOfHandlingAttempts,
    IEnumerable<JobAggregateRootEntityDto> AffectedAggregateRootEntities,
    IEnumerable<JobHandlingAttemptDto> HandlingAttempts
);
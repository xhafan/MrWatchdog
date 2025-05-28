using MrWatchdog.Core.Features.Jobs.Domain;
using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Jobs.Queries;

[TsInterface]
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
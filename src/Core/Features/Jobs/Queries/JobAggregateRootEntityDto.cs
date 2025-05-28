using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Jobs.Queries;

[TsInterface]
public record JobAggregateRootEntityDto(string AggregateRootEntityName, long AggregateRootEntityId);
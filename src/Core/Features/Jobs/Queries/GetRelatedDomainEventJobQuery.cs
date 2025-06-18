using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Jobs.Queries;

public record GetRelatedDomainEventJobQuery(Guid CommandJobGuid, string Type) : IQuery;
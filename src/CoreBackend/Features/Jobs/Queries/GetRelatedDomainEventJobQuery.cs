using CoreDdd.Queries;

namespace CoreBackend.Features.Jobs.Queries;

public record GetRelatedDomainEventJobQuery(Guid CommandJobGuid, string Type) : IQuery<JobDto>;
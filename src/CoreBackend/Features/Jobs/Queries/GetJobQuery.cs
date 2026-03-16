using CoreDdd.Queries;

namespace CoreBackend.Features.Jobs.Queries;

public record GetJobQuery(Guid JobGuid) : IQuery<JobDto>;
using IQuery = CoreDdd.Queries.IQuery;

namespace MrWatchdog.Core.Features.Jobs.Queries;

public record GetJobQuery(Guid JobGuid) : IQuery;
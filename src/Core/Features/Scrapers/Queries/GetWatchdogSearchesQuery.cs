using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogSearchesQuery(long UserId) : IQuery;
using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetUserWatchdogsQuery(long UserId) : IQuery;
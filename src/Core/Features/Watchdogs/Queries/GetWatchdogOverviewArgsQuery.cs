using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogOverviewArgsQuery(long WatchdogId) : IQuery<WatchdogOverviewArgs>;
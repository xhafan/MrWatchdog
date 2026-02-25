using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogScraperArgsQuery(long WatchdogId) : IQuery<WatchdogScraperDto>;
using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogWebPageDisabledWarningQuery(long WatchdogId, long WatchdogWebPageId) : IQuery;
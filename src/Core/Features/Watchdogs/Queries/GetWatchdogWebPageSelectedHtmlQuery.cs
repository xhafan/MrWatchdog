using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogWebPageSelectedHtmlQuery(long WatchdogId, long WatchdogWebPageId) : IQuery;
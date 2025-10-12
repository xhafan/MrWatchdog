﻿using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogSearchScrapingResultsUpdated;

public record WatchdogSearchScrapingResultsUpdatedDomainEvent(long WatchdogSearchId) : DomainEvent;
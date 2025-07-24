using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events;
using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Shared.ReinforcedTypings;

[TsClass(IncludeNamespace = false)]
public static class DomainConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogEntityName = nameof(Watchdog);
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogWebPageEntityName = nameof(WatchdogWebPage);
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogWebPageScrapingDataUpdatedDomainEventName = nameof(WatchdogWebPageScrapingDataUpdatedDomainEvent);
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogAlertEntityName = nameof(WatchdogAlert);
}
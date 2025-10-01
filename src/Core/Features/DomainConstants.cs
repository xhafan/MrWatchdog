using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingDataUpdated;
using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features;

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
    

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginTokenEntityName = nameof(LoginToken);
    
}
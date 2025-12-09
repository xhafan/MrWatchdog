using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;
using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features;

[TsClass(IncludeNamespace = false)]
public static class DomainConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperEntityName = nameof(Scraper);
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperWebPageEntityName = nameof(ScraperWebPage);
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperWebPageScrapingDataUpdatedDomainEventName = nameof(ScraperWebPageScrapingDataUpdatedDomainEvent);
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogSearchEntityName = nameof(WatchdogSearch);
    

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginTokenEntityName = nameof(LoginToken);
    
}
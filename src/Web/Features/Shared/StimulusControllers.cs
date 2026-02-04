using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Shared;

[TsClass(IncludeNamespace = false)]
public static class StimulusControllers
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string Body = "body";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string TurboFrame = "turbo-frame";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ViewOrEditForm = "view-or-edit-form";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersCreate = "scrapers--create";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersDetail = "scrapers--detail";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersDetailActions = "scrapers--detail-actions";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersDetailWebPage = "scrapers--detail-web-page";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersDetailWebPageOverview = "scrapers--detail-web-page-overview";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersDetailWebPageDisabledWarning = "scrapers--detail-web-page-disabled-warning";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersDetailWebPageScrapedResults = "scrapers--detail-web-page-scraped-results";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersScrapedResults = "scrapers--scraped-results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersScrapedResultsWebPages = "scrapers--scraped-results-web-pages";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetail = "watchdogs--detail";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailOverview = "watchdogs--detail-overview";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLogin = "account--login";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginLinkSent = "account--login-link-sent";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string Onboarding = "onboarding";
}
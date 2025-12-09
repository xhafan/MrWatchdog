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
    public const string ScrapersDetailWebPageScrapingResults = "scrapers--detail-web-page-scraping-results";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersScrapingResults = "scrapers--scraping-results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersScrapingResultsWebPages = "scrapers--scraping-results-web-pages";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogSearch = "watchdogs--search";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogSearchOverview = "watchdogs--search-overview";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLogin = "account--login";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginLinkSent = "account--login-link-sent";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string Onboarding = "onboarding";
}
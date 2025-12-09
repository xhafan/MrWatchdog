using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Resources;
using MrWatchdog.Web.Features.Shared;
using MrWatchdog.Web.Features.Shared.TagHelpers;

namespace MrWatchdog.Web.Features.Scrapers.Shared.ScrapingResultsWebPages;

[HtmlTargetElement("scraping-results-web-pages")]
public class ScrapingResultsWebPages(IHtmlHelper htmlHelper) : BaseStimulusModelViewTagHelper<ScrapingResultsWebPagesStimulusModel>(htmlHelper)
{
    public ScraperScrapingResultsArgs ScraperScrapingResultsArgs { get; set; } = null!;
    
    protected override string GetStimulusControllerName()
    {
        return StimulusControllers.ScrapersScrapingResultsWebPages;
    }

    protected override Task<ScrapingResultsWebPagesStimulusModel> GetStimulusModel()
    {
        return Task.FromResult(
            new ScrapingResultsWebPagesStimulusModel(
                Resource.NoScrapedResultsAvailable,
                Resource.NoScrapedResultsMatchingTheSearchTermAvailable
            )
        );
    }
}
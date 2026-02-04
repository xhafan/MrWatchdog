using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Resources;
using MrWatchdog.Web.Features.Shared;
using MrWatchdog.Web.Features.Shared.TagHelpers;

namespace MrWatchdog.Web.Features.Scrapers.Shared.ScrapedResultsWebPages;

[HtmlTargetElement("scraped-results-web-pages")]
public class ScrapedResultsWebPages(IHtmlHelper htmlHelper) : BaseStimulusModelViewTagHelper<ScrapedResultsWebPagesStimulusModel>(htmlHelper)
{
    public ScraperScrapedResultsArgs ScraperScrapedResultsArgs { get; set; } = null!;
    
    protected override string GetStimulusControllerName()
    {
        return StimulusControllers.ScrapersScrapedResultsWebPages;
    }

    protected override Task<ScrapedResultsWebPagesStimulusModel> GetStimulusModel()
    {
        return Task.FromResult(
            new ScrapedResultsWebPagesStimulusModel(
                Resource.NoScrapedResultsAvailable,
                Resource.NoScrapedResultsMatchingTheSearchTermAvailable
            )
        );
    }
}
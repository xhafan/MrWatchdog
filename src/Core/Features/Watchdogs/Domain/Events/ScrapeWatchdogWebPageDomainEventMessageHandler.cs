using CoreUtils;
using CoreUtils.Extensions;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events;

public class ScrapeWatchdogWebPageDomainEventMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    IHttpClientFactory httpClientFactory
) 
    : IHandleMessages<WatchdogWebPageScrapingDataUpdatedDomainEvent>
{
    public async Task Handle(WatchdogWebPageScrapingDataUpdatedDomainEvent domainEvent)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(domainEvent.WatchdogId);
        var watchdogWebPage = watchdog.GetWatchdogWebPageArgs(domainEvent.WatchdogWebPageId);

        var httpClient = httpClientFactory.CreateClient();
        
        string? responseContent = null;
        string? scrapingErrorMessage = null;
        
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, watchdogWebPage.Url);
            var response = await httpClient.SendAsync(request);
            responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                scrapingErrorMessage =
                    $"Error scraping web page, HTTP status code: {(int) response.StatusCode} {response.ReasonPhrase}{(
                        !string.IsNullOrWhiteSpace(responseContent)
                            ? $": {responseContent}"
                            : ""
                    )}";

            }
        }
        catch (Exception ex)
        {
            scrapingErrorMessage = ex.Message;
        }

        if (scrapingErrorMessage != null)
        {
            watchdog.SetScrapingErrorMessage(domainEvent.WatchdogWebPageId, scrapingErrorMessage);
            return;
        }
        
        Guard.Hope(responseContent != null, nameof(responseContent) + " is null");

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(responseContent);
        
        var nodes = htmlDoc.DocumentNode.QuerySelectorAll(watchdogWebPage.Selector).ToList();
        if (nodes.IsEmpty())
        {
            scrapingErrorMessage = "No HTML node selected. Please review the Selector.";
        } 

        if (scrapingErrorMessage != null)
        {
            watchdog.SetScrapingErrorMessage(domainEvent.WatchdogWebPageId, scrapingErrorMessage);
            return;
        }        
        
        watchdog.SetScrapingResults(domainEvent.WatchdogWebPageId, nodes.Select(x => x.OuterHtml).ToList());
    }
}
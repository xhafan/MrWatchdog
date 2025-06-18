using CoreUtils;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events;

public class WatchdogWebPageUpdatedDomainEventMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    IHttpClientFactory httpClientFactory
) 
    : IHandleMessages<WatchdogWebPageUpdatedDomainEvent>
{
    public async Task Handle(WatchdogWebPageUpdatedDomainEvent domainEvent)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(domainEvent.WatchdogId);
        var watchdogWebPage = watchdog.GetWatchdogWebPageArgs(domainEvent.WatchdogWebPageId);

        var httpClient = httpClientFactory.CreateClient(); 

        using var request = new HttpRequestMessage(HttpMethod.Get, watchdogWebPage.Url);
        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        Guard.Hope(
            response.IsSuccessStatusCode,
            $"Error scraping web page {watchdogWebPage.Url}, HTTP status code: {(int)response.StatusCode} {response.ReasonPhrase}{(
                !string.IsNullOrWhiteSpace(responseContent)
                    ? $": {responseContent}"
                    : ""
            )}"
        );

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(responseContent);
        
        var nodes = htmlDoc.DocumentNode.QuerySelectorAll(watchdogWebPage.Selector).ToList();
        Guard.Hope(nodes.Count == 1, $"Error selecting HTML, URL: {watchdogWebPage.Url}, selector: {watchdogWebPage.Selector}, HTML nodes found: {nodes.Count}");

        watchdog.SetSelectedHtml(domainEvent.WatchdogWebPageId, nodes.Single().OuterHtml);
    }
}
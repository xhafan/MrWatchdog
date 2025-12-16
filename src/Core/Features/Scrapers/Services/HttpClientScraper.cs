using System.Net;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Extensions;
using MrWatchdog.Core.Infrastructure.HttpClients;

namespace MrWatchdog.Core.Features.Scrapers.Services;

public class HttpClientScraper(IHttpClientFactory httpClientFactory) : IWebScraper
{
    public int Priority => 10;

    public async Task<ScrapeResult> Scrape(string url, ICollection<(string Name, string Value)>? httpHeaders)
    {
        var httpClient = httpClientFactory.CreateClient(HttpClientConstants.HttpClientWithRetries);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (httpHeaders != null)
            {
                foreach (var httpHeader in httpHeaders)
                {
                    request.Headers.TryAddWithoutValidation(httpHeader.Name, httpHeader.Value);
                }
            }

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                return ScrapeResult.Failed(
                    $"Error scraping web page, HTTP status code: {(int) response.StatusCode} {response.ReasonPhrase}",
                    stopWebScraperChain: response.StatusCode == HttpStatusCode.NotFound
                );
            }

            var responseContent = await response.Content.ReadAsStringWithLimitAsync(
                ScrapingConstants.WebPageSizeLimitInMegaBytes,
                $"Web page larger than {ScrapingConstants.WebPageSizeLimitInMegaBytes} MB."
            );

            return ScrapeResult.Succeeded(responseContent);
        }
        catch (HttpResponseTooLargeException ex)
        {
            return ScrapeResult.Failed(ex.Message, stopWebScraperChain: true);
        }
        catch (Exception ex)
        {
            return ScrapeResult.Failed(ex.Message);
        }
    }
}
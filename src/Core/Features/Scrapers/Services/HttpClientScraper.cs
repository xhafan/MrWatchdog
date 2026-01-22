using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Extensions;
using MrWatchdog.Core.Infrastructure.HttpClients;

namespace MrWatchdog.Core.Features.Scrapers.Services;

public class HttpClientScraper(
    IHttpClientFactory httpClientFactory,
    ILogger<HttpClientScraper>? logger = null
) : IWebScraper
{
    public int Priority => 10;
    public bool IsBrowserRenderedHtmlScrapingSupported => false;

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
                var httpStatusCode = (int) response.StatusCode;
                return ScrapeResult.Failed(
                    $"Error scraping web page, HTTP status code: {httpStatusCode} {response.ReasonPhrase}",
                    httpStatusCode: httpStatusCode
                );
            }

            var responseContent = await response.Content.ReadAsStringWithLimitAsync(
                ScrapingConstants.WebPageSizeLimitInMegaBytes,
                $"Web page {url} larger than {ScrapingConstants.WebPageSizeLimitInMegaBytes} MB."
            );

            return ScrapeResult.Succeeded(
                responseContent,
                httpStatusCode: (int) response.StatusCode
            );
        }
        catch (HttpResponseTooLargeException ex)
        {
            return ScrapeResult.Failed(ex.Message, stopWebScraperChain: true);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, ex.Message);
            return ScrapeResult.Failed(ex.Message, stopWebScraperChain: ex.Message.Contains("No such host is known") // Windows
                                                                        || ex.Message.Contains("Name does not resolve") // Linux
            );
        }
    }
}
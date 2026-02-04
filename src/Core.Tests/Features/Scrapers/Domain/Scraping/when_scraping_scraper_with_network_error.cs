using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingFailed;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;
using System.Net;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Scraping;

[TestFixture]
public class when_scraping_scraper_with_network_error : BaseTest
{
    private Scraper _scraper = null!;
    private HttpClientFactory _httpClientFactory = null!;
    private WebScraperChain _webScraperChain = null!;

    [SetUp]
    public async Task Context()
    {
        await _BuildEntities();

        _httpClientFactory = new HttpClientFactoryBuilder()
            .WithRequestResponse(new HttpMessageRequestResponse(
                "https://www.pcgamer.com/epic-games-store-free-games-list/",
                () => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                }))
            .Build();
        _webScraperChain = new WebScraperChain([new HttpClientScraper(_httpClientFactory)]);

        await _scraper.Scrape(_webScraperChain);
    }

    [Test]
    public void scraper_web_page_scraping_error_message_is_set_after_first_failed_scraping()
    {
        var webPage = _scraper.WebPages.Single();

        webPage.ScrapingErrorMessage.ShouldBe(
            """
            Scraping failed:
            HttpClientScraper: Error scraping web page, HTTP status code: 404 Not Found
            """,
            ignoreLineEndings: true
        );

        webPage.ScrapedResults.ShouldBeEmpty();
        webPage.ScrapedOn.ShouldBeNull();
    }
    
    [Test]
    public void scraper_web_page_scraping_failed_domain_event_is_not_raised_after_the_first_failed_scraping_attempt()
    {
        RaisedDomainEvents.ShouldNotContain(new ScraperWebPageScrapingFailedDomainEvent(_scraper.Id));
    }  

    [Test]
    public void scraper_can_notify_about_failed_scraping_after_the_first_scraping_attempt()
    {
        _scraper.CanNotifyAboutFailedScraping.ShouldBe(true);
    }

    [Test]
    public async Task scraper_web_page_scraping_failed_domain_event_is_raised_after_the_second_failed_scraping_attempt()
    {
        await _scraper.Scrape(_webScraperChain);

        RaisedDomainEvents.ShouldContain(new ScraperWebPageScrapingFailedDomainEvent(_scraper.Id));
    }

    [Test]
    public async Task number_of_failed_scraping_attempts_before_the_next_alert_is_reset_after_the_second_failed_scraping_attempt()
    {
        await _scraper.Scrape(_webScraperChain);

        _scraper.WebPages.Single().NumberOfFailedScrapingAttemptsBeforeTheNextAlert.ShouldBe(0);
    }
    
    [Test]
    public async Task scraper_cannot_notify_about_failed_scraping_after_the_second_failed_scraping_attempt()
    {
        await _scraper.Scrape(_webScraperChain);

        _scraper.CanNotifyAboutFailedScraping.ShouldBe(false);
    }

    [Test]
    public async Task scraping_for_the_third_time_does_not_raise_scraping_failed_domain_event_again()
    {
        await _scraper.Scrape(_webScraperChain);
        RaisedDomainEvents.Clear();
        
        await _scraper.Scrape(_webScraperChain);
        
        RaisedDomainEvents.ShouldNotContain(new ScraperWebPageScrapingFailedDomainEvent(_scraper.Id));
    }
    
    [Test]
    public async Task number_of_failed_scraping_attempts_before_the_next_alert_is_reset_after_the_first_failed_scraping_attempt_and_subsequent_successful_scraping_attempt()
    {
        var successfulScrapingHttpClientFactory = new HttpClientFactoryBuilder()
            .WithRequestResponse(new HttpMessageRequestResponse(
                "https://www.pcgamer.com/epic-games-store-free-games-list/",
                () => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        """
                        <html>
                        <body>
                        <div id="article-body">
                        <p class="infoUpdate-log">
                        <a href="https://store.epicgames.com/en-US/p/two-point-hospital" target="_blank">Two Point Hospital</a>
                        </p>
                        </div>
                        </body>
                        </html>
                        """)
                }))
            .Build();

        await _scraper.Scrape(new WebScraperChain([new HttpClientScraper(successfulScrapingHttpClientFactory)]));

        _scraper.WebPages.Single().NumberOfFailedScrapingAttemptsBeforeTheNextAlert.ShouldBe(0);
    }

    private async Task _BuildEntities()
    {
        _scraper = new ScraperBuilder()
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "https://www.pcgamer.com/epic-games-store-free-games-list/",
                Selector = """
                           div#article-body p.infoUpdate-log a[href^="https://store.epicgames.com/"]
                           """,
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .WithNumberOfFailedScrapingAttemptsBeforeAlerting(2)
            .Build();
        
        await _successfullyScrapeScraperSoItCanRaiseFailedScrapingDomainEvent();
        return;

        async Task _successfullyScrapeScraperSoItCanRaiseFailedScrapingDomainEvent()
        {
            var httpClientFactory = new HttpClientFactoryBuilder()
                .WithRequestResponse(new HttpMessageRequestResponse(
                    "https://www.pcgamer.com/epic-games-store-free-games-list/",
                    () => new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(
                            """
                            <html>
                            <body>
                            <div id="article-body">
                            <p class="infoUpdate-log">
                            <a href="https://store.epicgames.com/en-US/p/two-point-hospital" target="_blank">Two Point Hospital</a>
                            </p>
                            </div>
                            </body>
                            </html>
                            """)
                    }))
                .Build();

            await _scraper.Scrape(new WebScraperChain([new HttpClientScraper(httpClientFactory)]));
        }
    }
}
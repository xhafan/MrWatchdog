using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingFailed;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;
using System.Net;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Scraping;

[TestFixture]
public class when_scraping_watchdog_with_network_error : BaseTest
{
    private Watchdog _watchdog = null!;
    private HttpClientFactory _httpClientFactory = null!;

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
        
        await _watchdog.Scrape(_httpClientFactory);
    }

    [Test]
    public void watchdog_web_page_scraping_error_message_is_set_after_first_failed_scraping()
    {
        var webPage = _watchdog.WebPages.Single();
        
        webPage.ScrapingErrorMessage.ShouldBe("Error scraping web page, HTTP status code: 404 Not Found");

        webPage.ScrapingResults.ShouldBeEmpty();
        webPage.ScrapedOn.ShouldBeNull();
    }
    
    [Test]
    public void watchdog_web_page_scraping_failed_domain_event_is_not_raised_after_the_first_failed_scraping_attempt()
    {
        RaisedDomainEvents.ShouldNotContain(new WatchdogWebPageScrapingFailedDomainEvent(_watchdog.Id));
    }  

    [Test]
    public void watchdog_can_notify_about_failed_scraping_after_the_first_scraping_attempt()
    {
        _watchdog.CanNotifyAboutFailedScraping.ShouldBe(true);
    }

    [Test]
    public async Task watchdog_web_page_scraping_failed_domain_event_is_raised_after_the_second_failed_scraping_attempt()
    {
        await _watchdog.Scrape(_httpClientFactory);

        RaisedDomainEvents.ShouldContain(new WatchdogWebPageScrapingFailedDomainEvent(_watchdog.Id));
    }

    [Test]
    public async Task watchdog_number_of_failed_scraping_attempts_is_reset_after_the_second_failed_scraping_attempt()
    {
        await _watchdog.Scrape(_httpClientFactory);

        _watchdog.WebPages.Single().NumberOfFailedScrapingAttemptsBeforeTheNextAlert.ShouldBe(0);
    }
    
    [Test]
    public async Task watchdog_cannot_notify_about_failed_scraping_after_the_second_failed_scraping_attempt()
    {
        await _watchdog.Scrape(_httpClientFactory);

        _watchdog.CanNotifyAboutFailedScraping.ShouldBe(false);
    }

    [Test]
    public async Task scraping_for_the_third_time_does_not_raise_scraping_failed_domain_event_again()
    {
        await _watchdog.Scrape(_httpClientFactory);
        RaisedDomainEvents.Clear();
        
        await _watchdog.Scrape(_httpClientFactory);
        
        RaisedDomainEvents.ShouldNotContain(new WatchdogWebPageScrapingFailedDomainEvent(_watchdog.Id));
    }    
    
    private async Task _BuildEntities()
    {
        _watchdog = new WatchdogBuilder()
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "https://www.pcgamer.com/epic-games-store-free-games-list/",
                Selector = """
                           div#article-body p.infoUpdate-log a[href^="https://store.epicgames.com/"]
                           """,
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .WithNumberOfFailedScrapingAttemptsBeforeAlerting(2)
            .Build();
        
        await _successfullyScrapeWatchdogSoItCanRaiseFailedScrapingDomainEvent();
        return;

        async Task _successfullyScrapeWatchdogSoItCanRaiseFailedScrapingDomainEvent()
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

            await _watchdog.Scrape(httpClientFactory);
        }
    }
}
using CoreBackend.TestsShared;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using MrWatchdog.Core.Features.Scrapers.Services;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Services;

[TestFixture]
public class when_sharing_a_playwright_browser
{
    [Test]
    public async Task concurrent_scrapes_share_a_browser_but_keep_headers_cookies_and_storage_isolated()
    {
        await using var browser = _createBrowser(2);
        var contexts = await Task.WhenAll(
            browser.NewContextAsync(new BrowserNewContextOptions { ExtraHTTPHeaders = new Dictionary<string, string> { ["X-Probe"] = "first" } }),
            browser.NewContextAsync(new BrowserNewContextOptions { ExtraHTTPHeaders = new Dictionary<string, string> { ["X-Probe"] = "second" } }));
        await using var first = contexts[0];
        await using var second = contexts[1];
        first.Context.Browser.ShouldBeSameAs(second.Context.Browser);
        await first.Context.AddCookiesAsync([new Cookie { Name = "probe", Value = "first", Url = "https://isolation.test" }]);
        var firstPage = await _openProbePage(first.Context);
        var secondPage = await _openProbePage(second.Context);
        await firstPage.EvaluateAsync("localStorage.setItem('probe', 'first')");
        await firstPage.EvaluateAsync("sessionStorage.setItem('probe', 'first')");
        (await firstPage.TextContentAsync("body")).ShouldBe("first");
        (await secondPage.TextContentAsync("body")).ShouldBe("second");
        (await second.Context.CookiesAsync()).ShouldBeEmpty();
        (await secondPage.EvaluateAsync<string?>("localStorage.getItem('probe')")).ShouldBeNull();
        (await secondPage.EvaluateAsync<string?>("sessionStorage.getItem('probe')")).ShouldBeNull();
    }

    [Test]
    public async Task a_disconnected_browser_is_replaced_for_the_next_scrape()
    {
        await using var browser = _createBrowser(2);
        await using var first = await browser.NewContextAsync();
        var crashedBrowser = first.Context.Browser!;
        await crashedBrowser.CloseAsync();
        await using var second = await browser.NewContextAsync();
        second.Context.Browser.ShouldNotBeSameAs(crashedBrowser);
        second.Context.Browser!.IsConnected.ShouldBeTrue();
        var page = await _openProbePage(second.Context);
        (await page.TitleAsync()).ShouldBe("Rendered probe");
    }

    [Test]
    public async Task queued_scrapes_can_be_cancelled_and_disposing_a_context_releases_capacity_once()
    {
        await using var browser = _createBrowser(1);
        await using var first = await browser.NewContextAsync();
        using var cancellation = new CancellationTokenSource();
        var cancelled = browser.NewContextAsync(cancellationToken: cancellation.Token);
        cancelled.IsCompleted.ShouldBeFalse();
        await cancellation.CancelAsync();
        await Should.ThrowAsync<OperationCanceledException>(() => cancelled);
        var next = browser.NewContextAsync();
        next.IsCompleted.ShouldBeFalse();
        await first.DisposeAsync();
        await first.DisposeAsync();
        await using var second = await next.WaitAsync(TimeSpan.FromSeconds(10));
        second.Context.Browser!.Contexts.Count.ShouldBe(1);
    }

    [Test]
    public async Task shutdown_rejects_queued_work_and_waits_for_active_contexts_to_close()
    {
        await using var browser = _createBrowser(1);
        await using var active = await browser.NewContextAsync();
        var chromium = active.Context.Browser!;
        var queued = browser.NewContextAsync();
        var shutdown = browser.DisposeAsync().AsTask();
        shutdown.IsCompleted.ShouldBeFalse();
        await Should.ThrowAsync<OperationCanceledException>(() => queued);
        await active.DisposeAsync();
        await shutdown.WaitAsync(TimeSpan.FromSeconds(10));
        chromium.IsConnected.ShouldBeFalse();
        await Should.ThrowAsync<ObjectDisposedException>(() => browser.NewContextAsync());
    }

    private static PlaywrightBrowser _createBrowser(int maximumConcurrentScrapes)
    {
        var options = OptionsTestRetriever.Retrieve<PlaywrightScraperOptions>().Value;
        options.MaximumConcurrentScrapes = maximumConcurrentScrapes;
        return new PlaywrightBrowser(Options.Create(options));
    }

    private static async Task<IPage> _openProbePage(IBrowserContext context)
    {
        await context.RouteAsync("https://isolation.test/**", route => route.FulfillAsync(new RouteFulfillOptions
        {
            ContentType = "text/html",
            Body = $"<html><head><title>Rendered probe</title></head><body>{route.Request.Headers.GetValueOrDefault("x-probe", "none")}</body></html>"
        }));
        var page = await context.NewPageAsync();
        await page.GotoAsync("https://isolation.test/");
        return page;
    }
}

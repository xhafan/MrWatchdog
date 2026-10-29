using CoreBackend.TestsShared;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.Core.TestsShared;

namespace MrWatchdog.Core.Tests;

[SetUpFixture]
public class RunOncePerTestRun : BaseRunOncePerTestRun
{
    public static PlaywrightBrowser Browser = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        Browser = new PlaywrightBrowser(OptionsTestRetriever.Retrieve<PlaywrightScraperOptions>());
    }
    
    [OneTimeTearDown]
    public async Task TearDown()
    {
        await Browser.DisposeAsync();
    }
}

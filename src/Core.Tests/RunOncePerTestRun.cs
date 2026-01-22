using Microsoft.Playwright;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests;

[SetUpFixture]
public class RunOncePerTestRun : BaseRunOncePerTestRun
{
    public static Lazy<Task<IPlaywright>> PlaywrightTask = null!;
    
    [OneTimeSetUp]
    public void SetUp()
    {
        PlaywrightTask = new Lazy<Task<IPlaywright>>(Playwright.CreateAsync);
    }
    
    [OneTimeTearDown]
    public void TearDown()
    {
        if (PlaywrightTask.IsValueCreated)
        {
            PlaywrightTask.Value.Result.Dispose();
        }
    }
}

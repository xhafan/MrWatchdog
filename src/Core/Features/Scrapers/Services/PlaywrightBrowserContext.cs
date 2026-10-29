using Microsoft.Playwright;

namespace MrWatchdog.Core.Features.Scrapers.Services;

public sealed class PlaywrightBrowserContext(IBrowserContext context, Action releaseCapacity) : IAsyncDisposable
{
    private int _disposed;

    public IBrowserContext Context => context;

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        try
        {
            await context.CloseAsync();
        }
        finally
        {
            releaseCapacity();
        }
    }
}

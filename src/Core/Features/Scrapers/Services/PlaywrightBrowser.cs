using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace MrWatchdog.Core.Features.Scrapers.Services;

public sealed class PlaywrightBrowser : IAsyncDisposable, IDisposable
{
    private readonly PlaywrightScraperOptions _options;
    private readonly int _maximumConcurrentScrapes;
    private readonly SemaphoreSlim _capacity;
    private readonly SemaphoreSlim _browserGate = new(1, 1);
    private readonly CancellationTokenSource _shutdown = new();
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private volatile bool _disposed;

    public PlaywrightBrowser(IOptions<PlaywrightScraperOptions> options)
    {
        _options = options.Value;
        ArgumentOutOfRangeException.ThrowIfLessThan(_options.MaximumConcurrentScrapes, 1);
        _maximumConcurrentScrapes = _options.MaximumConcurrentScrapes;
        _capacity = new SemaphoreSlim(_maximumConcurrentScrapes, _maximumConcurrentScrapes);
    }

    public async Task<PlaywrightBrowserContext> NewContextAsync(
        BrowserNewContextOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdown.Token);
        await _capacity.WaitAsync(cancellation.Token);
        try
        {
            IBrowser browser;
            await _browserGate.WaitAsync(cancellation.Token);
            try
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                _playwright ??= await Playwright.CreateAsync();
                if (_browser?.IsConnected != true)
                {
                    if (_browser != null) await _browser.CloseAsync();
                    _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        ExecutablePath = _options.BrowserExecutablePath,
                        Headless = false,
                        Args = ["--window-position=-2000,0"]
                    });
                }
                browser = _browser;
            }
            finally
            {
                _browserGate.Release();
            }

            var context = await browser.NewContextAsync(options);
            if (cancellation.IsCancellationRequested)
            {
                await context.CloseAsync();
                cancellation.Token.ThrowIfCancellationRequested();
            }
            return new PlaywrightBrowserContext(context, () => _capacity.Release());
        }
        catch
        {
            _capacity.Release();
            throw;
        }
    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync()
    {
        await _browserGate.WaitAsync();
        try
        {
            if (_disposed) return;
            _disposed = true;
            await _shutdown.CancelAsync();
        }
        finally
        {
            _browserGate.Release();
        }

        for (var i = 0; i < _maximumConcurrentScrapes; i++) await _capacity.WaitAsync();
        try
        {
            if (_browser != null) await _browser.CloseAsync();
        }
        finally
        {
            _playwright?.Dispose();
        }
    }
}

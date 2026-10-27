using Microsoft.Extensions.Options;

namespace CoreBackend.Infrastructure.ErrorReporting;

public sealed class InMemoryErrorReportGate(IOptions<ErrorReportingOptions> options, TimeProvider clock, string scope) : IErrorReportGate
{
    private readonly object _sync = new();
    private readonly Dictionary<string, DateTimeOffset> _reservations = new();
    private readonly TimeSpan _window = options.Value.SuppressionWindow > TimeSpan.Zero
        ? options.Value.SuppressionWindow
        : throw new ArgumentOutOfRangeException(nameof(options), "SuppressionWindow must be positive.");

    public Task<bool> TryReserveAsync(ErrorReport report, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = ErrorReportSuppressionKey.Create(report, scope);
        lock (_sync)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var now = clock.GetUtcNow();
            foreach (var expired in _reservations.Where(entry => entry.Value <= now).Select(entry => entry.Key).ToArray())
                _reservations.Remove(expired);
            if (_reservations.ContainsKey(key)) return Task.FromResult(false);
            _reservations[key] = now.Add(_window);
            return Task.FromResult(true);
        }
    }
}

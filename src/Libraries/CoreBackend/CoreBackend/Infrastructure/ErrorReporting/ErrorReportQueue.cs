using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace CoreBackend.Infrastructure.ErrorReporting;

public sealed class ErrorReportQueue
{
    private readonly Channel<ErrorReport> _channel;
    internal ErrorReportingOptions Options { get; }
    internal ChannelReader<ErrorReport> Reader => _channel.Reader;

    public ErrorReportQueue(IOptions<ErrorReportingOptions> options)
    {
        Options = options.Value;
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Options.QueueCapacity);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(Options.DeliveryTimeout, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(Options.ShutdownTimeout, TimeSpan.Zero);
        _channel = Channel.CreateBounded<ErrorReport>(new BoundedChannelOptions(Options.QueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            AllowSynchronousContinuations = false
        });
    }

    public bool TryEnqueue(ErrorReport report)
    {
        if (!Options.Enabled) return false;
        if (_channel.Writer.TryWrite(report)) return true;

        ErrorReportingDiagnostics.QueueUnavailable();
        return false;
    }

    internal void Complete() => _channel.Writer.TryComplete();
}

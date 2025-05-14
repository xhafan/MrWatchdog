using Microsoft.Extensions.Logging;

namespace MrWatchdog.TestsShared.Loggers;

public class TestLogger : ILogger, IDisposable
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => this;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        Console.WriteLine($"[{logLevel}] {message}");

        if (exception != null)
        {
            Console.WriteLine(exception);
        }
    }

    public void Dispose()
    {
    }
}
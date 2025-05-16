using Microsoft.Extensions.Logging;

namespace MrWatchdog.TestsShared.Loggers;

public class ConsoleTestLogger : ILogger, IDisposable
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
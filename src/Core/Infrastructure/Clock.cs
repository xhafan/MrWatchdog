using CoreUtils.AmbientStorages;

namespace MrWatchdog.Core.Infrastructure;

// Use Clock.UtcNow instead of DateTime.UtcNow in the application code when a test needs to simulate a historical date time.
// Due to the use of AmbientStorage, multiple tests can use it when executed concurrently.
public static class Clock
{
    public static readonly AmbientStorage<Func<DateTime>?> CurrentDateTimeProvider = new();
    
    public static DateTime UtcNow => CurrentDateTimeProvider.Value?.Invoke() ?? DateTime.UtcNow;
}
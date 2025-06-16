namespace MrWatchdog.TestsShared;

public static class Incrementer
{
    private static long _lastUsedValue;

    public static long GetNextIncrement()
    {
        return Interlocked.Increment(ref _lastUsedValue);
    }
}
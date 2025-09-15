namespace MrWatchdog.Core.Infrastructure;

public static class HttpClientConstants
{
    public const string HttpClientWithRetries = nameof(HttpClientWithRetries);
    public const int RetryCount = 4;
    public static readonly Func<int, int> RetrySleepDurationInSecondsProvider = retryAttempt => (int)Math.Pow(2, retryAttempt);
}
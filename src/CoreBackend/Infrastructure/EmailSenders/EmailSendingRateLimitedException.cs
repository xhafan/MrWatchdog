namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class EmailSendingRateLimitedException(string message, Exception innerException) : Exception(message, innerException);
namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class EmailSendingNotAllowedInCoolDownPeriodException(string message) : Exception(message);
namespace CoreBackend.Infrastructure.EmailSenders;

public class EmailSendingNotAllowedInCoolDownPeriodException(string message) : Exception(message);
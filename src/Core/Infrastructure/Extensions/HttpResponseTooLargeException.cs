namespace MrWatchdog.Core.Infrastructure.Extensions;

public class HttpResponseTooLargeException(string message) : Exception(message);
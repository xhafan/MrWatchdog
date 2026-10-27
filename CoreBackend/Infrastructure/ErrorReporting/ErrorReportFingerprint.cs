using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CoreBackend.Infrastructure.ErrorReporting;

public static class ErrorReportFingerprint
{
    public static string ForException(Exception exception) => _hash(_identity(exception));

    public static string ForMessage(string source, string message) => _hash(new[] { source, message });

    public static string ForFailedMessage(Type messageType, string errorDetails) => ForMessage(
        messageType.FullName ?? messageType.Name,
        Regex.Replace(errorDetails, @"(?m)(^\s+at .+?) in [^\r\n]+:line \d+\s*$", "$1", RegexOptions.CultureInvariant)
    );

    private static ExceptionIdentity _identity(Exception exception)
    {
        var method = new StackTrace(exception, false).GetFrame(0)?.GetMethod();
        return new ExceptionIdentity(
            exception.GetType().FullName ?? exception.GetType().Name,
            exception.Message,
            method == null ? null : $"{method.DeclaringType?.FullName}.{method}",
            exception is HttpRequestException http ? (int?)http.StatusCode : null,
            exception is AggregateException aggregate
                ? aggregate.InnerExceptions.Select(_identity).ToArray()
                : exception.InnerException == null ? [] : [_identity(exception.InnerException)]
        );
    }

    private static string _hash<T>(T value) => Convert.ToHexString(SHA256.HashData(
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))));

    private sealed record ExceptionIdentity(string Type, string Message, string? ThrowingMethod, int? HttpStatusCode, ExceptionIdentity[] Inner);
}

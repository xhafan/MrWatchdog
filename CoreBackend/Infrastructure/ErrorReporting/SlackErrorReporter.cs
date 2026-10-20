using System.Net.Http.Json;
using CoreBackend.Features.Jobs;
using CoreBackend.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace CoreBackend.Infrastructure.ErrorReporting;

public sealed class SlackErrorReporter(
    IHttpClientFactory httpClientFactory,
    IOptions<SlackErrorReportingOptions> iSlackErrorReportingOptions,
    IOptions<RuntimeOptions> iRuntimeOptions
) : IErrorReporter
{
    public const string HttpClientName = nameof(SlackErrorReporter);
    private const int MaximumMessageLength = 4000;
    private const string TruncationNotice = "\n\nDetails truncated; see application logs for the full error.";

    public async Task Report(ErrorReport report, CancellationToken cancellationToken = default)
    {
        var options = iSlackErrorReportingOptions.Value;
        if (string.IsNullOrWhiteSpace(options.Channel))
        {
            return;
        }

        var payload = new Dictionary<string, string>
        {
            ["channel"] = options.Channel,
            ["username"] = $"{options.ApplicationName} errors",
            ["icon_emoji"] = ":warning:",
            ["text"] = _formatReportText(report)
        };

        var httpClient = httpClientFactory.CreateClient(HttpClientName);
        using var response = await httpClient.PostAsJsonAsync(
            options.WebHookUrl,
            payload,
            cancellationToken
        );
        response.EnsureSuccessStatusCode();
    }

    private string _formatReportText(ErrorReport report)
    {
        var metadata = $"RequestId: {_escapeSlackText(report.RequestId)}\n"
                       + $"TimeStamp: {report.OccurredAtUtc:yyyy-MM-dd HH:mm:ss.fff} UTC\n";
        if (report.FailedMessage is { } failure)
        {
            var jobUrl = iRuntimeOptions.Value.Url.TrimEnd('/')
                         + JobUrlConstants.GetJobUrlTemplate.WithJobGuid(failure.JobGuid);
            metadata += $"Job: <{_escapeSlackText(jobUrl)}|{_escapeSlackText(failure.FailedMessageType.Name)}>\n";
        }
        metadata += "Message: ";

        var message = _escapeSlackText(report.Message);
        if (metadata.Length + message.Length <= MaximumMessageLength) return metadata + message;

        return metadata
               + _truncateEscapedText(message, MaximumMessageLength - metadata.Length - TruncationNotice.Length)
               + TruncationNotice;
    }

    private static string _truncateEscapedText(string text, int maximumLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maximumLength);
        if (maximumLength == 0) return string.Empty;

        var length = maximumLength;
        if (char.IsHighSurrogate(text[length - 1])) length--;
        if (length == 0) return string.Empty;

        var entityStart = text.LastIndexOf('&', length - 1);
        if (entityStart >= 0 && text.IndexOf(';', entityStart, length - entityStart) < 0) length = entityStart;

        return text[..length];
    }

    private static string _escapeSlackText(string value) => value
        .Replace("&", "&amp;", StringComparison.Ordinal)
        .Replace("<", "&lt;", StringComparison.Ordinal)
        .Replace(">", "&gt;", StringComparison.Ordinal);
}

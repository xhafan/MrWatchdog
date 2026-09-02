using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace CoreBackend.Infrastructure.ErrorReporting;

public sealed class SlackErrorReporter(
    IHttpClientFactory httpClientFactory,
    IOptions<SlackErrorReportingOptions> iSlackErrorReportingOptions
) : IErrorReporter
{
    public const string HttpClientName = nameof(SlackErrorReporter);

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
            ["username"] = "SmartGuide web app errors",
            ["icon_emoji"] = ":warning:",
            ["text"] = $"""
                       RequestId: {_escapeSlackText(report.RequestId)}
                       TimeStamp: {report.OccurredAtUtc:yyyy-MM-dd HH:mm:ss.fff} UTC
                       Message: {_escapeSlackText(report.Message)}
                       """
        };

        var httpClient = httpClientFactory.CreateClient(HttpClientName);
        using var response = await httpClient.PostAsJsonAsync(
            options.WebHookUrl,
            payload,
            cancellationToken
        );
        response.EnsureSuccessStatusCode();
    }

    private static string _escapeSlackText(string value) => value
        .Replace("&", "&amp;", StringComparison.Ordinal)
        .Replace("<", "&lt;", StringComparison.Ordinal)
        .Replace(">", "&gt;", StringComparison.Ordinal);
}

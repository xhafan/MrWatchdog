using Microsoft.Extensions.Logging;

namespace MrWatchdog.Core.Infrastructure.HttpClients;

// taken from https://stackoverflow.com/a/18925296/379279
public class HttpClientLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpClientLoggingHandler> _logger;

    public HttpClientLoggingHandler(ILogger<HttpClientLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request:\n{Request}\n{Content}", request, request.Content != null
            ? await request.Content.ReadAsStringAsync(cancellationToken)
            : ""
        );

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation("Response:\n{Response}\n{Content}", response, await response.Content.ReadAsStringAsync(cancellationToken));

        return response;
    }
}
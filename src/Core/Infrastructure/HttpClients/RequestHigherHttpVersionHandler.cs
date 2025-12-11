namespace MrWatchdog.Core.Infrastructure.HttpClients;

public class RequestHigherHttpVersionHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        return base.SendAsync(request, cancellationToken);
    }
}
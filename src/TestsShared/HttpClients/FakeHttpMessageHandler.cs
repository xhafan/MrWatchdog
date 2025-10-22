using System.Text.RegularExpressions;
using MrWatchdog.Core.Infrastructure.Extensions;

namespace MrWatchdog.TestsShared.HttpClients;

public class FakeHttpMessageHandler(List<HttpMessageRequestResponse> requestResponses) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestUrl = request.RequestUri!.AbsoluteUri;
        var requestResponse = requestResponses
            .SingleOrDefault(x => (x.RequestHttpMethod == null || x.RequestHttpMethod == request.Method)
                                  && (x.RequestUrlOrRegexUrlPattern == requestUrl || Regex.IsMatch(requestUrl, x.RequestUrlOrRegexUrlPattern))
                                  && (x.RequestHeaders == null
                                      || x.RequestHeaders.AreEquivalent(request.Headers.Select(header => (header.Key, Value: string.Join(",", header.Value)))))
            );
        if (requestResponse != null)
        {
            return Task.FromResult(requestResponse.HttpResponseMessageFunc());
        }
        
        throw new NotImplementedException($"No configured response for {request.Method} {requestUrl}");
    }
}
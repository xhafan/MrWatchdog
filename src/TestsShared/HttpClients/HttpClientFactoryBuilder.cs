using CoreUtils.Extensions;

namespace MrWatchdog.TestsShared.HttpClients;

public class HttpClientFactoryBuilder
{
    private HttpClient? _httpClient;
    private readonly List<HttpMessageRequestResponse> _requestResponses = [];

    public HttpClientFactoryBuilder WithHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        return this;
    }
    
    public HttpClientFactoryBuilder WithRequestResponse(HttpMessageRequestResponse requestResponse)
    {
        _requestResponses.Add(requestResponse);
        return this;
    }
    
    public HttpClientFactory Build()
    {
        if (_requestResponses.IsEmpty())
        {
            return new HttpClientFactory(_httpClient);
        }
        
        return new HttpClientFactory(new HttpClient(new FakeHttpMessageHandler(_requestResponses)));
    }
}
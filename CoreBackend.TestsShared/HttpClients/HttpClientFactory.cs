namespace CoreBackend.TestsShared.HttpClients;

public class HttpClientFactory(HttpClient? httpClient = null) : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return httpClient ?? new HttpClient();
    }
}
namespace MrWatchdog.TestsShared.HttpClients;

public record HttpMessageRequestResponse(
    string RequestUrlOrRegexUrlPattern,
    Func<HttpResponseMessage> HttpResponseMessageFunc,
    HttpMethod? RequestHttpMethod = null,
    IEnumerable<(string Name, string Value)>? RequestHeaders = null
);

using MrWatchdog.Core.Features.Watchdogs;
using System.Net;
using System.Text.RegularExpressions;

namespace MrWatchdog.Web.Tests.Features;

public static partial class E2ETestHelper
{
    [GeneratedRegex("__RequestVerificationToken\"[^>]+value=\"(?<token>[a-zA-Z0-9-_]+)\"")]
    private static partial Regex RequestVerificationTokenRegex();

    public static string ExtractRequestVerificationToken(string html)
    {
        return RequestVerificationTokenRegex().Match(html).Groups["token"].Value;
    }

    public static FormUrlEncodedContent GetFormUrlEncodedContentWithRequestVerificationToken(string requestVerificationToken)
    {
        return new FormUrlEncodedContent(new Dictionary<string, string>
        {
            {"__RequestVerificationToken", requestVerificationToken},
        });
    }

    public static async Task LogUserIn(HttpClient webApplicationClient, Guid loginTokenGuid)
    {
        var response = await webApplicationClient.PostAsync(
            AccountUrlConstants.ApiCompleteLoginUrlTemplate.WithLoginTokenGuid(loginTokenGuid), content: null);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
    }
}
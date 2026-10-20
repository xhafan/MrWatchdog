using CoreUtils;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;
using CoreBackend.Infrastructure;
using CoreBackend.Infrastructure.Jsons;
using CoreBackend.Infrastructure.ErrorReporting;
using Microsoft.Extensions.DependencyInjection;
using CoreBackend.TestsShared;
using CoreWeb.Features.Logs;

namespace MrWatchdog.Web.E2E.Tests.Features.Logs;

[TestFixture]
public class when_logging_error : BaseDatabaseTest
{
    private readonly string _jsErrorMessage = $"JS error message {Guid.NewGuid()}";

    [Test]
    public void error_notifications_are_disabled_for_the_shared_test_host()
    {
        var queue = RunOncePerTestRun.WebApplicationFactory.Value.Services.GetRequiredService<ErrorReportQueue>();

        queue.TryEnqueue(new ErrorReport("Test error", "test-request", DateTime.UtcNow, Origin: ErrorReportOrigin.Server)).ShouldBeFalse();
    }

    [Test]
    public async Task error_message_can_be_logged_unauthenticated_with_a_secret_in_header()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<when_logging_error>()
            .AddEnvironmentVariables()
            .Build();

        var logErrorApiSecret = config["Logging:LogErrorApiSecret"];
        Guard.Hope(logErrorApiSecret != null, nameof(logErrorApiSecret) + " is null");

        var request = new HttpRequestMessage(HttpMethod.Post, LogsUrlConstants.LogErrorUrl);
        request.Headers.Add(LogConstants.LogErrorApiSecretHeaderName, logErrorApiSecret);
        request.Content = new StringContent(
            JsonHelper.Serialize(_jsErrorMessage),
            Encoding.UTF8,
            "application/json"
        );
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.SendAsync(request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task error_message_cannot_be_logged_without_a_secret_in_header()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, LogsUrlConstants.LogErrorUrl);
        request.Content = new StringContent(
            JsonHelper.Serialize(_jsErrorMessage),
            Encoding.UTF8,
            "application/json"
        );
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.SendAsync(request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
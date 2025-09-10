using System.Net;
using System.Text;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.Tests.Features.Logs.E2e;

[TestFixture]
public class when_logging_error : BaseDatabaseTest
{
    [Test]
    public async Task error_message_can_be_logged_unauthenticated()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.PostAsync("/api/Logs/LogError",
            content: new StringContent(
                JsonHelper.Serialize("JS error message"),
                Encoding.UTF8,
                "application/json"
            )
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Infrastructure.Rebus.RebusQueueRedirectors;

namespace MrWatchdog.Web.Tests.Infrastructure.Rebus.HttpContextRebusQueueRedirectors;

[TestFixture]
public class when_getting_queue_for_redirection
{
    private string? _queueForRedirection;

    [SetUp]
    public void Context()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[CustomHeaders.QueueForRedirection] = RebusQueues.AdminBulk;

        var httpContextAccessor = A.Fake<IHttpContextAccessor>();
        A.CallTo(() => httpContextAccessor.HttpContext).Returns(httpContext);
        
        var webHostEnvironment = A.Fake<IWebHostEnvironment>();
        A.CallTo(() => webHostEnvironment.EnvironmentName).Returns("Test");

        var httpContextRebusQueueRedirector = new HttpContextRebusQueueRedirector(httpContextAccessor, webHostEnvironment);

        _queueForRedirection = httpContextRebusQueueRedirector.GetQueueForRedirection();
    }

    [Test]
    public void queue_for_redirection_is_correct()
    {
        _queueForRedirection.ShouldBe($"Test{RebusQueues.AdminBulk}");
    }
}
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Infrastructure.Rebus.RebusQueueRedirectors;

namespace MrWatchdog.Web.Tests.Infrastructure.Rebus.HttpContextRebusQueueRedirectors;

[TestFixture]
public class when_getting_queue_for_redirection_with_invalid_queue_name_in_http_header
{
    private HttpContextRebusQueueRedirector _httpContextRebusQueueRedirector = null!;

    [SetUp]
    public void Context()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[CustomHeaders.QueueForRedirection] = "SomeQueueName";

        var httpContextAccessor = A.Fake<IHttpContextAccessor>();
        A.CallTo(() => httpContextAccessor.HttpContext).Returns(httpContext);
        
        var webHostEnvironment = A.Fake<IWebHostEnvironment>();
        A.CallTo(() => webHostEnvironment.EnvironmentName).Returns("Test");

        _httpContextRebusQueueRedirector = new HttpContextRebusQueueRedirector(httpContextAccessor, webHostEnvironment);
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(() => _httpContextRebusQueueRedirector.GetQueueForRedirection());

        ex.Message.ShouldBe("Unsupported Rebus queue SomeQueueName.");
    }
}
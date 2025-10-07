using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MrWatchdog.Web.Infrastructure.Rebus.RebusQueueRedirectors;

namespace MrWatchdog.Web.Tests.Infrastructure.Rebus.HttpContextRebusQueueRedirectors;

[TestFixture]
public class when_getting_queue_for_redirection_without_http_context
{
    private string? _queueForRedirection;

    [SetUp]
    public void Context()
    {
        var httpContextAccessor = A.Fake<IHttpContextAccessor>();
        A.CallTo(() => httpContextAccessor.HttpContext).Returns(null);
        
        var httpContextRebusQueueRedirector = new HttpContextRebusQueueRedirector(httpContextAccessor, A.Fake<IWebHostEnvironment>());

        _queueForRedirection = httpContextRebusQueueRedirector.GetQueueForRedirection();
    }

    [Test]
    public void queue_for_redirection_is_empty()
    {
        _queueForRedirection.ShouldBe(null);
    }
}
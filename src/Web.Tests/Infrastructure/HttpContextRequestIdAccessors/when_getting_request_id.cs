using FakeItEasy;
using Microsoft.AspNetCore.Http;
using MrWatchdog.Web.Infrastructure.RequestIdAccessors;

namespace MrWatchdog.Web.Tests.Infrastructure.HttpContextRequestIdAccessors;

[TestFixture]
public class when_getting_request_id
{
    private string? _requestId;

    [SetUp]
    public void Context()
    {
        var httpContextAccessor = A.Fake<IHttpContextAccessor>();
        A.CallTo(() => httpContextAccessor.HttpContext)
            .Returns(new DefaultHttpContext
            {
                TraceIdentifier = "0HNFBP8T98MQS:00000045"
            });
        var requestIdAccessor = new HttpContextRequestIdAccessor(httpContextAccessor);

        _requestId = requestIdAccessor.GetRequestId();
    }

    [Test]
    public void request_id_is_correct()
    {
        _requestId.ShouldBe("0HNFBP8T98MQS:00000045");
    }
}
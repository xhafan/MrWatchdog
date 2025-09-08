using FakeItEasy;
using Microsoft.AspNetCore.Http;
using MrWatchdog.Web.Infrastructure.RequestIdAccessors;

namespace MrWatchdog.Web.Tests.Infrastructure.HttpContextRequestIdAccessors;

[TestFixture]
public class when_getting_request_id_without_http_context
{
    private string? _requestId;

    [SetUp]
    public void Context()
    {
        var httpContextAccessor = A.Fake<IHttpContextAccessor>();
        A.CallTo(() => httpContextAccessor.HttpContext).Returns(null);
        var actingUserAccessor = new HttpContextRequestIdAccessor(httpContextAccessor);

        _requestId = actingUserAccessor.GetRequestId();
    }

    [Test]
    public void request_id_is_empty()
    {
        _requestId.ShouldBe(null);
    }
}
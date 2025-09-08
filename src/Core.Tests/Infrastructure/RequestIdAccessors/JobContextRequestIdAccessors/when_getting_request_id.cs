using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.RequestIdAccessors;

namespace MrWatchdog.Core.Tests.Infrastructure.RequestIdAccessors.JobContextRequestIdAccessors;

[TestFixture]
public class when_getting_request_id
{
    private string? _requestId;

    [SetUp]
    public void Context()
    {
        JobContext.RequestId.Value = "0HNFBP8T98MQS:00000045";
        var actingUserAccessor = new JobContextRequestIdAccessor();

        _requestId = actingUserAccessor.GetRequestId();
    }

    [Test]
    public void request_id_is_correct()
    {
        _requestId.ShouldBe("0HNFBP8T98MQS:00000045");
    }
}
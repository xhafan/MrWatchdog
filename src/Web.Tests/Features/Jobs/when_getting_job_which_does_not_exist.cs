using Microsoft.AspNetCore.Mvc;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.Tests.Features.Jobs;

[TestFixture]
public class when_getting_job_which_does_not_exist : BaseDatabaseTest
{
    private readonly Guid _nonExistentJobGuid = Guid.NewGuid();

    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        var controller = new JobsControllerBuilder(UnitOfWork)
            .Build();

        _actionResult = await controller.GetJob(_nonExistentJobGuid);
    }

    [Test]
    public void job_is_not_found()
    {
        _actionResult.ShouldBeOfType<NotFoundResult>();
    }
}
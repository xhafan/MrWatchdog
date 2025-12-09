using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Account;

namespace MrWatchdog.Web.Tests.Features.Account;

[TestFixture]
public class when_completing_onboarding : BaseDatabaseTest
{
    private UsersController _controller = null!;
    private ICoreBus _bus = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<ICoreBus>();

        _controller = new UsersController(_bus);

        _actionResult = await _controller.CompleteOnboarding(OnboardingIdentifiers.ScraperScrapingResults);
    } 

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new CompleteOnboardingCommand(OnboardingIdentifiers.ScraperScrapingResults))).MustHaveHappenedOnceExactly();
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<OkObjectResult>();
        var okObjectResult = (OkObjectResult) _actionResult;
        var value = okObjectResult.Value;
        value.ShouldBeOfType<string>();
        var jobGuid = (string) value;
        jobGuid.ShouldMatch(@"[0-9A-Fa-f\-]{36}");
    }
}
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.Overview;
using MrWatchdog.Web.Infrastructure.Authorizations;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Overview;

[TestFixture]
public class when_updating_watchdog_detail_overview_as_unauthorized_user : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private OverviewModel _model = null!;
    private ICoreBus _bus = null!;    
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        _bus = A.Fake<ICoreBus>();
        
        var authorizationService = A.Fake<IAuthorizationService>();
        A.CallTo(() => authorizationService.AuthorizeAsync(
                A<ClaimsPrincipal>._,
                _watchdog.Id,
                A<IAuthorizationRequirement[]>.That.Matches(p => p.OfType<WatchdogOwnerOrSuperAdminRequirement>().Any())
            ))
            .Returns(AuthorizationResult.Failed());

        _model = new OverviewModelBuilder(UnitOfWork)
            .WithAuthorizationService(authorizationService)
            .WithBus(_bus)
            .Build();

        _model.WatchdogOverviewArgs = new WatchdogOverviewArgs
        {
            WatchdogId = _watchdog.Id,
            Name = "watchdog updated name",
            ScrapingIntervalInSeconds = 60,
            IntervalBetweenSameResultNotificationsInDays = 30
        };

        _actionResult = await _model.OnPost();
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<ForbidResult>();
    }

    [Test]
    public void command_not_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new UpdateWatchdogOverviewCommand(_model.WatchdogOverviewArgs)))
            .MustNotHaveHappened();
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }    
}
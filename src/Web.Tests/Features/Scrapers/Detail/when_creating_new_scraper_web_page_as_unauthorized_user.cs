using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail;
using MrWatchdog.Web.Infrastructure.Authorizations;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail;

[TestFixture]
public class when_creating_new_watchdog_web_page_as_unauthorized_user : BaseDatabaseTest
{
    private DetailModel _model = null!;
    private Watchdog _watchdog = null!;
    private ICoreBus _bus = null!;
    private IActionResult _actionResult = null!;

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
        
        _model = new DetailModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithAuthorizationService(authorizationService)
            .Build();
        
        _actionResult = await _model.OnPostCreateWatchdogWebPage(_watchdog.Id);
    }

    [Test]
    public void command_is_not_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new CreateWatchdogWebPageCommand(_watchdog.Id))).MustNotHaveHappened();
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<ForbidResult>();
    }
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }    
}
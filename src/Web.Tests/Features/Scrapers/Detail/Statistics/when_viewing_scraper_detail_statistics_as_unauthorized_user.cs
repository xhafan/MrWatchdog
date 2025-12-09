using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.Statistics;
using MrWatchdog.Web.Infrastructure.Authorizations;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Statistics;

[TestFixture]
public class when_viewing_watchdog_detail_statistics_as_unauthorized_user : BaseDatabaseTest
{
    private StatisticsModel _model = null!;
    private Watchdog _watchdog = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        var authorizationService = A.Fake<IAuthorizationService>();
        A.CallTo(() => authorizationService.AuthorizeAsync(
                A<ClaimsPrincipal>._,
                _watchdog.Id,
                A<IAuthorizationRequirement[]>.That.Matches(p => p.OfType<WatchdogOwnerOrSuperAdminRequirement>().Any())
            ))
            .Returns(AuthorizationResult.Failed());
        
        _model = new StatisticsModelBuilder(UnitOfWork)
            .WithAuthorizationService(authorizationService)
            .Build();
        
        _actionResult = await _model.OnGet(_watchdog.Id);
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
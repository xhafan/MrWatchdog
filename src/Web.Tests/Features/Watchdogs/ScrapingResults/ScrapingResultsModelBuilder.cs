using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.ScrapingResults;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.ScrapingResults;

public class ScrapingResultsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private User? _actingUser;
    private IAuthorizationService? _authorizationService;

    public ScrapingResultsModelBuilder WithActingUser(User actingUser)
    {
        _actingUser = actingUser;
        return this;
    }

    public ScrapingResultsModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }


    public ScrapingResultsModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogScrapingResultsArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));

        var actingUserAccessor = A.Fake<IActingUserAccessor>();
        if (_actingUser != null)
        {
            A.CallTo(() => actingUserAccessor.GetActingUserId()).Returns(_actingUser.Id);
        }
        
        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }

        var model = new ScrapingResultsModel(
            new QueryExecutor(queryHandlerFactory),
            actingUserAccessor,
            _authorizationService
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}
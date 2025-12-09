using System.Security.Claims;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Detail.Badges;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Badges;

public class BadgesModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private IAuthorizationService? _authorizationService;

    public BadgesModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }

    public BadgesModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogDetailArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));
        
        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }

        var model = new BadgesModel(
            new QueryExecutor(queryHandlerFactory),
            _authorizationService
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}
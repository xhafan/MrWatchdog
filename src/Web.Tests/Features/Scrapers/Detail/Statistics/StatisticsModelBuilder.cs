using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Detail.Statistics;
using System.Security.Claims;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Statistics;

public class StatisticsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private IAuthorizationService? _authorizationService;

    public StatisticsModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }
    
    public StatisticsModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetPublicWatchdogStatisticsQueryHandler(unitOfWork));

        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogDetailPublicStatusArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));

        
        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }

        var model = new StatisticsModel(
            new QueryExecutor(queryHandlerFactory),
            _authorizationService
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}
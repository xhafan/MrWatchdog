using System.Security.Claims;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Search.Overview;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Search.Overview;

public class OverviewModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IAuthorizationService? _authorizationService;

    private WatchdogSearchOverviewArgs? _watchdogSearchOverviewArgs;

    public OverviewModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }

    public OverviewModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }

    public OverviewModelBuilder WithWatchdogSearchOverviewArgs(WatchdogSearchOverviewArgs watchdogSearchOverviewArgs)
    {
        _watchdogSearchOverviewArgs = watchdogSearchOverviewArgs;
        return this;
    }
    
    public OverviewModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogSearchOverviewArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<WatchdogSearch>(unitOfWork)
        ));

        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }
        
        var model = new OverviewModel(
            new QueryExecutor(queryHandlerFactory),
            _bus,
            _authorizationService
        )
        {
            WatchdogSearchOverviewArgs = _watchdogSearchOverviewArgs ?? new WatchdogSearchOverviewArgs
            {
                WatchdogSearchId = 0,
                ReceiveNotification = true,
                SearchTerm = null
            }
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}
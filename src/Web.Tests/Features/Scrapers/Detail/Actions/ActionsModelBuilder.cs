using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Detail.Actions;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Actions;

public class ActionsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IAuthorizationService? _authorizationService;

    public ActionsModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }

    public ActionsModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }
    
    public ActionsModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
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
        
        var model = new ActionsModel(
            new QueryExecutor(queryHandlerFactory),
            _bus,
            _authorizationService
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}
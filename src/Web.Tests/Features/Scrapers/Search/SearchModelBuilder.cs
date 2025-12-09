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
using MrWatchdog.Web.Features.Watchdogs.Search;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Search;

public class SearchModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IAuthorizationService? _authorizationService;

    public SearchModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }

    public SearchModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }
    
    public SearchModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogSearchArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<WatchdogSearch>(unitOfWork)
        ));
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogScrapingResultsArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));

        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }

        var model = new SearchModel(
            new QueryExecutor(queryHandlerFactory),
            _bus,
            _authorizationService
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}
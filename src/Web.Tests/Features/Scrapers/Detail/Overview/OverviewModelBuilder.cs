using System.Security.Claims;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Scrapers.Detail.Overview;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.Overview;

public class OverviewModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IAuthorizationService? _authorizationService;

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
    
    public OverviewModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetScraperOverviewArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Scraper>(unitOfWork)
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
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}
using System.Security.Claims;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Scrapers.Detail.Statistics;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.Statistics;

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
        
        queryHandlerFactory.RegisterQueryHandler(new GetPublicScraperStatisticsQueryHandler(unitOfWork));

        queryHandlerFactory.RegisterQueryHandler(new GetScraperDetailPublicStatusArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Scraper>(unitOfWork)
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
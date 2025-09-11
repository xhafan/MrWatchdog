using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.ScrapingResults;

public class ScrapingResultsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    public ScrapingResultsModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogScrapingResultsArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));
        
        var model = new ScrapingResultsModel(
            new QueryExecutor(queryHandlerFactory)
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}
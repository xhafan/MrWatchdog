using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

public class WebPageSelectedElementsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    public WebPageSelectedElementsModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogWebPageSelectedElementsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));

        var model = new WebPageSelectedElementsModel(
            new QueryExecutor(queryHandlerFactory)
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}
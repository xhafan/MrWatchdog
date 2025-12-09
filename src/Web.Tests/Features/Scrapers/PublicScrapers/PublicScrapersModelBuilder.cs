using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Scrapers.PublicScrapers;

namespace MrWatchdog.Web.Tests.Features.Scrapers.PublicScrapers;

public class PublicScrapersModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    public PublicScrapersModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetPublicScrapersQueryHandler(unitOfWork));

        var model = new PublicScrapersModel(
            new QueryExecutor(queryHandlerFactory)
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}
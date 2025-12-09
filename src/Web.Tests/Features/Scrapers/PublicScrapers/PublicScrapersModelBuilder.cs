using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.PublicWatchdogs;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.PublicWatchdogs;

public class PublicWatchdogsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    public PublicWatchdogsModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetPublicWatchdogsQueryHandler(unitOfWork));

        var model = new PublicWatchdogsModel(
            new QueryExecutor(queryHandlerFactory)
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}
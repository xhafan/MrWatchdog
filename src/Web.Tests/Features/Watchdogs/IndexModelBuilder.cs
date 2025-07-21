using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs;

namespace MrWatchdog.Web.Tests.Features.Watchdogs;

public class IndexModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    public IndexModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogsQueryHandler(unitOfWork));
        
        var model = new IndexModel(new QueryExecutor(queryHandlerFactory));
        ModelValidator.ValidateModel(model);
        return model;
    }
}
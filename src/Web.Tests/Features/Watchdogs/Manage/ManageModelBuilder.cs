using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Manage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Manage;

public class ManageModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    public ManageModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogsQueryHandler(unitOfWork));
        
        var model = new ManageModel(new QueryExecutor(queryHandlerFactory));
        ModelValidator.ValidateModel(model);
        return model;
    }
}
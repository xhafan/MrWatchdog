using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Alerts;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Alerts;

public class AlertsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    public AlertsModel Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogAlertsQueryHandler(unitOfWork));
        
        var model = new AlertsModel(new QueryExecutor(queryHandlerFactory));
        ModelValidator.ValidateModel(model);
        return model;
    }
}
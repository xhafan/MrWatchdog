using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Detail;
using Rebus.Bus;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail;

public class DetailModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private IBus? _bus;
    
    public DetailModelBuilder WithBus(IBus bus)
    {
        _bus = bus;
        return this;
    }    
    
    public DetailModel Build()
    {
        _bus ??= A.Fake<IBus>();
        
        var model = new DetailModel(
            new QueryExecutor(
                new FakeQueryHandlerFactory<GetWatchdogArgsQuery>(
                    new GetWatchdogArgsQueryHandler(
                        unitOfWork,
                        new NhibernateRepository<Watchdog>(unitOfWork)
                    )
                )
            ),
            _bus
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}
﻿using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Detail;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail;

public class DetailModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    
    public DetailModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }    
    
    public DetailModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));
        
        var model = new DetailModel(
            new QueryExecutor(queryHandlerFactory),
            _bus
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}
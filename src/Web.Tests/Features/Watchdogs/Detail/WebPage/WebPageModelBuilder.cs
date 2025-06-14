﻿using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;
using Rebus.Bus;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

public class WebPageModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private IBus? _bus;
    private WatchdogWebPageArgs? _watchdogWebPageArgs;

    public WebPageModelBuilder WithBus(IBus bus)
    {
        _bus = bus;
        return this;
    }
    
    public WebPageModelBuilder WithWatchdogWebPageArgs(WatchdogWebPageArgs watchdogWebPageArgs)
    {
        _watchdogWebPageArgs = watchdogWebPageArgs;
        return this;
    }       
    
    public WebPageModel Build()
    {
        _bus ??= A.Fake<IBus>();
        
        var model = new WebPageModel(
            new QueryExecutor(
                new FakeQueryHandlerFactory<GetWatchdogWebPageArgsQuery>(
                    new GetWatchdogWebPageArgsQueryHandler(
                        unitOfWork,
                        new NhibernateRepository<Watchdog>(unitOfWork)
                    )
                )
            ),
            _bus
        )
        {
            WatchdogWebPageArgs = _watchdogWebPageArgs!
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}